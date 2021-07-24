using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.DB.MySqlDb.Entities
{
    public class User : IdentityUser<int>
    {
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
        public virtual ICollection<GroupUser> UserGroups { get; set; }

        [NotMapped]
        public byte[] AvatarImage
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(AvatarImagePath))
                    return File.ReadAllBytes(AvatarImagePath);
                else
                    return null;
            }
        }
        public virtual string AvatarImagePath { get; set; }
    }
}
