using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.Interface;
using RepositoryLayer.DTO;
using RepositoryLayer.Interface;

namespace BusinessLayer.Service
{
    public class CollabImpleBL:ICollabBL
    {
        private readonly ICollabRL _collabRL;
        public CollabImpleBL(ICollabRL _collabRL)
        {
            this._collabRL = _collabRL;
        }

        public async Task<ResponseDTO<string>> AddCollabToNoteAsync(CollabDTO request, int userId)
        {
            return await _collabRL.AddCollabToNoteAsync(request, userId);
        }
        public async Task<ResponseDTO<string>> RemoveCollaboratorAsync(CollabDTO request, int userId)
        {
            return await _collabRL.RemoveCollaboratorAsync(request, userId);
        }
        public async Task<ResponseDTO<List<CollabDTO>>> ViewAllCollabsAsync(int noteId)
        {
            return await _collabRL.ViewAllCollabsAsync(noteId);
        }

    }
}
