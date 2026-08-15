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
    /// Interaction logic for SelectIssuanceTypeWindow.xaml
    /// </summary>
    public partial class SelectIssuanceTypeWindow : Window
    {
        public SelectIssuanceTypeWindow()
        {
            InitializeComponent();
        }

        private void ById_Click(object sender, RoutedEventArgs e)
        {
            ComponentIssuanceWindow selectionWindow = new ComponentIssuanceWindow();
            selectionWindow.ShowDialog();
        }

        private void LabWise_Click(object sender, RoutedEventArgs e)
        {
            ComponentIssuanceWindow selectionWindow = new ComponentIssuanceWindow();
            selectionWindow.ShowDialog();
        }
    }
}
