using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Groover.AvaloniaUI.ViewModels.Notifications;

namespace Groover.AvaloniaUI.Views
{
    public partial class NotificationsView : ReactiveUserControl<NotificationViewModel>
    {
        public NotificationsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
