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
using Microsoft.Data.SqlClient;

namespace LabManagement
{
    /// <summary>
    /// Interaction logic for AttendantDashboard.xaml
    /// </summary>
    public partial class AttendantDashboard : Window
    {
        public AttendantDashboard()
        {
            InitializeComponent();
        }

        private void ComponentIssuance_Click(object sender, RoutedEventArgs e)
        {
            SelectIssuanceTypeWindow selectionWindow = new SelectIssuanceTypeWindow();
            selectionWindow.ShowDialog();
        }

        private void AddComponent_Click(object sender, RoutedEventArgs e)
        {
            int labId = SessionManager.LabId;
            int attendantId = SessionManager.LabAttendantId;

            AddComponentWindow addWindow = new AddComponentWindow(labId, attendantId);
            addWindow.ShowDialog();
        }

        private void ComponentReturn_Click(object sender, RoutedEventArgs e)
        {
            ComponentReturnWindow returnWindow = new ComponentReturnWindow();
            returnWindow.ShowDialog();
        

        }

        private void ComponentTrack_Click(object sender, RoutedEventArgs e)
        {
            ComponentTrackOptionsWindow optionsWindow = new ComponentTrackOptionsWindow();
            optionsWindow.ShowDialog();

        }

        private void StatusUpdate_Click(object sender, RoutedEventArgs e)
        {
            StatusUpdateWindow window = new StatusUpdateWindow();
            window.ShowDialog();
      
        }

        private void RequestComponent_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Invalid credentials!");
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            // Find and show the previous window
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                mainWindow.Show();
            }
            this.Close();
        }

        private void RemoveComponent_Click(object sender, RoutedEventArgs e)
        {
            RemoveComponentWindow removeWindow = new RemoveComponentWindow();
            removeWindow.Show();
        }
    }
}
