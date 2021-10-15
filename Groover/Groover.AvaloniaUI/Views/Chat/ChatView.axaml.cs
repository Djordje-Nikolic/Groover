using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Groover.AvaloniaUI.ViewModels.Chat;
using ReactiveUI;
using System.Reactive.Linq;
using DynamicData;

namespace Groover.AvaloniaUI.Views.Chat
{
    public partial class ChatView : ReactiveUserControl<ChatViewModel>
    {
        private ScrollViewer _messageScrollViewer;

        public ChatView()
        {
            InitializeComponent();

            _messageScrollViewer = this.FindControl<ScrollViewer>("messageScrollViewer");

            this.WhenActivated(disposables =>
            {
                ViewModel?.InitializeCommand.Subscribe(_ =>
                {
                    _messageScrollViewer?.ScrollToEnd();
                });
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
