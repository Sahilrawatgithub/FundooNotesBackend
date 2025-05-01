using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLayer.Entity;
using RepositoryLayer.DTO;

namespace RepositoryLayer.Interface
{
    public interface ILabelRL
    {
        Task<ResponseDTO<LabelEntity>> CreateLabelAsync(string labelName, int userId);
        Task<ResponseDTO<string>> DeleteLabelAsync(string labelName,int userId);
        Task<ResponseDTO<string>> AddLabelToNoteAsync(string labelName, int noteId, int userId);
        Task<ResponseDTO<List<LabelEntity>>> GetAllLabelsAsync(int userId);

        Task<ResponseDTO<LabelEntity>> GetLabelByIdAsync(int labelId,int userId);
        Task<ResponseDTO<LabelEntity>> UpdateLabelAsync(string oldLabel,string newLabel,int userId);
    }
}
