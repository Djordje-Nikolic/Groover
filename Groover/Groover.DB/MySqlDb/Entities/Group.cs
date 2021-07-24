using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.DB.MySqlDb.Entities
{
    public class Group
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<GroupUser> GroupUsers { get; set; }

        [NotMapped]
        public byte[] Image
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ImagePath))
                    return File.ReadAllBytes(ImagePath);
                else
                    return null;
            }
        }
        public virtual string ImagePath { get; set; }
    }
}
