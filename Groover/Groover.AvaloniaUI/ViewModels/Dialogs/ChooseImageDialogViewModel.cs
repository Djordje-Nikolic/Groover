using Avalonia.Media.Imaging;
using Groover.AvaloniaUI.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels.Dialogs
{
    public class ChooseImageDialogViewModel : ReactiveValidationObject
    {
        private ImageConfiguration _config;
        public Interaction<string[], string?> ShowChooseFileDialog { get; }

        [Reactive]
        public string? ChosenFilePath { get; private set; }
        [ObservableAsProperty]
        public Bitmap? Image { get; }

        public ReactiveCommand<Unit, string?> YesCommand { get; }
        public ReactiveCommand<Unit, string?> NoCommand { get; }
        public ReactiveCommand<Unit, Unit> ChooseImageCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearImageCommand { get; }

        public ChooseImageDialogViewModel(ImageConfiguration config)
        {
            _config = config;
            ShowChooseFileDialog = new Interaction<string[], string?>();
            ChosenFilePath = null;
            Image = null;

            var canYes = this.WhenAnyValue(vm => vm.ChosenFilePath, filepath => !string.IsNullOrWhiteSpace(filepath));
            YesCommand = ReactiveCommand.Create<string?>(() => ChosenFilePath, this.IsValid());
            NoCommand = ReactiveCommand.Create<string?>(() => { return null; });
            ChooseImageCommand = ReactiveCommand.CreateFromTask(ChooseImage);
            ClearImageCommand = ReactiveCommand.Create(ClearImage);

            this.WhenAnyValue(vm => vm.ChosenFilePath)
                .Do(val => { if (Image != null) Image.Dispose(); })
                .Select(val =>
                {
                    if (val != null && File.Exists(val))
                        return new Bitmap(val);
                    else
                        return null;
                })
                .ToPropertyEx(this, vm => vm.Image);

            InitializeValidations();
        }

        private async Task ChooseImage()
        {
            ChosenFilePath = await ShowChooseFileDialog.Handle(_config.AllowedExtensions.ToArray());
        }

        private void ClearImage()
        {
            ChosenFilePath = null;
        }

        private void InitializeValidations()
        {
            this.ValidationRule(vm => vm.Image, image => image == null || image.Size.Width < _config.MaxWidth, $"Image too wide. Max width: {_config.MaxWidth} px");
            this.ValidationRule(vm => vm.Image, image => image == null || image.Size.Width > _config.MinWidth, $"Image too narrow. Min width: {_config.MinWidth} px");
            this.ValidationRule(vm => vm.Image, image => image == null || image.Size.Height < _config.MaxHeight, $"Image too tall. Max height: {_config.MaxHeight} px");
            this.ValidationRule(vm => vm.Image, image => image == null || image.Size.Height > _config.MinHeight, $"Image too short. Min height: {_config.MinHeight} px");    
            this.ValidationRule(vm => vm.Image, image => image != null, "No image chosen.");

            IObservable<bool> isValidSize = this.WhenAnyValue(vm => vm.ChosenFilePath,
                filepath => string.IsNullOrWhiteSpace(filepath) || 
                !File.Exists(filepath) ||
                new FileInfo(filepath).Length < (_config.MaxSizeInMb * 1024 * 1024));
            this.ValidationRule(vm => vm.Image, isValidSize, $"Image too big. Max size: {_config.MaxSizeInMb} mb");

            IObservable<bool> isValidExtension = this.WhenAnyValue(vm => vm.ChosenFilePath,
                filepath => string.IsNullOrWhiteSpace(filepath) ||
                !File.Exists(filepath) ||
                _config.AllowedExtensions.Contains(Path.GetExtension(filepath).TrimStart('.')));
            this.ValidationRule(vm => vm.Image, isValidExtension, $"Extension not supported. Allowed extensions: {string.Join(',', _config.AllowedExtensions)}");

            IObservable<bool> doesFileExist = this.WhenAnyValue(vm => vm.ChosenFilePath,
                filepath => string.IsNullOrWhiteSpace(filepath) ||
                File.Exists(filepath));
            this.ValidationRule(vm => vm.Image, doesFileExist, "Chosen file does not exist.");

            IObservable<bool> isPathValid = this.WhenAnyValue(vm => vm.ChosenFilePath,
                filepath => string.IsNullOrWhiteSpace(filepath) ||
                Path.IsPathFullyQualified(filepath));
            this.ValidationRule(vm => vm.Image, isPathValid, "Path to file is invalid.");
        }
    }
}
