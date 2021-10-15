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
    public struct ChooseTrackResult
    {
        public string? FilePath { get; }
        public string? FileName { get; }

        public ChooseTrackResult(string? path, string? name)
        {
            FilePath = path;
            FileName = name;
        }
    }

    public class ChooseTrackDialogViewModel : ReactiveValidationObject
    {
        private TrackConfiguration _config;
        public Interaction<string[], string?> ShowChooseFileDialog { get; }

        [Reactive]
        public string TrackName { get; set; }
        [Reactive]
        public string? ChosenFilePath { get; private set; }
        [ObservableAsProperty]
        public FileInfo? ChosenFileInfo { get; }
        [ObservableAsProperty]
        public string FileSizeInMb { get; }

        public ReactiveCommand<Unit, ChooseTrackResult?> YesCommand { get; }
        public ReactiveCommand<Unit, ChooseTrackResult?> NoCommand { get; }
        public ReactiveCommand<Unit, Unit> ChooseTrackCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearTrackCommand { get; }

        public ChooseTrackDialogViewModel(TrackConfiguration config)
        {
            _config = config;
            ShowChooseFileDialog = new Interaction<string[], string?>();
            ChosenFilePath = null;

            var canYes = this.WhenAnyValue(vm => vm.ChosenFilePath, vm => vm.TrackName,
                (filepath, name) => 
                !string.IsNullOrWhiteSpace(filepath) &&
                !string.IsNullOrWhiteSpace(name));
            YesCommand = ReactiveCommand.Create<ChooseTrackResult?>(() => new ChooseTrackResult(ChosenFilePath, TrackName), this.IsValid());
            NoCommand = ReactiveCommand.Create<ChooseTrackResult?>(() => { return null; });
            ChooseTrackCommand = ReactiveCommand.CreateFromTask(ChooseTrack);
            ClearTrackCommand = ReactiveCommand.Create(ClearTrack);

            this.WhenAnyValue(vm => vm.ChosenFilePath)
                .Select(val =>
                {
                    if (val != null && File.Exists(val))
                        return new FileInfo(val);
                    else
                        return null;
                })
                .ToPropertyEx(this, vm => vm.ChosenFileInfo);

            this.WhenAnyValue(vm => vm.ChosenFileInfo)
                .WhereNotNull()
                .Subscribe(val => this.TrackName = val.Name);

            this.WhenAnyValue(vm => vm.ChosenFileInfo)
                .WhereNotNull()
                .Select(val => string.Format("{0:N2} Mb", val))
                .ToPropertyEx(this, vm => vm.FileSizeInMb);

            InitializeValidations();
        }

        private async Task ChooseTrack()
        {
            ChosenFilePath = await ShowChooseFileDialog.Handle(_config.AllowedExtensions.ToArray());
        }

        private void ClearTrack()
        {
            ChosenFilePath = null;
        }

        private void InitializeValidations()
        {
            this.ValidationRule(vm => vm.TrackName, name => !string.IsNullOrWhiteSpace(name), "Name cannot be empty.");
            this.ValidationRule(vm => vm.TrackName, name => name == null || name.Length < _config.MaxNameLength, $"Name is too long. Max length: {_config.MaxNameLength}");

            this.ValidationRule(vm => vm.ChosenFileInfo, info => info != null, "No file chosen.");
            this.ValidationRule(vm => vm.ChosenFileInfo, info => info == null || info.Exists, "Chosen file does not exist.");
            this.ValidationRule(vm => vm.ChosenFileInfo, info => info == null || !info.Exists || info.Length < _config.MaxTrackSize, 
                $"File too large. Max size: {string.Format("{0:N2} Kb", _config.MaxTrackSize / 1024 / 1024)}");
            this.ValidationRule(vm => vm.ChosenFileInfo, info => info == null || !info.Exists || _config.AllowedExtensions.Contains(info.Extension.TrimStart('.')),
                $"Extension not supported: Allowed extensions: {string.Join(',', _config.AllowedExtensions)}");
        }
    }
}
