using Groover.AvaloniaUI.Models;
using Groover.AvaloniaUI.Utils;
using Groover.AvaloniaUI.ViewModels.Dialogs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Groover.AvaloniaUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        //Change this to return some ViewModel if necessary
        public Interaction<WelcomeViewModel, WelcomeDialogResult?> ShowWelcomeDialog { get; }
        public Interaction<ChangeRoleDialogViewModel, GrooverGroupRole?> ShowGroupRoleDialog { get; set; }
        public Interaction<ChooseUserDialogViewModel, int?> ShowUserSearchDialog { get; set; }
        public Interaction<YesNoDialogViewModel, bool> ShowYesNoDialog { get; set; }
        public ReactiveCommand<Unit, Unit> WelcomeDialogCommand { get; }

        [ObservableAsProperty]
        public WelcomeDialogResult WelcomeDialogResult { get; }

        public MainWindowViewModel()
        {
            ShowYesNoDialog = new Interaction<YesNoDialogViewModel, bool>();
            ShowWelcomeDialog = new Interaction<WelcomeViewModel, WelcomeDialogResult?>();
            ShowGroupRoleDialog = new Interaction<ChangeRoleDialogViewModel, GrooverGroupRole?>();
            ShowUserSearchDialog = new Interaction<ChooseUserDialogViewModel, int?>();

            WelcomeDialogCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                WelcomeViewModel welcomeVm = DIContainer.GetRequiredService<WelcomeViewModel>(Locator.Current);

                ReactiveCommand<WelcomeDialogResult?, WelcomeDialogResult?> helperCommand = ReactiveCommand.Create<WelcomeDialogResult?, WelcomeDialogResult?>(x => x);
                helperCommand.ToPropertyEx(this, x => x.WelcomeDialogResult);

                var result = await ShowWelcomeDialog.Handle(welcomeVm);

                helperCommand.Execute(result).Subscribe();

                //This throws "Call from invalid thread" in the view
                //ShowWelcomeDialog.Handle(welcomeVm).ToPropertyEx(this, x => x.WelcomeDialogResult);
            });
        }
    }
}
