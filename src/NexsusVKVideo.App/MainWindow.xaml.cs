using NexsusVKVideo.App.ViewModels;
using System.Windows;

namespace NexsusVKVideo.App;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
