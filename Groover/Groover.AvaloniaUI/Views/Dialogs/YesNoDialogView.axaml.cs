using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Groover.AvaloniaUI.ViewModels.Dialogs;
using ReactiveUI;
using System;
using System.Reactive.Disposables;

namespace Groover.AvaloniaUI.Views.Dialogs
{
    public partial class YesNoDialogView : ReactiveWindow<YesNoDialogViewModel>
    {
        public YesNoDialogView()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            this.WhenActivated(disposables =>
            {
                ViewModel.YesCommand.Subscribe(x => this.Close(x)).DisposeWith(disposables);
                ViewModel.NoCommand.Subscribe(x => this.Close(x)).DisposeWith(disposables);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
