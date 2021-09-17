using Groover.IdentityDB.MySqlDb.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models.DTOs
{
    public class GroupUserDTO
    {
        public int UserId { get; set; }
        public int GroupId { get; set; }
        public UserDTO User { get; set; }
        public GroupDTO Group { get; set; }
        public GroupRole GroupRole { get; set; }
    }
}
