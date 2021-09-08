using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.DTOs
{
    public class UserGroup
    {
        public Group Group { get; set; }
        public string GroupRole { get; set; }
    }
}
