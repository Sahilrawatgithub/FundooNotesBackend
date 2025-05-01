using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

//using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using ModelLayer.Entity;
using RepositoryLayer.Context;
using RepositoryLayer.DTO;
using RepositoryLayer.Interface;
using StackExchange.Redis;

namespace RepositoryLayer.Service
{
    public class LabelImplRL : ILabelRL
    {
        private readonly UserContext _userContext;
        private readonly ILogger<LabelImplRL> _logger;
        private readonly IDatabase _redisDatabase;
        public LabelImplRL(UserContext userContext, ILogger<LabelImplRL> logger, IConnectionMultiplexer redis)
        {
            _userContext = userContext;
            _logger = logger;
            _redisDatabase = redis.GetDatabase();
        }

        public async Task<ResponseDTO<LabelEntity>> CreateLabelAsync(string labelName, int userId)
        {
            try
            {
                _logger.LogInformation($"Attempting creation of label on user id :{userId}");
                var label = new LabelEntity
                {
                    LabelName = labelName,
                    UserId = userId
                };
                await _userContext.Labels.AddAsync(label);
                await _userContext.SaveChangesAsync();
                _logger.LogInformation($"Created label: {label}");
                await CacheSingleLabel(label);
                await InvalidateAllUserLabels(userId); 
                await CacheAllUserLabels(userId); 
                return new ResponseDTO<LabelEntity>
                {
                    Success = true,
                    Message = "Label created successfully",
                    Data = label
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating label");
                return new ResponseDTO<LabelEntity>
                {
                    Success = false,
                    Message = "Error creating label",
                    Data = null
                };
            }
        }

        public async Task<ResponseDTO<string>> DeleteLabelAsync(string labelName, int userId)
        {
            using var transactions = await _userContext.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation($"Attempting deletion of label on user id :{userId}");
                var label = await _userContext.Labels.FirstOrDefaultAsync(l => l.LabelName == labelName && l.UserId == userId);
                if (label == null)
                {
                    return new ResponseDTO<string>
                    {
                        Success = false,
                        Message = "Label not found"
                    };
                }
                _userContext.Labels.Remove(label);              
                await _userContext.SaveChangesAsync();
                _logger.LogInformation($"Deleted label: {labelName} for user id :{userId}");
                await InvalidateSingleLabel(label.LabelId, userId); 
                await InvalidateAllUserLabels(userId);
                await CacheAllUserLabels(userId);
                await transactions.CommitAsync();

                return new ResponseDTO<string>
                {
                    Success = true,
                    Message = "Label deleted successfully"
                };
            }
            catch (Exception ex)
            {
                await transactions.RollbackAsync();
                _logger.LogError(ex, "Error deleting label");
                return new ResponseDTO<string>
                {
                    Success = false,
                    Message = "Error deleting label",
                    Data = null
                };
            }
        }

        public async Task<ResponseDTO<string>> AddLabelToNoteAsync(string labelName, int noteId, int userId)
        {
            var transactions = await _userContext.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Attempting addition of label to a note");
                var label = await _userContext.Labels.Include(l => l.LabelNotes).FirstOrDefaultAsync(n => n.LabelName == labelName && n.UserId == userId);
                if (label == null)
                {
                    return new ResponseDTO<string>
                    {
                        Success = false,
                        Message = "Label not found"
                    };
                }
                var note = await _userContext.Notes.FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);
                if (note == null)
                {
                    return new ResponseDTO<string>
                    {
                        Success = false,
                        Message = "Note not found"
                    };
                }
                if (label.LabelNotes.Any(n => n.NoteId == noteId))
                {
                    return new ResponseDTO<string>
                    {
                        Success = false,
                        Message = "Label already exists for this note"
                    };
                }
                var newLabel = new NoteLabelEntity
                {
                    LabelId= userId,
                    NoteId= noteId
                };
                note.LabelNotes.Add(newLabel);
                await _userContext.SaveChangesAsync();
                await CacheSingleLabelNote(newLabel);
                await transactions.CommitAsync();
                _logger.LogInformation("Label addded to note successfully");
                return new ResponseDTO<string>
                {
                    Success = true,
                    Message = "Label added to note successfully"
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding label to note");
                return new ResponseDTO<string>
                {
                    Success = false,
                    Message = "Error adding label to note",
                    Data = null
                };
            }
        }

        public async Task<ResponseDTO<List<LabelEntity>>> GetAllLabelsAsync(int userId)
        {
            var transactions = await _userContext.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation($"Attempting retrieval of all labels for user id :{userId}");
                var cacheKey = $"AllLabels_User_{userId}";
                var cachedLabels = await _redisDatabase.StringGetAsync(cacheKey);
                if(cachedLabels.HasValue)
                {
                    _logger.LogInformation("Labels retrieved from cache successfully");
                   var labelss = JsonSerializer.Deserialize<List<LabelEntity>>(cachedLabels);
                    return new ResponseDTO<List<LabelEntity>>
                    {
                        Success = true,
                        Message = "Labels retrieved successfully",
                        Data = labelss
                    };
                }
                var labels = await _userContext.Labels.Where(n => n.UserId == userId).ToListAsync();
                if (labels == null || labels.Count == 0)
                {
                    return new ResponseDTO<List<LabelEntity>>
                    {
                        Success = false,
                        Message = "No labels found"
                    };
                }
                _logger.LogInformation($"Retrieved all labels for user id :{userId}");
                await CacheAllUserLabels(userId);
                await transactions.CommitAsync();
                return new ResponseDTO<List<LabelEntity>>
                {
                    Success = true,
                    Message = "Labels retrieved successfully",
                    Data = labels
                };
            }
            catch (Exception ex)
            {
                await transactions.RollbackAsync();
                _logger.LogError(ex, "Error retrieving all labels");
                return new ResponseDTO<List<LabelEntity>>
                {
                    Success = false,
                    Message = "Error retrieving all labels"
                };
            }
        }

        public async Task<ResponseDTO<LabelEntity>> GetLabelByIdAsync(int labelId, int userId)
        {
            var transactions = await _userContext.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation($"Retrieving user for LabelID: {labelId} and UserID: {userId}");
                var label = await _userContext.Labels.FirstOrDefaultAsync(l => l.UserId == userId && l.LabelId == labelId);
                if (label != null)
                {
                    _logger.LogInformation("Label fetched from database for LabelID: {LabelId}", labelId);
                    await CacheSingleLabel(label);
                    await transactions.CommitAsync();

                    return new ResponseDTO<LabelEntity>
                    {
                        Success = true,
                        Message = "Label fetched successfully",
                        Data = label
                    };
                }
                _logger.LogWarning("No label found for LabelID: {LabelId} and UserID: {UserId}", labelId, userId);
                return new ResponseDTO<LabelEntity>
                {
                    Success = false,
                    Message = "No label found",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user with ID: {UserId}", userId);
                return new ResponseDTO<LabelEntity>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the user.",
                    Data = null
                };
            }
        }
        public async Task<ResponseDTO<LabelEntity>> UpdateLabelAsync(string oldLabelName, string newLabelName, int userId)
        {
            var transactions = await _userContext.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation($"Attempting update of label for user id :{userId}");
                var label = await _userContext.Labels.FirstOrDefaultAsync(l => l.LabelName == oldLabelName && l.UserId == userId);
                if (label == null)
                {
                    return new ResponseDTO<LabelEntity>
                    {
                        Success = false,
                        Message = "Label not found"
                    };
                }
                label.LabelName = newLabelName;
                await _userContext.SaveChangesAsync();
                await InvalidateSingleLabel(label.LabelId, userId);
                await CacheSingleLabel(label);
                await InvalidateAllUserLabels(userId);
                await CacheAllUserLabels(userId);
                await transactions.CommitAsync();
                _logger.LogInformation($"Updated label: {oldLabelName} to {newLabelName} for user id :{userId}");
                return new ResponseDTO<LabelEntity>
                {
                    Success = true,
                    Message = "Label updated successfully",
                    Data = label
                };
            }
            catch (Exception ex)
            {
                await transactions.RollbackAsync();
                _logger.LogError(ex, "Error updating label");
                return new ResponseDTO<LabelEntity>
                {
                    Success = false,
                    Message = "Error updating label",
                    Data = null
                };
            }
        }
        private async Task InvalidateSingleLabel(int labelId, int userId)
        {
            string cacheKey = $"Label_{labelId}_User_{userId}";
            await _redisDatabase.KeyDeleteAsync(cacheKey);
        }

        private async Task InvalidateAllUserLabels(int userId)
        {
            string cacheKey = $"AllLabels_User_{userId}";
            await _redisDatabase.KeyDeleteAsync(cacheKey);
        }

        private async Task CacheSingleLabel(LabelEntity label)
        {
            string cacheKey = $"Label_{label.LabelId}_User_{label.UserId}";
            var serializedLabel = JsonSerializer.Serialize(label);
            TimeSpan expiration = TimeSpan.FromMinutes(10);

            await _redisDatabase.StringSetAsync(cacheKey, serializedLabel, expiration);
        }

        private async Task CacheSingleLabelNote(NoteLabelEntity noteLabel)
        {
            string cacheKey = $"Label_{noteLabel.LabelId}_Note{noteLabel.NoteId}";
            var serializedLabel = JsonSerializer.Serialize(noteLabel);
            TimeSpan expiration = TimeSpan.FromMinutes(10);

            await _redisDatabase.StringSetAsync(cacheKey, serializedLabel, expiration);
        }

        private async Task CacheAllUserLabels(int userId)
        {
            var allLabels = await _userContext.Labels.Where(n => n.UserId == userId).ToListAsync();
            var serializedLabel = JsonSerializer.Serialize(allLabels); // System.Text.Json

            string cacheKey = $"AllLabels_User_{userId}";
            TimeSpan expiration = TimeSpan.FromMinutes(10);

            await _redisDatabase.StringSetAsync(cacheKey, serializedLabel, expiration);
        }

    }
}
