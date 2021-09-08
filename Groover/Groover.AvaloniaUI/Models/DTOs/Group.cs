using AutoMapper;
using Avalonia.Media.Imaging;
using DynamicData;
using DynamicData.Binding;
using Groover.AvaloniaUI.Models.Requests;
using Groover.AvaloniaUI.Utils;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.DTOs
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<GroupUser> GroupUsers { get; set; }

        public string ImageBase64 { get; set; }
    }
}
