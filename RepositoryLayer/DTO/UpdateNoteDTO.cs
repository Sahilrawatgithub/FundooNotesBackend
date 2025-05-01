using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.DTO
{
    public class UpdateNoteDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string BackgroundColor { get; set; }
        public string Image { get; set; } 
        public DateTime Reminder { get; set; }
        public bool Pin { get; set; }
        public bool Trash { get; set; }
        public bool Archive { get; set; }
        public DateTime Edited { get; set; }

    }
}
