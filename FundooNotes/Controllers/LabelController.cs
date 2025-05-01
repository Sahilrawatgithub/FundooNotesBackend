using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FundooNotes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabelController : ControllerBase
    {
        private readonly ILabelBL _labelBL;
        public LabelController(ILabelBL labelBL)
        {
            this._labelBL = labelBL;
        }

        /// <summary>
        /// Creates a new label for the authenticated user.
        /// </summary>
        /// <param name="labelName">Name of the label to create.</param>
        /// <returns>Returns success message and created label data if successful, otherwise an error message.</returns>
        [Authorize]
        [HttpPost("AddLabel")]
        public async Task<IActionResult> CreateLabel(string labelName)
        {
            try
            {
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id");
                var userId = Convert.ToInt32(userIdClaim.Value);

                var result = await _labelBL.CreateLabelAsync(labelName, userId);
                if (result != null)
                {
                    return Ok(result);
                }
                return BadRequest(new { success = false, message = "Failed to create label" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a label for the authenticated user.
        /// </summary>
        /// <param name="labelName">Name of the label to delete.</param>
        /// <returns>Returns success message and deleted label data if successful, otherwise an error message.</returns>

        [Authorize]
        [HttpDelete("DeleteLabel")]
        public async Task<IActionResult> DeleteLabel(string labelName)
        {
            try
            {
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id");
                var userId = Convert.ToInt32(userIdClaim.Value);
                var result = await _labelBL.DeleteLabelAsync(labelName, userId);
                if (result != null)
                {
                    return Ok(result);
                }
                return BadRequest(new { success = false, message = "Failed to delete label" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Adds a label to a specific note for the authenticated user.
        /// </summary>
        /// <param name="labelName">Label to be added.</param>
        /// <param name="noteId">ID of the note to which the label is to be added.</param>
        /// <returns>Returns success message and result data if successful, otherwise an error message.</returns>
        [Authorize]
        [HttpPost("AddLabelToNote")]
        public async Task<IActionResult> AddLabelToNote(string labelName, int noteId)
        {
            try
            {
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id");
                var userId = Convert.ToInt32(userIdClaim.Value);
                var result = await _labelBL.AddLabelToNote(labelName, noteId, userId);
                if (result != null)
                {
                    return Ok(result);
                }
                return BadRequest(new { success = false, message = "Failed to add label to note" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all labels for the authenticated user.
        /// </summary>
        /// <returns>Returns list of all labels associated with the user, or an error message.</returns>
        [Authorize]
        [HttpGet("GetAllLabels")]
        public async Task<IActionResult> GetAllLabels()
        {
            try
            {
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id");
                var userId = Convert.ToInt32(userIdClaim.Value);
                var result = await _labelBL.GetAllLabelsAsync(userId);
                if (result != null)
                {
                    return Ok(result);
                }
                return BadRequest(new { success = false, message = "Failed to retrieve labels" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a specific label by its ID for the authenticated user.
        /// </summary>
        /// <param name="labelId">ID of the label to retrieve.</param>
        /// <returns>Returns the label data if found, otherwise an error message.</returns>
        [Authorize]
        [HttpGet("GetLabelById")]
        public async Task<IActionResult> GetLabelById(int labelId)
        {
            try
            {
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id");
                var userId = Convert.ToInt32(userIdClaim.Value);
                var result = await _labelBL.GetLabelByIdAsync(labelId, userId);
                if (result != null)
                {
                    return Ok(result);
                }
                return BadRequest(new { success = false, message = "Failed to retrieve label" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing label for the authenticated user.
        /// </summary>
        /// <param name="oldLabel">Current label name.</param>
        /// <param name="newLabel">New label name to update to.</param>
        /// <returns>Returns success message and updated label data if successful, otherwise an error message.</returns>
        
        [Authorize]
        [HttpPatch("UpdateLabel")]
        public async Task<IActionResult> UpdateLabel(string oldLabel, string newLabel)
        {
            try
            {
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id");
                var userId = Convert.ToInt32(userIdClaim.Value);
                var result = await _labelBL.UpdateLabelAsync(oldLabel, newLabel, userId);
                if (result != null)
                {
                    return Ok(result);
                }
                return BadRequest(new { success = false, message = "Failed to update label" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
