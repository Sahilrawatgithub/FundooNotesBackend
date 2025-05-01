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
    public class LabelEntity
    {
        [Key]
        public int LabelId { get; set; }
        public string LabelName { get; set; }
        [ForeignKey("Users")]
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        [JsonIgnore]
        public ICollection<NoteLabelEntity> LabelNotes { get; set; } = new List<NoteLabelEntity>();
        [JsonIgnore]
        public UserEntity Users { get; set; }



    }
}
