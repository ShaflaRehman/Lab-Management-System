using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LabManagement
{
    /// <summary>
    /// Interaction logic for ComponentTrackOptionsWindow.xaml
    /// </summary>
    public partial class ComponentTrackOptionsWindow : Window
    {
        public ComponentTrackOptionsWindow()
        {
            InitializeComponent();
        }
        private void TrackById_Click(object sender, RoutedEventArgs e)
        {
            ComponentTrackWindow window = new ComponentTrackWindow(); // Your existing window
            window.ShowDialog();
        }

        private void TrackQuantity_Click(object sender, RoutedEventArgs e)
        {
            ComponentQuantityWindow quantityWindow = new ComponentQuantityWindow();
            quantityWindow.ShowDialog();
        }

        private void TrackByLab_Click(object sender, RoutedEventArgs e)
        {
            ComponentByLabWindow labWindow = new ComponentByLabWindow();
            labWindow.ShowDialog();
        }
    }
}
