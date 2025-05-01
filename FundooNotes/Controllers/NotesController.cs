using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Entity;
using RepositoryLayer.DTO;


namespace FundooNotes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly INotesBL _notesBL;
        private readonly ILogger<NotesController> _logger;
        public NotesController(INotesBL notesBL, ILogger<NotesController> logger)
        {
            _notesBL = notesBL;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new note for the authenticated user.
        /// </summary>
        /// <param name="noteDto">Note details to be created.</param>
        /// <returns>Returns success or failure response.</returns>
        [Authorize]
        [HttpPost("CreateNotes")]
        public async Task<IActionResult> CreateNotes(CreateNoteDTO noteDto)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id").Value);
                var result = await _notesBL.CreateNotesAsync(noteDto, userId);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating note");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        /// <summary>
        /// Retrieves a specific note by its ID for the authenticated user.
        /// </summary>
        /// <param name="noteId">ID of the note to retrieve.</param>
        /// <returns>Returns the note if found, otherwise error response.</returns>
        [Authorize]
        [HttpGet("RetrieveNotesById")]
        public async Task<IActionResult> RetrieveNotesById(int noteId)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id").Value);
                var result = await _notesBL.RetrieveNotesAsync(noteId, userId);
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving note");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        /// <summary>
        /// Retrieves all notes created by the authenticated user.
        /// </summary>
        /// <returns>Returns a list of notes or an error response.</returns>
        [Authorize]
        [HttpGet("RetrieveAllNotes")]

        public async Task<IActionResult> RetrieveAllNotes()
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id").Value);
                var result = await _notesBL.RetrieveAllNotesAsync(userId);
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all notes");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        /// <summary>
        /// Updates an existing note for the authenticated user.
        /// </summary>
        /// <param name="UpdatedNoteDTO">Updated note details.</param>
        /// <param name="noteId">ID of the note to be updated.</param>
        /// <returns>Returns success or error response after update attempt.</returns>
        [Authorize]
        [HttpPatch("UpdateNotes")]
        public async Task<IActionResult> UpdateNotes(UpdateNoteDTO UpdatedNoteDTO, int noteId)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id").Value);
                var result = await _notesBL.UpdateNotesAsync(UpdatedNoteDTO, noteId, userId);
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating note");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        /// <summary>
        /// Deletes a note by its ID for the authenticated user.
        /// </summary>
        /// <param name="noteId">ID of the note to delete.</param>
        /// <returns>Returns success or failure message.</returns>
        [Authorize]
        [HttpDelete("DeleteNotes")]
        public async Task<IActionResult> DeleteNotes(int noteId)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id").Value);
                var result = await _notesBL.DeleteNotesAsync(noteId, userId);
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting note");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        /// <summary>
        /// Moves a note to trash for the authenticated user.
        /// </summary>
        /// <param name="noteId">ID of the note to move to trash.</param>
        /// <returns>Returns success or failure message.</returns>
        [Authorize]
        [HttpPatch("TrashNote")]
        public async Task<IActionResult> TrashNote(int noteId)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id").Value);
                var result = await _notesBL.TrashNoteAsync(noteId, userId);
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error trashing note");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        /// <summary>
        /// Archives a note for the authenticated user.
        /// </summary>
        /// <param name="noteId">ID of the note to archive.</param>
        /// <returns>Returns success or failure message.</returns>
        [Authorize]
        [HttpPatch("ArchiveNote")]
        public async Task<IActionResult> ArchiveNote(int noteId)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id").Value);
                var result = await _notesBL.ArchiveNoteAsync(noteId, userId);
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving note");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        /// <summary>
        /// Pins a note for the authenticated user.
        /// </summary>
        /// <param name="noteId">ID of the note to pin.</param>
        /// <returns>Returns success or failure message.</returns>
        [Authorize]
        [HttpPatch("PinNote")]
        public async Task<IActionResult> PinNote(int noteId)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id").Value);
                var result = await _notesBL.PinNoteAsync(noteId, userId);
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pinning note");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        /// <summary>
        /// Unarchives a previously archived note for the authenticated user.
        /// </summary>
        /// <param name="noteId">ID of the note to unarchive.</param>
        /// <returns>Returns success or failure message.</returns>
        [Authorize]
        [HttpPatch("UnArchiveNote")]
        public async Task<IActionResult> UnArchiveNote(int noteId)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id").Value);
                var result = await _notesBL.UnArchiveNoteAsync(noteId, userId);
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unarchiving note");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        /// <summary>
        /// Restores a trashed note back to active status for the authenticated user.
        /// </summary>
        /// <param name="noteId">ID of the note to restore.</param>
        /// <returns>Returns success or failure message.</returns>
        [Authorize]
        [HttpPost("RestoreNote")]
        public async Task<IActionResult> RestoreNote(int noteId)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id").Value);
                var result = await _notesBL.RestoreNoteAsync(noteId, userId);
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring note");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
