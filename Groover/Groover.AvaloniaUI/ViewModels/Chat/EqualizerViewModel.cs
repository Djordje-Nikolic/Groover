using Groover.AvaloniaUI.Services.Interfaces;
using LibVLCSharp.Shared;
using ReactiveUI.Fody.Helpers;
using System;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;

namespace Groover.AvaloniaUI.ViewModels.Chat
{
    public class EqualizerViewModel : ViewModelBase
    {
        [Reactive]
        public EqBandViewModel Band32 { get; private set; }
        [Reactive]
        public EqBandViewModel Band64 { get; private set; }
        [Reactive]
        public EqBandViewModel Band125 { get; private set; }
        [Reactive]
        public EqBandViewModel Band250 { get; private set; }
        [Reactive]
        public EqBandViewModel Band500 { get; private set; }
        [Reactive]
        public EqBandViewModel Band1K { get; private set; }
        [Reactive]
        public EqBandViewModel Band2K { get; private set; }
        [Reactive]
        public EqBandViewModel Band4K { get; private set; }
        [Reactive]
        public EqBandViewModel Band8K { get; private set; }
        [Reactive]
        public EqBandViewModel Band16K { get; private set; }
        [Reactive]
        public PreampViewModel Preamp { get; private set; }

        [Reactive]
        public string? ErrorDisplay { get; private set; }
        [Reactive]
        public bool IsOn { get; set; }

        public float Maximum { get; }
        public float Minimum { get; }

        private IVLCWrapper _vlcWrapper { get; }
        private MediaPlayer _mediaPlayer { get; }

        private Equalizer _equalizer;
        public Equalizer Equalizer 
        {
            get => _equalizer;
            set
            {
                this.RaiseAndSetIfChanged(ref _equalizer, value);
            }
        }

        public ReactiveCommand<(float NewAmp, uint Index), Unit> NewAmpCommand { get; }
        public ReactiveCommand<float, Unit> NewPreampCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetEqCommand { get; }
        public ReactiveCommand<Unit, Unit> SetEqCommand { get; }
        public ReactiveCommand<Unit, Unit> RemoveEqCommand { get; }

        public EqualizerViewModel(MediaPlayer mediaPlayer, IVLCWrapper vlcWrapper)
        {
            Maximum = 12;
            Minimum = -12;

            IsOn = false;
            ErrorDisplay = null;
            _vlcWrapper = vlcWrapper ?? throw new ArgumentNullException(nameof(vlcWrapper));
            _mediaPlayer = mediaPlayer ?? throw new ArgumentNullException(nameof(vlcWrapper));
            _equalizer = _vlcWrapper.GetEqualizer();

            NewAmpCommand = ReactiveCommand.Create<(float NewAmp, uint Index)>(vals => OnAmpChange(vals.NewAmp, vals.Index));
            NewPreampCommand = ReactiveCommand.Create<float>(OnPreampChange);
            RemoveEqCommand = ReactiveCommand.Create(RemoveEq);

            var canSetEq = this.WhenAnyValue(vm => vm.IsOn, isOn => isOn == true);
            SetEqCommand = ReactiveCommand.Create(SetEq, canSetEq);
            ResetEqCommand = ReactiveCommand.Create(Reset, canSetEq);

            NewAmpCommand.InvokeCommand(SetEqCommand);
            NewPreampCommand.InvokeCommand(SetEqCommand);
            ResetEqCommand.InvokeCommand(SetEqCommand);

            this.WhenAnyValue(vm => vm.IsOn)
                .Where(isOn => isOn == true)
                .Select(_ => Unit.Default)
                .InvokeCommand(SetEqCommand);

            this.WhenAnyValue(vm => vm.IsOn)
                .Where(isOn => isOn == false)
                .Select(_ => Unit.Default)
                .InvokeCommand(RemoveEqCommand);

            InitializeBands();
        }

        private void InitializeBands()
        {
            var listOfBands = new List<(float Frequency, uint Index)>();

            uint bandCount = _equalizer.BandCount;
            for (uint bandIndex = 0; bandIndex < bandCount; bandIndex++)
            {
                var freq = _equalizer.BandFrequency(bandIndex);
                listOfBands.Add((freq, bandIndex));
            }

            Band32 = GetBand(32, 2, listOfBands);
            Band64 = GetBand(64, 2, listOfBands);
            Band125 = GetBand(125, 2, listOfBands);
            Band250 = GetBand(250, 2, listOfBands);
            Band500 = GetBand(500, 2, listOfBands);
            Band1K = GetBand(1000, 2, listOfBands, "1K");
            Band2K = GetBand(2000, 2, listOfBands, "2K");
            Band4K = GetBand(4000, 2, listOfBands, "4K");
            Band8K = GetBand(8000, 2, listOfBands, "8K");
            Band16K = GetBand(16000, 2, listOfBands, "16K");
            Preamp = GetPreamp();
        }

        private void OnAmpChange(float newAmp, uint index)
        {
            Equalizer.SetAmp(newAmp, index);
        }

        private void OnPreampChange(float newPreamp)
        {
            Equalizer.SetPreamp(newPreamp);
        }

        private void Reset()
        {
            Equalizer = _vlcWrapper.GetEqualizer();
            InitializeBands();
        }

        private void RemoveEq()
        {
            _mediaPlayer.UnsetEqualizer();
        }

        private void SetEq()
        {
            _mediaPlayer.SetEqualizer(Equalizer);
        }

        private EqBandViewModel? GetBand(float baseFreq, float vicinity, List<(float Frequency, uint Index)> listToLookFrom, string? displayName = null)
        {          
            (float Frequency, uint Index)? tuple = listToLookFrom.FirstOrDefault(band => Math.Abs(band.Frequency - baseFreq) < vicinity);

            if (tuple == null)
            {
                ErrorDisplay = $"Couldn't find a band for a base frequency of {baseFreq}.";
                return null;
            }

            var band = tuple.Value;
            float startVal = _equalizer.Amp(band.Index);
            return new EqBandViewModel(band.Frequency, band.Index, startVal, this.Maximum, this.Minimum, name: displayName, onNewAmpValueCommand: NewAmpCommand);
        }

        private PreampViewModel? GetPreamp()
        {
            float startVal = _equalizer.Preamp;
            return new PreampViewModel(startVal, this.Maximum, this.Minimum, NewPreampCommand);
        }
    }
}
