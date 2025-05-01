using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RepositoryLayer.DTO;

namespace FundooNotes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CollabController : ControllerBase
    {
        private readonly ICollabBL _collabBL;
        public CollabController(ICollabBL collabBL)
        {
            this._collabBL = collabBL;
        }

        /// <summary>
        /// Adds a collaborator to a specific note for the authenticated user.
        /// </summary>
        /// <param name="request">A <see cref="CollabDTO"/> object containing NoteId and CollaboratorEmail.</param>
        /// <returns>Returns success message and added collaborator data if successful, otherwise an error message.</returns>
        [Authorize]
        [HttpPost("AddCollabToNote")]
        public async Task<IActionResult> AddCollabToNoteAsync(CollabDTO request)
        {
            try
            {
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id");
                var userId = Convert.ToInt32(userIdClaim.Value);
                var result = await _collabBL.AddCollabToNoteAsync(request, userId);
                if (result != null)
                {
                    return Ok(result);
                }
                return BadRequest(new { success = false, message = "Failed to add collaborator" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Removes a collaborator from a specific note for the authenticated user.
        /// </summary>
        /// <param name="request">A <see cref="CollabDTO"/> object containing NoteId and CollaboratorEmail.</param>
        /// <returns>Returns success message and removed collaborator data if successful, otherwise an error message.</returns>
        [Authorize]
        [HttpPost("RemoveCollaborator")]
        public async Task<IActionResult> RemoveCollaboratorAsync(CollabDTO request)
        {
            try
            {
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id");
                var userId = Convert.ToInt32(userIdClaim.Value);
                var result = await _collabBL.RemoveCollaboratorAsync(request, userId);
                if (result != null)
                {
                    return Ok(result);
                }
                return BadRequest(new { success = false, message = "Failed to remove collaborator" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all collaborators associated with a specific note for the authenticated user.
        /// </summary>
        /// <param name="noteId">The ID of the note for which to retrieve collaborators.</param>
        /// <returns>Returns a list of collaborators if successful, otherwise an error message.</returns>
        [Authorize]
        [HttpGet("ViewAllCollabs")]
        public async Task<IActionResult> ViewAllCollabsAsync(int noteId)
        {
            try
            {
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id");
                var userId = Convert.ToInt32(userIdClaim.Value);
                var result = await _collabBL.ViewAllCollabsAsync(noteId);
                if (result != null)
                {
                    return Ok(result);
                }
                return BadRequest(new { success = false, message = "Failed to retrieve collaborators" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

    }
}
