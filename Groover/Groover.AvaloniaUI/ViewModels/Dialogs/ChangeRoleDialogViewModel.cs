using Groover.AvaloniaUI.Models;
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
    public class ChangeRoleDialogViewModel : ViewModelBase
    {
        [Reactive]
        public string TitleText { get; set; }
        [Reactive]
        public string YesButtonText { get; set; }
        [Reactive]
        public string NoButtonText { get; set; }

        [Reactive]
        public GrooverGroupRole ChosenRole { get; set; }

        [Reactive]
        public List<GrooverGroupRole> PossibleRoles { get; set; }

        public ReactiveCommand<Unit, GrooverGroupRole?> YesCommand { get; }
        public ReactiveCommand<Unit, GrooverGroupRole?> NoCommand { get; }

        public ChangeRoleDialogViewModel(GrooverGroupRole? currentRole = null)
        {
            TitleText = "Choose group role";
            YesButtonText = "CHOOSE";
            NoButtonText = "CANCEL";

            //Possibly remove currentRole from the following list
            var listofroles = Enum.GetValues(typeof(GrooverGroupRole)).Cast<GrooverGroupRole>().ToList();
            if (currentRole != null)
                listofroles.Remove(currentRole.Value);
            PossibleRoles = listofroles;

            YesCommand = ReactiveCommand.Create<Unit, GrooverGroupRole?>(x => ChosenRole);
            NoCommand = ReactiveCommand.Create<Unit, GrooverGroupRole?>(x => null);
        }
    }
}
