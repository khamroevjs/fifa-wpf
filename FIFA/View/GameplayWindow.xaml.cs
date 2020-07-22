using System.Windows;
using FIFA.ViewModel;

namespace FIFA.View
{
    /// <summary>
    /// Interaction logic for GameplayWindow.xaml
    /// </summary>
    public partial class GameplayWindow : Window
    {
        public GameplayWindow(GameplayViewModel gameplayViewModel)
        {
            DataContext = gameplayViewModel;
            InitializeComponent();
        }

    }
}
