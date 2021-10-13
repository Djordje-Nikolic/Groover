using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Groover.AvaloniaUI.ViewModels.Chat;

namespace Groover.AvaloniaUI.Views.Chat
{
    public partial class ChatView : ReactiveUserControl<ChatViewModel>
    {
        public ChatView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
