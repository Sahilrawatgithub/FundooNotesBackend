using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModelLayer.Entity
{
    public class NoteLabelEntity
    {
        [Key]
        public int LabelNoteId { get; set; }

        [ForeignKey("Label")]
        public int LabelId { get; set; }
        public LabelEntity Label { get; set; }

        [ForeignKey("Note")]
        public int NoteId { get; set; }
        [JsonIgnore]
        public NotesEntity Note { get; set; }

    }
}
