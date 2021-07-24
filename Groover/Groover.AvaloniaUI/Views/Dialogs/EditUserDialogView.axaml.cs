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
    public partial class EditUserDialogView : ReactiveWindow<EditUserDialogViewModel>
    {
        private ProgressBar _progressBar;
        private TextBlock _imageError;
        public EditUserDialogView()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.InitializeComponent();

            _progressBar = this.FindControl<ProgressBar>("isUpdating");
            _imageError = this.FindControl<TextBlock>("imageError");
            var imageButton = this.FindControl<Button>("chooseImageButton");
            imageButton.AddHandler(PointerPressedEvent, onImageButtonPointerReleased, handledEventsToo: true);
#if DEBUG
            this.AttachDevTools();
#endif

            this.WhenActivated(disposables =>
            {
                ViewModel.YesCommand.Subscribe(x => { if (x.IsSuccessful) this.Close(x); }).DisposeWith(disposables);
                ViewModel.NoCommand.Subscribe(x => this.Close(x)).DisposeWith(disposables);

                ViewModel.ShowChooseImageDialog
                    .RegisterHandler(DoShowChooseImageDialogAsync)
                    .DisposeWith(disposables);

                this.WhenAnyObservable(v => v.ViewModel.YesCommand.IsExecuting)
                    .BindTo(this, x => x._progressBar.IsVisible)
                    .DisposeWith(disposables);

                this.BindValidation(ViewModel, vm => vm.AvatarImage, v => v._imageError.Text)
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
                this.ViewModel?.ChooseImage.Execute().Subscribe();
            else if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
                this.ViewModel?.ClearImage.Execute().Subscribe();
        }

        private async Task DoShowChooseImageDialogAsync(InteractionContext<string[], string?> interaction)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filters.Add(new FileDialogFilter() { Name = "Image", Extensions = interaction.Input.ToList() });
            dialog.AllowMultiple = false;

            string[] results = await dialog.ShowAsync(this);
            string? result = results?.FirstOrDefault();

            interaction.SetOutput(result);
        }
    }
}
