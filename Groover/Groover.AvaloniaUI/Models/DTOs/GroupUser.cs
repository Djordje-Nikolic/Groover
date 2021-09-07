﻿using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.DTOs
{
    [DataContract]
    public class GroupUser : ReactiveObject
    {
        [DataMember]
        [Reactive]
        public User User { get; set; }
        [DataMember]
        [Reactive]
        public string GroupRole { get; set; }
    }
}
