using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels.Notifications
{
    public class ErrorViewModel : NotificationViewModel
    {
        [Reactive]
        public string ErrorCode { get; private set; }

        public ErrorViewModel(string errorCode) : base()
        {
            ErrorCode = $"Error: <{errorCode}>";
        }
    }
}
