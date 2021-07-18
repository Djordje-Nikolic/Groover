using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Username { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Email { get; set; }
        public byte[] AvatarImage { get; set; }
        public ICollection<GroupUserDTO> UserGroups { get; set; }
    }
}
