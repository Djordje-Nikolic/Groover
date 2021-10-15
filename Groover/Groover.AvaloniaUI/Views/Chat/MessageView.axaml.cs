using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Groover.AvaloniaUI.ViewModels.Chat;
using ReactiveUI;
using System.Reactive.Disposables;

namespace Groover.AvaloniaUI.Views.Chat
{
    public partial class MessageView : ReactiveUserControl<MessageViewModel>
    {
        private TrackView _trackView;

        public MessageView()
        {
            InitializeComponent();

            _trackView = this.FindControl<TrackView>("trackView");

            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel,
                    vm => vm.Track,
                    view => view._trackView.DataContext)
                .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    vm => vm.HasTrack,
                    view => view._trackView.IsVisible)
                .DisposeWith(disposables);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
