using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Groover.AvaloniaUI.ViewModels.Notifications;
using ReactiveUI;
using System;
using System.Reactive.Disposables;

namespace Groover.AvaloniaUI.Views.Notifications
{
    public partial class ErrorView : ReactiveWindow<ErrorViewModel>
    {
        public ErrorView()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                ViewModel.YesCommand.Subscribe(x => this.Close(x)).DisposeWith(disposables);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
