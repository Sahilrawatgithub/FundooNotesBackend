using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ModelLayer.Entity;
using RepositoryLayer.DTO;

namespace RepositoryLayer.Interface
{
    public interface INotesRL
    {
        Task<ResponseDTO<NotesEntity>> CreateNotesAsync(CreateNoteDTO noteDto, int userId);

        Task<ResponseDTO<NotesEntity>> RetrieveNotesAsync(int noteId,int userId);

        Task<ResponseDTO<List<NotesEntity>>> RetrieveAllNotesAsync( int userId);

        Task<ResponseDTO<NotesEntity>> UpdateNotesAsync(UpdateNoteDTO UpdatedNoteDTO, int noteId, int userId);

        Task<ResponseDTO<string>> DeleteNotesAsync(int noteId, int userId);

        Task<ResponseDTO<string>> TrashNoteAsync(int noteId, int userId);
        Task<ResponseDTO<string>> ArchiveNoteAsync(int noteId, int userId);
        Task<ResponseDTO<string>> PinNoteAsync(int noteId, int userId);

        Task<ResponseDTO<string>> UnArchiveNoteAsync(int noteId, int userId);

        Task<ResponseDTO<string>> RestoreNoteAsync(int noteId, int userId);


    }
}
