using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer.Entity
{
    public class CollaboratorEntity
    {
        [Key]
        public int CollaboratorId { get; set; }
        public int NoteId { get; set; }
        [ForeignKey("NoteId")]
        public NotesEntity Note { get; set; }

        public string CollabEmail { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public UserEntity User { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
