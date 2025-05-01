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
    public class LabelImplBL:ILabelBL
    {
        private readonly ILabelRL _labelRL;
        private readonly ILogger<LabelImplBL>  _logger;
        public LabelImplBL(ILabelRL _labelRL,ILogger<LabelImplBL> logger)
        {
            this._labelRL = _labelRL;
            this._logger = logger;
        }

        public async Task<ResponseDTO<LabelEntity>> CreateLabelAsync(string labelName, int userId)
        {
            try
            {
                return await _labelRL.CreateLabelAsync(labelName, userId);
            }
            catch (Exception ex)
            {
                
                return new ResponseDTO<LabelEntity>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ResponseDTO<string>> DeleteLabelAsync(string labelName, int userId)
        {
            try
            {
                return await _labelRL.DeleteLabelAsync(labelName, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting label");
                return new ResponseDTO<string>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
            
        }

        public async Task<ResponseDTO<string>> AddLabelToNote(string labelName, int noteId, int userId)
        {
            return await _labelRL.AddLabelToNoteAsync(labelName, noteId, userId);
        }
        public async Task<ResponseDTO<List<LabelEntity>>> GetAllLabelsAsync(int userId)
        {
            return await _labelRL.GetAllLabelsAsync(userId);
        }
        public async Task<ResponseDTO<LabelEntity>> GetLabelByIdAsync(int labelId, int userId)
        {
            return await _labelRL.GetLabelByIdAsync(labelId, userId);
        }
        public async Task<ResponseDTO<LabelEntity>> UpdateLabelAsync(string oldLabel, string newLabel, int userId)
        {
            return await _labelRL.UpdateLabelAsync(oldLabel, newLabel, userId);
        }
    }
}
