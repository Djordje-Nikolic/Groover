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

namespace Groover.AvaloniaUI.Views
{
    public partial class AppView : ReactiveUserControl<AppViewModel>
    {
        private Panel chatContainerPanel;

        public AppView()
        {
            InitializeComponent();

            //chatContainerPanel = this.FindControl<Panel>("ChatContainerPanel");

            //this.WhenActivated(disposables =>
            //{
            //    this.WhenAny(v => v.ViewModel.ChatViewModels)


            //});
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
