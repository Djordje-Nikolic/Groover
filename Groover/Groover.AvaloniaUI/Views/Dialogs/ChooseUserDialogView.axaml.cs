using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Groover.AvaloniaUI.ViewModels.Dialogs;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Groover.AvaloniaUI.Views.Dialogs
{
    public partial class ChooseUserDialogView : ReactiveWindow<ChooseUserDialogViewModel>
    {
        private Button CheckButton;

        private ProgressBar CheckProgressBar;
        private TextBox UsernameTextBox;

        public ChooseUserDialogView()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            CheckButton = this.FindControl<Button>("checkButton");
            CheckProgressBar = this.FindControl<ProgressBar>("checkProgressBar");
            UsernameTextBox = this.FindControl<TextBox>("usernameTextBox");
            this.WhenActivated(disposables => 
            {
                this.WhenAnyObservable(v => v.ViewModel.CheckCommand.IsExecuting)
                .Select(val => val ? "Outline" : "Light")
                .BindTo(this, x => x.CheckButton.Classes)
                .DisposeWith(disposables);

                this.WhenAnyObservable(v => v.ViewModel.CheckCommand.IsExecuting)
                .BindTo(this, x => x.CheckProgressBar.IsVisible)
                .DisposeWith(disposables);

                this.WhenAnyObservable(v => v.ViewModel.CheckCommand.IsExecuting)
                .BindTo(this, x => x.UsernameTextBox.IsReadOnly)
                .DisposeWith(disposables);

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
