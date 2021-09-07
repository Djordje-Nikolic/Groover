using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels.Notifications
{
    public class NotificationViewModel : ViewModelBase
    {
        public Guid Id { get; private set; }
        public DateTime CreatedAt { get; private set; }

        [Reactive]
        public string TitleText { get; set; }
        [Reactive]
        public string BodyText { get; set; }
        [Reactive]
        public string YesButtonText { get; set; }

        public ReactiveCommand<Unit, NotificationViewModel?> YesCommand { get; }

        [Reactive]
        public bool Read { get; set; }

        public NotificationViewModel()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;

            YesButtonText = "Ok";

            YesCommand = ReactiveCommand.CreateFromTask<NotificationViewModel?>(YesOperationAsync);
        }

        protected virtual async Task<NotificationViewModel?> YesOperationAsync() { return null; }
    }
}
