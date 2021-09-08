using AutoMapper;
using Avalonia.Media.Imaging;
using Groover.AvaloniaUI.Models.Requests;
using Groover.AvaloniaUI.Utils;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.DTOs
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public ICollection<UserGroup> UserGroups { get; set; }

        public string AvatarBase64 { get; set; }
    }
}
