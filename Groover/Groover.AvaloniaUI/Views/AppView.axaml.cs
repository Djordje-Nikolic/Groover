using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Groover.AvaloniaUI.ViewModels;
using Groover.AvaloniaUI.ViewModels.Dialogs;
using Groover.AvaloniaUI.Views.Dialogs;
using ReactiveUI;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Reactive.Disposables;
using System.Reactive;

namespace Groover.AvaloniaUI.Views
{
    public partial class AppView : ReactiveUserControl<AppViewModel>
    {
        private Panel chatContainerPanel;
        //public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

        public AppView()
        {
            InitializeComponent();

            //chatContainerPanel = this.FindControl<Panel>("ChatContainerPanel");
            //LogoutCommand = ReactiveCommand.Create(() => { });

            //this.WhenActivated(disposables =>
            //{
            //    this.WhenAnyValue(x => x.ViewModel)
            //        .WhereNotNull()
            //        .Subscribe(x => x.LogoutCommand.Subscribe(x =>
            //        {
            //            if (x == true)
            //                LogoutCommand.Execute().Subscribe();

            //        }).DisposeWith(disposables))
            //        .DisposeWith(disposables);
            //});
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
