using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Groover.AvaloniaUI.ViewModels.Chat;
using ReactiveUI;
using System.Reactive.Linq;
using DynamicData;
using System.Reactive.Disposables;
using Avalonia.Threading;
using System.Reactive;

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
                this.WhenAnyObservable(view => view.ViewModel.AddMessageCommand)
                    .Delay(TimeSpan.FromMilliseconds(100))
                    .Subscribe(_ =>
                    {
                        Dispatcher.UIThread.InvokeAsync(ScrollMessagesToEnd);
                    })
                    .DisposeWith(disposables);

                //The goal of the following is to scroll to the bottom when the messages are first loaded
                this.WhenAnyObservable(view => view.ViewModel.InitializeCommand)
                    .Delay(TimeSpan.FromMilliseconds(100))
                    .FirstAsync()
                    .Subscribe(_ =>
                    {
                        Dispatcher.UIThread.InvokeAsync(ScrollMessagesToEnd);
                    })
                    .DisposeWith(disposables);

                //The goal of the following is to scroll to the bottom when groups are switched
                _messageScrollViewer.AttachedToVisualTree += async (s,e) => await Dispatcher.UIThread.InvokeAsync(ScrollMessagesToEnd);

            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ScrollMessagesToEnd()
        {
            var tempVisib = _messageScrollViewer.VerticalScrollBarVisibility;
            _messageScrollViewer.VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Visible;
            _messageScrollViewer.ScrollToEnd();
            _messageScrollViewer.VerticalScrollBarVisibility = tempVisib;
        }
    }
}
