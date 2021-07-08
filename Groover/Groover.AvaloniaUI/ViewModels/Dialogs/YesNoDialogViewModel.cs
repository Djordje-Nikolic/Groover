using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels.Dialogs
{
    public class YesNoDialogViewModel : ViewModelBase
    {
        [Reactive]
        public string TitleText { get; set; }
        [Reactive]
        public string BodyText { get; set; }
        [Reactive]
        public string YesButtonText { get; set; }
        [Reactive]
        public string NoButtonText { get; set; }

        public ReactiveCommand<Unit, bool> YesCommand { get; }
        public ReactiveCommand<Unit, bool> NoCommand { get; }

        public YesNoDialogViewModel(string bodyText, string titleText)
        {
            TitleText = titleText;
            BodyText = bodyText;
            YesButtonText = "Yes";
            NoButtonText = "No";

            YesCommand = ReactiveCommand.Create<Unit, bool>(x => true);
            NoCommand = ReactiveCommand.Create<Unit, bool>(x => false);
        }
    }
}
