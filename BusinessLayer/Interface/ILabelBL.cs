using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLayer.Entity;
using RepositoryLayer.DTO;

namespace BusinessLayer.Interface
{
    public interface ILabelBL
    {
        Task<ResponseDTO<LabelEntity>> CreateLabelAsync(string labelName, int userId);

        Task<ResponseDTO<string>> DeleteLabelAsync(string labelName, int userId);
        Task<ResponseDTO<string>> AddLabelToNote(string labelName,int noteId, int userId);
        Task<ResponseDTO<List<LabelEntity>>> GetAllLabelsAsync(int userId);

        Task<ResponseDTO<LabelEntity>> GetLabelByIdAsync(int labelId, int userId);

        Task<ResponseDTO<LabelEntity>> UpdateLabelAsync(string oldLabel, string newLabel, int userId);

    }
}
