using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
//using Microsoft.EntityFrameworkCore.Storage;
using ModelLayer.Entity;
using RepositoryLayer.Context;
using RepositoryLayer.DTO;
using RepositoryLayer.Interface;
using StackExchange.Redis;

namespace RepositoryLayer.Service
{
    public class CollabImplRL : ICollabRL
    {
        private readonly UserContext _userContext;
        private readonly IDatabase _redisDatabase;
        private readonly ILogger<CollabImplRL> _logger;

        public CollabImplRL(UserContext userContext, ILogger<CollabImplRL> logger, IConnectionMultiplexer redis)
        {
            _userContext = userContext;
            _logger = logger;
            _redisDatabase = redis.GetDatabase();
        }
        public async Task<ResponseDTO<string>> AddCollabToNoteAsync(CollabDTO request, int userId)
        {
            using var transaction = _userContext.Database.BeginTransaction();
            try
            {
                _logger.LogInformation("Attempting collaborator to note with ID: {NoteId} for user with ID: {UserId}", request.NoteId, userId);
                var note = _userContext.Notes.FirstOrDefault(x => x.NoteId == request.NoteId && x.UserId == userId);
                if (note != null)
                {
                    var user = _userContext.Users.FirstOrDefault(u => u.Email == request.CollabEmail);
                    if (user == null)
                    {
                        _logger.LogWarning("Collaborator email {EmailId} is not registered", request.CollabEmail);
                        return new ResponseDTO<string>
                        {
                            Success = false,
                            Message = "Collaborator email is not registered"
                        };
                    }
                    bool exists = _userContext.Collaborators.Any(c => c.NoteId == request.NoteId && c.CollabEmail == request.CollabEmail && c.UserId == userId);
                    if (exists)
                    {
                        _logger.LogWarning("Collaborator with email {EmailId} already added to note with ID: {NoteId}", request.CollabEmail, request.NoteId);
                        
                        return new ResponseDTO<string>
                        {
                            Success = false,
                            Message = "Collaborator already added to this note"
                        };
                    }

                    var collab = new CollaboratorEntity
                    {
                        NoteId = request.NoteId,
                        CollabEmail = request.CollabEmail,
                        UserId = userId
                    };
                    _userContext.Collaborators.Add(collab);
                    await _userContext.SaveChangesAsync();
                    _logger.LogInformation("Collaborator with email {EmailId} added to note with ID: {NoteId}", request.CollabEmail, request.NoteId);
                    await transaction.CommitAsync();
                    return new ResponseDTO<string>
                    {
                        Success = true,
                        Message = "Collaboration added successfully"
                    };
                }
                else
                {
                    return new ResponseDTO<string>
                    {
                        Success = false,
                        Message = "Note not found"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while adding collaborator: {Message}", ex.Message);
                await transaction.RollbackAsync();
                return new ResponseDTO<string>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ResponseDTO<string>> RemoveCollaboratorAsync(CollabDTO request, int userId)
        {
            using var transaction = _userContext.Database.BeginTransaction();
            try
            {
                _logger.LogInformation("Attempting to remove collaborator with email {EmailId} from note with ID: {NoteId} for user with ID: {UserId}", request.CollabEmail, request.NoteId, userId);
                var note = _userContext.Notes.FirstOrDefault(x => x.NoteId == request.NoteId && x.UserId == userId);
                if (note != null)
                {
                    var collab = _userContext.Collaborators.FirstOrDefault(c => c.NoteId == request.NoteId && c.CollabEmail == request.CollabEmail && c.UserId == userId);
                    if (collab != null)
                    {
                        _userContext.Collaborators.Remove(collab);
                        await _userContext.SaveChangesAsync();
                        _logger.LogInformation("Collaborator with email {EmailId} removed from note with ID: {NoteId}", request.CollabEmail, request.NoteId);
                        await transaction.CommitAsync();
                        return new ResponseDTO<string>
                        {
                            Success = true,
                            Message = "Collaborator removed successfully"
                        };
                    }
                    else
                    {
                        return new ResponseDTO<string>
                        {
                            Success = false,
                            Message = "Collaborator not found"
                        };
                    }
                }
                else
                {
                    _logger.LogWarning("Note with ID: {NoteId} not found for user with ID: {UserId}", request.NoteId, userId);
                    await transaction.RollbackAsync();
                    return new ResponseDTO<string>
                    {
                        Success = false,
                        Message = "Note not found"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO<string>
                {
                    Success = false,
                    Message = ex.Message
                };
            }

        }
        public async Task<ResponseDTO<List<CollabDTO>>> ViewAllCollabsAsync(int noteId)
        {
            using var transaction = _userContext.Database.BeginTransaction();
            try
            {
                _logger.LogInformation("Attempting to view all collaborators for note with ID: {NoteId}", noteId);
                var collabs = _userContext.Collaborators.Where(c => c.NoteId == noteId).ToList();
                if (collabs != null && collabs.Count > 0)
                {
                    _logger.LogInformation("Found {Count} collaborators for note with ID: {NoteId}", collabs.Count, noteId);    
                    var collabList = new List<CollabDTO>();
                    foreach (var collab in collabs)
                    {
                        collabList.Add(new CollabDTO
                        {
                            NoteId = collab.NoteId,
                            CollabEmail = collab.CollabEmail
                        });
                    }
                    await transaction.CommitAsync();
                    return new ResponseDTO<List<CollabDTO>>
                    {
                        Success = true,
                        Message = "Collaborators retrieved successfully",
                        Data = collabList.ToList()
                    };
                }
                else
                {
                    return new ResponseDTO<List<CollabDTO>>
                    {
                        Success = false,
                        Message = "No collaborators found for this note"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while retrieving collaborators: {Message}", ex.Message);
                await transaction.RollbackAsync();
                return new ResponseDTO<List<CollabDTO>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

    }
}
