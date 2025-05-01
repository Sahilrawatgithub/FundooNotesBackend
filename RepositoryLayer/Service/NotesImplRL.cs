using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ModelLayer.Entity;
using Newtonsoft.Json;
using RepositoryLayer.Context;
using RepositoryLayer.DTO;
using RepositoryLayer.Interface;
using StackExchange.Redis;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RepositoryLayer.Service
{
    public class NotesImplRL : INotesRL
    {
        private readonly UserContext _userContext;
        private readonly ILogger<NotesImplRL> _logger;
        private readonly IDatabase _redisDatabase;
        private readonly IConnectionMultiplexer _redisConnection;

        public NotesImplRL(UserContext context, ILogger<NotesImplRL> logger, IConnectionMultiplexer redis)
        {
            _userContext = context;
            _logger = logger;
            _redisConnection = redis;
            _redisDatabase = redis.GetDatabase();
        }

        public async Task<ResponseDTO<NotesEntity>> CreateNotesAsync(CreateNoteDTO noteDto, int userId)
        {
            try
            {
                _logger.LogInformation("Attempting Note creation for user with ID: {UserId}", userId);
                var note = new NotesEntity
                {
                    Title = noteDto.Title,
                    Description = noteDto.Description,
                    Reminder = noteDto.Reminder ?? DateTime.Now,
                    Backgroundcolor = noteDto.Backgroundcolor,
                    Image = "",
                    Pin = noteDto.Pin,
                    Trash = noteDto.Trash,
                    Archieve = noteDto.Archieve,
                    Created = DateTime.Now,
                    Edited = DateTime.Now,
                    UserId = userId
                };

                _userContext.Notes.Add(note);
                await _userContext.SaveChangesAsync();

                await InvalidateUserNotesCache(userId);
                await InvalidateSingleNoteCache(note.NoteId, userId);

                await CacheAllUserNotes(userId);
                await CacheSingleNote(note);

                _logger.LogInformation("Note created successfully for user with ID: {UserId}", userId);
                return new ResponseDTO<NotesEntity>
                {
                    Success = true,
                    Message = "Note created successfully.",
                    Data = note
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating note");
                return new ResponseDTO<NotesEntity>
                {
                    Success = false,
                    Message = "Error creating note.",
                    Data = null
                };
            }
        }

        public async Task<ResponseDTO<NotesEntity>> RetrieveNotesAsync(int noteId, int userId)
        {
            try
            {
                _logger.LogInformation($"Retrieving note for NoteID: {noteId} and UserID: {userId}");

                string cacheKey = $"Note_{noteId}_User_{userId}";
                var cachedNote = await _redisDatabase.StringGetAsync(cacheKey);

                if (cachedNote.HasValue)
                {
                    _logger.LogInformation("Note found in cache for NoteID: {NoteId}", noteId);
                    var noteFromCache = JsonSerializer.Deserialize<NotesEntity>(cachedNote);
                    return new ResponseDTO<NotesEntity>
                    {
                        Success = true,
                        Message = "Note fetched from cache",
                        Data = noteFromCache
                    };
                }

                var note = await _userContext.Notes.FirstOrDefaultAsync(n => n.UserId == userId && n.NoteId == noteId);

                if (note != null)
                {
                    _logger.LogInformation("Note fetched from database for NoteID: {NoteId}", noteId);
                    await CacheAllUserNotes(userId);
                    return new ResponseDTO<NotesEntity>
                    {
                        Success = true,
                        Message = "Note fetched from database",
                        Data = note
                    };
                }

                _logger.LogWarning("No note found for NoteID: {NoteId} and UserID: {UserId}", noteId, userId);
                return new ResponseDTO<NotesEntity>
                {
                    Success = false,
                    Message = "No note found",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving note with NoteID: {NoteId} and UserID: {UserId}", noteId, userId);
                return new ResponseDTO<NotesEntity>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the note.",
                    Data = null
                };
            }
        }

        public async Task<ResponseDTO<List<NotesEntity>>> RetrieveAllNotesAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Retrieving all notes for UserID: {userId}");
                string cacheKey = $"AllNotes_User_{userId}";
                var cachedNotes = await _redisDatabase.StringGetAsync(cacheKey);
                if (cachedNotes.HasValue)
                {
                    _logger.LogInformation("All notes found in cache for UserID: {UserId}", userId);
                    var notesFromCache = JsonSerializer.Deserialize<List<NotesEntity>>(cachedNotes);
                    return new ResponseDTO<List<NotesEntity>>
                    {
                        Success = true,
                        Message = "All notes fetched from cache",
                        Data = notesFromCache
                    };
                }
                var allNotes = await _userContext.Notes.Where(n => n.UserId == userId).ToListAsync();
                if (allNotes != null && allNotes.Count > 0)
                {
                    _logger.LogInformation("All notes fetched from database for UserID: {UserId}", userId);
                    await CacheAllUserNotes(userId);
                    return new ResponseDTO<List<NotesEntity>>
                    {
                        Success = true,
                        Message = "All notes fetched from database",
                        Data = allNotes
                    };
                }
                _logger.LogWarning("No notes found for UserID: {UserId}", userId);
                return new ResponseDTO<List<NotesEntity>>
                {
                    Success = false,
                    Message = "No notes found",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all notes for UserID: {UserId}", userId);
                return new ResponseDTO<List<NotesEntity>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving all notes.",
                    Data = null
                };
            }
        }


        public async Task<ResponseDTO<NotesEntity>> UpdateNotesAsync(UpdateNoteDTO UpdatedNoteDTO, int noteId, int userId)
        {
            try
            {
                _logger.LogInformation($"Updating note for NoteID: {noteId} and UserID: {userId}");
                var cachekey = $"Note_{noteId}_User_{userId}";
                var cachedNote = await _redisDatabase.StringGetAsync(cachekey);
                if (cachedNote.HasValue)
                {
                    _logger.LogInformation("Note found in cache for NoteID: {NoteId}", noteId);
                    var noteFromCache = JsonSerializer.Deserialize<NotesEntity>(cachedNote);
                    if (noteFromCache != null)
                    {
                        noteFromCache.Title = UpdatedNoteDTO.Title;
                        noteFromCache.Description = UpdatedNoteDTO.Description;
                        noteFromCache.Reminder = UpdatedNoteDTO.Reminder;
                        
                        noteFromCache.Image = UpdatedNoteDTO.Image;
                        noteFromCache.Pin = UpdatedNoteDTO.Pin;
                        noteFromCache.Trash = UpdatedNoteDTO.Trash;
                        
                        noteFromCache.Edited = DateTime.Now;
                        _userContext.Notes.Update(noteFromCache);
                        await _userContext.SaveChangesAsync();
                        await InvalidateSingleNoteCache(noteId, userId);
                        await InvalidateUserNotesCache(userId);
                        await CacheAllUserNotes(userId);
                        await CacheSingleNote(noteFromCache);

                        return new ResponseDTO<NotesEntity>
                        {
                            Success = true,
                            Message = "Note updated successfully.",
                            Data = noteFromCache
                        };
                    }
                }
                else
                {
                    var note = await _userContext.Notes.FirstOrDefaultAsync(n => n.UserId == userId && n.NoteId == noteId);
                    if (note == null)
                    {
                        _logger.LogWarning("No note found for NoteID: {NoteId} and UserID: {UserId}", noteId, userId);
                        return new ResponseDTO<NotesEntity>
                        {
                            Success = false,
                            Message = "No note found",
                            Data = null
                        };
                    }
                    else
                    {
                        _logger.LogInformation("Note fetched from database for NoteID: {NoteId}", noteId);
                        note.Title = UpdatedNoteDTO.Title;
                        note.Description = UpdatedNoteDTO.Description;
                        note.Reminder = UpdatedNoteDTO.Reminder;
                        note.Edited = DateTime.Now;
                        
                        note.Image = UpdatedNoteDTO.Image;
                        note.Pin = UpdatedNoteDTO.Pin;
                        note.Trash = UpdatedNoteDTO.Trash;
                       
                        _userContext.Notes.Update(note);
                        await _userContext.SaveChangesAsync();
                        await InvalidateSingleNoteCache(noteId, userId);
                        await InvalidateUserNotesCache(userId);
                        await CacheAllUserNotes(userId);
                        await CacheSingleNote(note);

                        return new ResponseDTO<NotesEntity>
                        {
                            Success = true,
                            Message = "Note updated successfully.",
                            Data = note
                        };
                    }
                }
                return new ResponseDTO<NotesEntity>
                {
                    Success = false,
                    Message = "An unexpected error occurred during update.",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating note with NoteID: {NoteId} and UserID: {UserId}", noteId, userId);
                return new ResponseDTO<NotesEntity>
                {
                    Success = false,
                    Message = "An error occurred while updating the note.",
                    Data = null
                };
            }
        }


        public async Task<ResponseDTO<string>> DeleteNotesAsync(int noteId, int userId)
        {
            try
            {
                _logger.LogInformation($"Attempting deletion of note for NoteID: {noteId} and UserID: {userId}");

                var cacheKey = $"Note_{noteId}_User_{userId}";
                var note = await _userContext.Notes.FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);

                if (note != null)
                {
                    if (note.Trash == false)
                    {
                        var noteJson = JsonConvert.SerializeObject(note);
                        await CacheSingleNote(note);
                        _logger.LogInformation($"Note backed up in Redis for NoteID: {noteId}");
                    }
                    else
                    {
                        
                        await _redisDatabase.KeyDeleteAsync(cacheKey); 
                        _logger.LogInformation($"No backup created since note is already in Trash for NoteID: {noteId}");
                    }

                    _userContext.Notes.Remove(note);
                    await _userContext.SaveChangesAsync();

                    await InvalidateUserNotesCache(userId);
                    await CacheAllUserNotes(userId);

                    return new ResponseDTO<string>
                    {
                        Success = true,
                        Message = "Note deleted successfully.",
                        Data = "Note deleted."
                    };
                }
                else
                {
                    return new ResponseDTO<string>
                    {
                        Success = false,
                        Message = "Note not found.",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting note.");
                return new ResponseDTO<string>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }


        public async Task<ResponseDTO<string>> TrashNoteAsync(int noteId, int userId)
        {
            try
            {
                _logger.LogInformation($"Trashing note for userId: {userId}, noteId: {noteId}");

                
                string cacheKey = $"Note_{noteId}_User_{userId}";

                var cachedNote = await _redisDatabase.StringGetAsync(cacheKey);
                if (cachedNote.HasValue)
                {
                    
                    var noteData = JsonConvert.DeserializeObject<NotesEntity>(cachedNote);

                    if (noteData.Trash)
                    {
                        return new ResponseDTO<string>
                        {
                            Success = true,
                            Message = "Note is already trashed.",
                            Data = null
                        };
                    }
                    var note = await _userContext.Notes.FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);
                    if (note == null)
                    {
                        return new ResponseDTO<string>
                        {
                            Success = false,
                            Message = "Note not found in database.",
                            Data = null
                        };
                    }

                    note.Trash = true;
                    await _userContext.SaveChangesAsync();
                   
                        await InvalidateUserNotesCache(userId);
                        await CacheAllUserNotes(userId);
                    

                    return new ResponseDTO<string>
                    {
                        Success = true,
                        Message = "Note trashed successfully.",
                        Data = null
                    };
                }
                else
                {
                    
                    var note = await _userContext.Notes.FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);
                    if (note == null)
                    {
                        return new ResponseDTO<string>
                        {
                            Success = false,
                            Message = "Note not found in database.",
                            Data = null
                        };
                    }

                    
                    note.Trash = true;
                    await _userContext.SaveChangesAsync();
                    await InvalidateUserNotesCache(userId);
                    await CacheAllUserNotes(userId);
                    return new ResponseDTO<string>
                    {
                        Success = true,
                        Message = "Note trashed and updated in cache.",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trashing note with NoteID: {NoteId} and UserID: {UserId}", noteId, userId);
                return new ResponseDTO<string>
                {
                    Success = false,
                    Message = "An error occurred while trashing the note.",
                    Data = null
                };
            }
        }



        public async Task<ResponseDTO<string>> ArchiveNoteAsync(int noteId, int userId)
        {
            try
            {
                _logger.LogInformation($"Archiving note for userId: {userId}, noteId: {noteId}");

                string cacheKey = $"Note_{noteId}_User_{userId}";

                var cachedNote = await _redisDatabase.StringGetAsync(cacheKey);
                if (cachedNote.HasValue)
                {
                    var noteData = JsonConvert.DeserializeObject<NotesEntity>(cachedNote);

                    if (noteData.Archieve)
                    {
                        return new ResponseDTO<string>
                        {
                            Success = true,
                            Message = "Note is already archived.",
                            Data = null
                        };
                    }

                    var note = await _userContext.Notes.FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);
                    if (note == null)
                    {
                        return new ResponseDTO<string>
                        {
                            Success = false,
                            Message = "Note not found in database.",
                            Data = null
                        };
                    }

                    note.Archieve = true;
                    await _userContext.SaveChangesAsync();
                    await InvalidateUserNotesCache(userId);
                    await CacheAllUserNotes(userId);

                    return new ResponseDTO<string>
                    {
                        Success = true,
                        Message = "Note archived successfully.",
                        Data = null
                    };
                }
                else
                {
                    var note = await _userContext.Notes.FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);
                    if (note == null)
                    {
                        return new ResponseDTO<string>
                        {
                            Success = false,
                            Message = "Note not found in database.",
                            Data = null
                        };
                    }

                    note.Archieve = true;
                    await _userContext.SaveChangesAsync();
                    await InvalidateUserNotesCache(userId);
                    await CacheAllUserNotes(userId);
                    return new ResponseDTO<string>
                    {
                        Success = true,
                        Message = "Note archived and updated in cache.",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while archiving note with NoteID: {NoteId} and UserID: {UserId}", noteId, userId);
                return new ResponseDTO<string>
                {
                    Success = false,
                    Message = "An error occurred while archiving the note.",
                    Data = null
                };
            }
        }


        public async Task<ResponseDTO<string>> PinNoteAsync(int noteId, int userId)
        {
            try
            {
                _logger.LogInformation($"Pinning note for userId: {userId}, noteId: {noteId}");

                string cacheKey = $"Note_{noteId}_User_{userId}";

                var cachedNote = await _redisDatabase.StringGetAsync(cacheKey);
                if (cachedNote.HasValue)
                {
                    var noteData = JsonConvert.DeserializeObject<NotesEntity>(cachedNote);

                    if (noteData.Pin)
                    {
                        return new ResponseDTO<string>
                        {
                            Success = true,
                            Message = "Note is already pinned.",
                            Data = null
                        };
                    }

                    var note = await _userContext.Notes.FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);
                    if (note == null)
                    {
                        return new ResponseDTO<string>
                        {
                            Success = false,
                            Message = "Note not found in database.",
                            Data = null
                        };
                    }

                    note.Pin = true;
                    await _userContext.SaveChangesAsync();
                    await InvalidateUserNotesCache(userId);
                    await CacheAllUserNotes(userId);

                    return new ResponseDTO<string>
                    {
                        Success = true,
                        Message = "Note pinned successfully.",
                        Data = null
                    };
                }
                else
                {
                    var note = await _userContext.Notes.FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);
                    if (note == null)
                    {
                        return new ResponseDTO<string>
                        {
                            Success = false,
                            Message = "Note not found in database.",
                            Data = null
                        };
                    }

                    note.Pin = true;
                    await _userContext.SaveChangesAsync();

                    await CacheAllUserNotes(userId);
                    return new ResponseDTO<string>
                    {
                        Success = true,
                        Message = "Note pinned and updated in cache.",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while pinning note with NoteID: {NoteId} and UserID: {UserId}", noteId, userId);
                return new ResponseDTO<string>
                {
                    Success = false,
                    Message = "An error occurred while pinning the note.",
                    Data = null
                };
            }
        }


        public async Task<ResponseDTO<string>> UnArchiveNoteAsync(int noteId, int userId)
        {
            try
            {
                _logger.LogInformation($"Unarchiving note for userId: {userId}, noteId: {noteId}");

                string cacheKey = $"Note_{noteId}_User_{userId}";

                var cachedNote = await _redisDatabase.StringGetAsync(cacheKey);
                if (cachedNote.HasValue)
                {
                    var noteData = JsonConvert.DeserializeObject<NotesEntity>(cachedNote);

                    if (!noteData.Archieve)
                    {
                        return new ResponseDTO<string>
                        {
                            Success = true,
                            Message = "Note is not archived.",
                            Data = null
                        };
                    }

                    var note = await _userContext.Notes.FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);
                    if (note == null)
                    {
                        return new ResponseDTO<string>
                        {
                            Success = false,
                            Message = "Note not found in database.",
                            Data = null
                        };
                    }

                    note.Archieve = false;
                    await _userContext.SaveChangesAsync();
                    await InvalidateUserNotesCache(userId);
                    await CacheAllUserNotes(userId);

                    return new ResponseDTO<string>
                    {
                        Success = true,
                        Message = "Note unarchived successfully.",
                        Data = null
                    };
                }
                else
                {
                    var note = await _userContext.Notes.FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);
                    if (note == null)
                    {
                        return new ResponseDTO<string>
                        {
                            Success = false,
                            Message = "Note not found in database.",
                            Data = null
                        };
                    }

                    note.Archieve = false;
                    await _userContext.SaveChangesAsync();
                    await InvalidateUserNotesCache(userId);
                    await CacheAllUserNotes(userId);
                    return new ResponseDTO<string>
                    {
                        Success = true,
                        Message = "Note unarchived and updated in cache.",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while unarchiving note with NoteID: {NoteId} and UserID: {UserId}", noteId, userId);
                return new ResponseDTO<string>
                {
                    Success = false,
                    Message = "An error occurred while unarchiving the note.",
                    Data = null
                };
            }
        }

        public async Task<ResponseDTO<string>> RestoreNoteAsync(int noteId,int userId)
        {
            try
            {
                _logger.LogInformation($"Restoring note for userId: {userId}, noteId: {noteId}");
                string cacheKey = $"Note_{noteId}_User_{userId}";
                var cachedNote = await _redisDatabase.StringGetAsync(cacheKey);
                if (cachedNote.HasValue)
                {
                    _logger.LogInformation("cached note has some value indeed");
                    var noteToRestore = await _userContext.Notes
                        .FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);

                    if (noteToRestore == null)
                    {
                        _logger.LogInformation("Adding cached note to database");
                        noteToRestore= JsonConvert.DeserializeObject<NotesEntity>(cachedNote.ToString());
                        noteToRestore.NoteId = 0;
                        _userContext.Notes.Add(noteToRestore);
                        await _userContext.SaveChangesAsync();
                        await InvalidateSingleNoteCache(noteId, userId);
                        await InvalidateUserNotesCache(userId);
                        await CacheAllUserNotes(userId);
                        await CacheSingleNote(noteToRestore);

                        return new ResponseDTO<string>
                        {
                            Success = true,
                            Message = "Note recovered successfully",
                            Data = null
                        };
                    }
                    else
                    {
                        return new ResponseDTO<string>
                        {
                            Success = false,
                            Message = "Note already exists",
                            Data = null
                        };
                    }
                }
                else
                {
                    _logger.LogInformation("Cannot restore note as it no longer exists in cache");
                    return new ResponseDTO<string>
                    {
                        Success = false,
                        Message = "Note could not be restored",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while restoring note with NoteID: {NoteId} and UserID: {UserId}", noteId, userId);
                return new ResponseDTO<string>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
            }

     


        private async Task InvalidateUserNotesCache(int userId)
        {
            string key = $"AllNotes_User_{userId}";
            await _redisDatabase.KeyDeleteAsync(key);
        }

        private async Task InvalidateSingleNoteCache(int noteId, int userId)
        {
            string key = $"Note_{noteId}_User_{userId}";
            await _redisDatabase.KeyDeleteAsync(key);
        }

        private async Task CacheSingleNote(NotesEntity note)
        {
            string cacheKey = $"Note_{note.NoteId}_User_{note.UserId}";
            var serializedNote = JsonSerializer.Serialize(note);
            TimeSpan expiration = TimeSpan.FromMinutes(10);

            await _redisDatabase.StringSetAsync(cacheKey, serializedNote, expiration);
        }

        private async Task CacheAllUserNotes(int userId)
        {
            var allNotes = await _userContext.Notes.Where(n => n.UserId == userId).ToListAsync();
            var serializedNotes = JsonSerializer.Serialize(allNotes); // System.Text.Json

            string cacheKey = $"AllNotes_User_{userId}";
            TimeSpan expiration = TimeSpan.FromMinutes(10);

            await _redisDatabase.StringSetAsync(cacheKey, serializedNotes, expiration);
        }


    }
}
