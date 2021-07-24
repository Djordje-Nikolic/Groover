using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Groover.AvaloniaUI.Models;
using Groover.AvaloniaUI.Models.DTOs;
using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.Utils;
using Groover.AvaloniaUI.ViewModels;
using Groover.AvaloniaUI.ViewModels.Dialogs;
using Groover.AvaloniaUI.Views.Dialogs;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private AppView _mainView;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            _mainView = this.FindControl<AppView>("mainView");

            this.WhenActivated(disposables =>
            {
                ViewModel.ShowWelcomeDialog
                .RegisterHandler(DoShowWelcomeDialogAsync)
                .DisposeWith(disposables);

                ViewModel.ShowYesNoDialog
                .RegisterHandler(DoShowYesNoDialogAsync)
                .DisposeWith(disposables);

                ViewModel.ShowGroupRoleDialog
                .RegisterHandler(DoShowGroupRoleDialogAsync)
                .DisposeWith(disposables);

                ViewModel.ShowUserEditDialog
                .RegisterHandler(DoShowUserEditDialogAsync)
                .DisposeWith(disposables);

                ViewModel.ShowUserSearchDialog
                .RegisterHandler(DoShowUserSearchDialogAsync)
                .DisposeWith(disposables);

                ViewModel.ShowGroupEditDialog
                .RegisterHandler(DoShowGroupEditDialogAsync)
                .DisposeWith(disposables);

                //this._mainView.LogoutCommand.Subscribe(x => ShowWelcomeDialog());

                this.WhenAnyValue(v => v.ViewModel.WelcomeDialogResult)
                .Select(result => result?.AppViewModel)
                .Do(vm => 
                 {
                     if (vm != null)
                     {
                         vm.ShowYesNoDialog = ViewModel.ShowYesNoDialog;
                         vm.ShowGroupRoleDialog = ViewModel.ShowGroupRoleDialog;
                         vm.ShowUserSearchDialog = ViewModel.ShowUserSearchDialog;
                         vm.ShowGroupEditDialog = ViewModel.ShowGroupEditDialog;
                         vm.ShowUserEditDialog = ViewModel.ShowUserEditDialog;
                         vm.LogoutCommand.Subscribe(x =>
                         {
                             if (x == true)
                                 ShowWelcomeDialog();
                         }).DisposeWith(disposables);
                     }
                 })
                .BindTo(this, x => x._mainView.DataContext)
                .DisposeWith(disposables);

                this.WhenAnyValue(v => v.ViewModel.WelcomeDialogResult)
                .Select(result => result?.ExitApp)
                .Where(val => val != null && val == true)
                .Subscribe(val => this.Close())
                .DisposeWith(disposables);
            });

            this.Opened += DoOnOpen;
        }

        private void DoOnOpen(object? sender, EventArgs e)
        {
            ShowWelcomeDialog();
        }

        private void ShowWelcomeDialog()
        {
            this.ViewModel.WelcomeDialogCommand.Execute().Subscribe();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async Task DoShowWelcomeDialogAsync(InteractionContext<WelcomeViewModel, WelcomeDialogResult?> interaction)
        {
            var dialog = new WelcomeWindow();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<WelcomeDialogResult?>(this);
            interaction.SetOutput(result);
        }

        private async Task DoShowYesNoDialogAsync(InteractionContext<YesNoDialogViewModel, bool> interaction)
        {
            var dialog = new YesNoDialogView();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<bool>(this);
            interaction.SetOutput(result);
        }

        private async Task DoShowGroupRoleDialogAsync(InteractionContext<ChangeRoleDialogViewModel, GrooverGroupRole?> interaction)
        {
            var dialog = new ChangeRoleDialogView();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<GrooverGroupRole?>(this);
            interaction.SetOutput(result);
        }

        private async Task DoShowUserSearchDialogAsync(InteractionContext<ChooseUserDialogViewModel, int?> interaction)
        {
            var dialog = new ChooseUserDialogView();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<int?>(this);
            interaction.SetOutput(result);
        }

        private async Task DoShowGroupEditDialogAsync(InteractionContext<BaseGroupViewModel, GroupResponse?> interaction)
        {
            var dialog = new GroupDialogView();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<GroupResponse?>(this);
            interaction.SetOutput(result);
        }

        private async Task DoShowUserEditDialogAsync(InteractionContext<EditUserDialogViewModel, UserResponse?> interaction)
        {
            var dialog = new EditUserDialogView();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<UserResponse?>(this);
            interaction.SetOutput(result);
        }
    }
}
