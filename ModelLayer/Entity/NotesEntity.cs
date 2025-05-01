using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModelLayer.Entity
{
    public class NotesEntity
    {
        [Key]
        public int NoteId { get; set; }
        
        [ForeignKey("Users")]
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Reminder { get; set; }
        public string Backgroundcolor { get; set; }
        public string Image { get; set; }
        public bool Pin { get; set; }
        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public bool Trash { get; set; }
        public bool Archieve { get; set; }

        [JsonIgnore]
        public UserEntity Users { get; set; }

        [JsonIgnore]
        public ICollection<NoteLabelEntity> LabelNotes { get; set; }=new List<NoteLabelEntity>();

    }
}
