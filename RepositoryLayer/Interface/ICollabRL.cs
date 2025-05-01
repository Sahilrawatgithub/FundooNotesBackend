using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepositoryLayer.DTO;

namespace RepositoryLayer.Interface
{
    public interface ICollabRL
    {
        public Task<ResponseDTO<string>> AddCollabToNoteAsync( CollabDTO request,int userId);

        public Task<ResponseDTO<string>> RemoveCollaboratorAsync(CollabDTO request, int userId);

        public Task<ResponseDTO<List<CollabDTO>>> ViewAllCollabsAsync(int noteId);
    }
}
