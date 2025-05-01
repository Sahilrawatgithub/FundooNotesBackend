using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.DTO
{
    public class UpdateEmailRequest
    {
        public string fName { get; set; }

        public string lName { get; set; }

        [EmailAddress]
        public string newEmail { get; set; }
    }
}
