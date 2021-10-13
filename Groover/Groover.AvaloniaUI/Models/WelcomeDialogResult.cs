using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models
{
    public class WelcomeDialogResult
    {
        public AppViewModel AppViewModel { get; set; }
        public bool ExitApp { get; set; }
    }
}
