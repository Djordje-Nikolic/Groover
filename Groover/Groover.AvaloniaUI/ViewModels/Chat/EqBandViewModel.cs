using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels.Chat
{
    public class EqBandViewModel : ViewModelBase
    {
        public float Frequency { get; }
        public uint BandIndex { get; }

        public string Name { get; }
        public float Maximum { get; }
        public float Minimum { get; }

        [Reactive]
        public float Value { get; set; }

        private ReactiveCommand<(float NewAmp, uint Index), Unit> OnNewAmpValueCommand { get; }

        public EqBandViewModel(
            float freq, 
            uint index,
            float startAmp,
            float maxVal,
            float minVal,
            string? name = null,
            ReactiveCommand<(float NewAmp, uint Index), Unit>? onNewAmpValueCommand = null)
        {
            Frequency = freq;
            BandIndex = index;

            Value = startAmp;
            Maximum = maxVal;
            Minimum = minVal;

            Name = name ?? Math.Round(Frequency).ToString("0");
            OnNewAmpValueCommand = onNewAmpValueCommand;

            if (OnNewAmpValueCommand != null)
                this.WhenAnyValue(vm => vm.Value)
                    .Select<float, (float NewAmp, uint Index)>(amp => (amp, BandIndex))
                    .InvokeCommand(OnNewAmpValueCommand);
        }

    }
}
