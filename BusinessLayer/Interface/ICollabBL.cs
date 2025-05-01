using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepositoryLayer.DTO;

namespace BusinessLayer.Interface
{
    public interface ICollabBL
    {
        Task<ResponseDTO<string>> AddCollabToNoteAsync(CollabDTO request, int userId);
        Task<ResponseDTO<string>> RemoveCollaboratorAsync(CollabDTO request, int userId);
        Task<ResponseDTO<List<CollabDTO>>> ViewAllCollabsAsync(int noteId);
    }
}
