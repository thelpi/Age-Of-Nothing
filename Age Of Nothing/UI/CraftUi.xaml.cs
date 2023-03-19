using System.Windows;
using System.Windows.Controls;

namespace Age_Of_Nothing.UI
{
    /// <summary>
    /// Logique d'interaction pour CraftUi.xaml
    /// </summary>
    public partial class CraftUi : UserControl
    {
        public Craft Craft { get; }

        public string TargetType => Craft.Target.GetType().Name;

        public CraftUi(Craft craft)
        {
            InitializeComponent();
            Craft = craft;
            DataContext = this;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Craft.Cancel();
        }
    }
}
