using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.DTO
{
    public class CreateNoteDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? Reminder { get; set; }
        public string Backgroundcolor { get; set; }
        public bool Pin { get; set; }
        public bool Trash { get; set; }
        public bool Archieve { get; set; }

    }
}
