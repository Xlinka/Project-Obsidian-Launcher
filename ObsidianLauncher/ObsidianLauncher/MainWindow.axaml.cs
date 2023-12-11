using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ObsidianLauncher.ViewModels;

namespace ObsidianLauncher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            DataContext = new MainWindowViewModel(this);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
