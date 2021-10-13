using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Groover.AvaloniaUI.Models;
using Groover.AvaloniaUI.ViewModels.Dialogs;
using ReactiveUI;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Disposables;
using ReactiveUI.Validation.Extensions;

namespace Groover.AvaloniaUI.Views.Dialogs
{
    public partial class ChooseTrackDialogView : ReactiveWindow<ChooseTrackDialogViewModel>
    {
        private readonly TextBlock _trackError;

        public ChooseTrackDialogView()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            _trackError = this.FindControl<TextBlock>("trackError");

            this.WhenActivated(disposables =>
            {
                ViewModel?.YesCommand.Subscribe(x => this.Close(x)).DisposeWith(disposables);
                ViewModel?.NoCommand.Subscribe(x => this.Close(x)).DisposeWith(disposables);

                ViewModel?.ShowChooseFileDialog
                    .RegisterHandler(DoShowChooseTrackDialogAsync)
                    .DisposeWith(disposables);

                this.BindValidation(ViewModel, vm => vm.ChosenFileInfo, v => v._trackError.Text)
                    .DisposeWith(disposables);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async Task DoShowChooseTrackDialogAsync(InteractionContext<string[], string?> interaction)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (interaction.Input.Length > 0)
                dialog.Filters.Add(new FileDialogFilter() { Name = "Audio track", Extensions = interaction.Input.ToList() });
            dialog.AllowMultiple = false;

            string[] results = await dialog.ShowAsync(this);
            string? result = results?.FirstOrDefault();

            interaction.SetOutput(result);
        }
    }
}
