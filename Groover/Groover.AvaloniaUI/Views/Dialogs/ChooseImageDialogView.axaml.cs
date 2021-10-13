using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Groover.AvaloniaUI.ViewModels.Dialogs;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Views.Dialogs
{
    public partial class ChooseImageDialogView : ReactiveWindow<ChooseImageDialogViewModel>
    {
        private readonly TextBlock _imageError;

        public ChooseImageDialogView()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            _imageError = this.FindControl<TextBlock>("imageError");
            var imageButton = this.FindControl<Button>("chooseImageButton");
            imageButton.AddHandler(PointerPressedEvent, onImageButtonPointerReleased, handledEventsToo: true);

            this.WhenActivated(disposables =>
            {
                ViewModel?.YesCommand.Subscribe(x => this.Close(x)).DisposeWith(disposables);
                ViewModel?.NoCommand.Subscribe(x => this.Close(x)).DisposeWith(disposables);

                ViewModel?.ShowChooseFileDialog
                    .RegisterHandler(DoShowChooseImageDialogAsync)
                    .DisposeWith(disposables);

                this.BindValidation(ViewModel, vm => vm.Image, v => v._imageError.Text)
                    .DisposeWith(disposables);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void onImageButtonPointerReleased(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                this.ViewModel?.ChooseImageCommand.Execute().Subscribe();
            else if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
                this.ViewModel?.ClearImageCommand.Execute().Subscribe();
        }

        private async Task DoShowChooseImageDialogAsync(InteractionContext<string[], string?> interaction)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (interaction.Input.Length > 0)
                dialog.Filters.Add(new FileDialogFilter() { Name = "Image", Extensions = interaction.Input.ToList() });
            dialog.AllowMultiple = false;

            string[] results = await dialog.ShowAsync(this);
            string? result = results?.FirstOrDefault();

            interaction.SetOutput(result);
        }
    }
}
