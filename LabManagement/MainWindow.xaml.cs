using System.Windows;
using Microsoft.Data.SqlClient;
using System.Windows.Controls;

namespace LabManagement
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password.Trim();
            string role = (cbRole.SelectedItem as ComboBoxItem)?.Content.ToString();

            // First validate credentials before proceeding
            if (!DatabaseHelper.ValidateUser(username, password, role))
            {
                MessageBox.Show("Invalid credentials!");
                return; // Exit if credentials are invalid
            }

            // If we get here, credentials are valid
            if (role == "Lab Attendant")
            {
                using (SqlConnection conn = new SqlConnection(DatabaseHelper.connectionString))
                {
                    conn.Open();

                    // Step 1: Get UserId from Users table
                    SqlCommand cmdUser = new SqlCommand("SELECT Id FROM Users WHERE Username = @username", conn);
                    cmdUser.Parameters.AddWithValue("@username", username);
                    var userId = cmdUser.ExecuteScalar();

                    if (userId == null)
                    {
                        MessageBox.Show("User not found.");
                        return;
                    }

                    // Step 2: Get LabId from LabAttendants table using UserId
                    SqlCommand cmdLab = new SqlCommand("SELECT LabId FROM LabAttendants WHERE UserId = @userId", conn);
                    cmdLab.Parameters.AddWithValue("@userId", userId);
                    var labId = cmdLab.ExecuteScalar();

                    if (labId == null)
                    {
                        MessageBox.Show("Lab not found for this lab attendant.");
                        return;
                    }

                    // Store in session/global manager
                    SessionManager.LabAttendantId = (int)userId;
                    SessionManager.LabId = (int)labId;
                }
            }

            // Open the appropriate dashboard
            Window dashboard = null;
            switch (role)
            {
                case "HOD":
                    dashboard = new HODDashboard();
                    break;
                case "Student":
                    dashboard = new StudentDashboard();
                    break;
                case "Faculty":
                    dashboard = new InstructorDashboard();
                    break;
                case "Lab Attendant":
                    dashboard = new AttendantDashboard();
                    
                    break;
                default:
                    MessageBox.Show("Unknown role selected.");
                    return;
            }

            dashboard.Show();
            this.Hide(); // Close login window
        }

        



        private void SignUp_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SignupWindow signup = new SignupWindow();
            signup.Show();
            this.Close(); // Optional: close login window
        }

        private void txtPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            passwordPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void txtPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtPassword.Password))
            {
                passwordPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPassword.Password))
            {
                passwordPlaceholder.Visibility = Visibility.Collapsed;
            }
            else if (!txtPassword.IsFocused)
            {
                passwordPlaceholder.Visibility = Visibility.Visible;
            }
        }
        private void txtUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtUsername.Text == "Username")
            {
                txtUsername.Text = "";
                txtUsername.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#93C5FD"));
            }
        }

        private void txtUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                txtUsername.Text = "Username";
                txtUsername.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#93C5FD"));
            }
        }

    }
}
