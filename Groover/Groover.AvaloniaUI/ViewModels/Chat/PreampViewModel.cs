using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels.Chat
{
    public class PreampViewModel : ViewModelBase
    {
        public string Name { get; }
        public float Maximum { get; }
        public float Minimum { get; }

        [Reactive]
        public float Value { get; set; }

        private ReactiveCommand<float, Unit> OnNewPreampValueCommand { get; }

        public PreampViewModel(
            float startAmp,
            float maxVal,
            float minVal,
            ReactiveCommand<float, Unit>? onNewPreampValueCommand = null)
        {
            Value = startAmp;
            Maximum = maxVal;
            Minimum = minVal;

            Name = "Preamp";
            OnNewPreampValueCommand = onNewPreampValueCommand;

            if (OnNewPreampValueCommand != null)
                this.WhenAnyValue(vm => vm.Value)
                    .InvokeCommand(OnNewPreampValueCommand);
        }
    }
}
