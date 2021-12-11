using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Groover.AvaloniaUI.ViewModels;

namespace Groover.AvaloniaUI.Views
{
    public partial class HeaderView : ReactiveUserControl<AppViewModel>
    {
        public HeaderView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
