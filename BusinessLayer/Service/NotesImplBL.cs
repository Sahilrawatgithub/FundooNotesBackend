using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.Interface;
using Microsoft.Extensions.Logging;
using ModelLayer.Entity;
using RepositoryLayer.DTO;
using RepositoryLayer.Interface;

namespace BusinessLayer.Service
{
    public class NotesImplBL : INotesBL
    {
        private readonly INotesRL _notesRL;
        public NotesImplBL(INotesRL notesRL)
        {
            _notesRL = notesRL;
        }

        public async Task<ResponseDTO<NotesEntity>> CreateNotesAsync(CreateNoteDTO noteDto, int userId)
        {
            return await _notesRL.CreateNotesAsync(noteDto, userId);
        }

        public async Task<ResponseDTO<NotesEntity>> RetrieveNotesAsync(int noteId, int userId)
        {
            return await _notesRL.RetrieveNotesAsync(noteId, userId);   
        }

        public async Task<ResponseDTO<List<NotesEntity>>> RetrieveAllNotesAsync(int userId)
        {
            return await _notesRL.RetrieveAllNotesAsync(userId);
        }
        public async Task<ResponseDTO<NotesEntity>> UpdateNotesAsync(UpdateNoteDTO UpdatedNoteDTO, int noteId, int userId)
        {
            return await _notesRL.UpdateNotesAsync(UpdatedNoteDTO, noteId, userId);
        }
        public async Task<ResponseDTO<string>> DeleteNotesAsync(int noteId, int userId)
        {
            return await _notesRL.DeleteNotesAsync(noteId, userId);
        }
        public async Task<ResponseDTO<string>> TrashNoteAsync(int noteId, int userId)
        {
            return await _notesRL.TrashNoteAsync(noteId, userId);
        }
        public async Task<ResponseDTO<string>> ArchiveNoteAsync(int noteId, int userId)
        {
            return await _notesRL.ArchiveNoteAsync(noteId, userId);
        }
        public async Task<ResponseDTO<string>> PinNoteAsync(int noteId, int userId)
        {
            return await _notesRL.PinNoteAsync(noteId, userId);
        }
        public async Task<ResponseDTO<string>> UnArchiveNoteAsync(int noteId, int userId)
        {
            return await _notesRL.UnArchiveNoteAsync(noteId, userId);
        }
        public async Task<ResponseDTO<string>> RestoreNoteAsync(int noteId, int userId)
        {
            return await _notesRL.RestoreNoteAsync(noteId, userId);
        }

    }
}
