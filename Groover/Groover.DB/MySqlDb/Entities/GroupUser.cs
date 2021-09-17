using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.IdentityDB.MySqlDb.Entities
{
    public class GroupUser
    {
        public int UserId { get; set; }
        public int GroupId { get; set; }
        public User User { get; set; }
        public Group Group { get; set; }
        public GroupRole GroupRole { get; set; }
    }
}
