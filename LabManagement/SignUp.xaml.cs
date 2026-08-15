using System.Windows;
using System.Windows.Controls;

namespace LabManagement
{
    public partial class SignupWindow : Window
    {
        public SignupWindow()
        {
            InitializeComponent();
            cbRole.SelectionChanged += cbRole_SelectionChanged;
        }

        private void cbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedRole = (cbRole.SelectedItem as ComboBoxItem)?.Content.ToString();

            bool isLabAttendant = selectedRole == "Lab Attendant";
            bool isStudent = selectedRole == "Student";

            // Get the parent Border elements
            var regNoBorder = (Border)txtRegNo.Parent;
            var degreeBorder = (Border)txtDegree.Parent;
            var syndicateBorder = (Border)txtSyndicate.Parent;
            var labNameBorder = (Border)cbLabName.Parent;

            // Set visibility on the Borders
            regNoBorder.Visibility = isStudent ? Visibility.Visible : Visibility.Collapsed;
            degreeBorder.Visibility = isStudent ? Visibility.Visible : Visibility.Collapsed;
            syndicateBorder.Visibility = isStudent ? Visibility.Visible : Visibility.Collapsed;
            labNameBorder.Visibility = isLabAttendant ? Visibility.Visible : Visibility.Collapsed;

            // Set visibility on the labels
            lblRegNo.Visibility = isStudent ? Visibility.Visible : Visibility.Collapsed;
            lblDegree.Visibility = isStudent ? Visibility.Visible : Visibility.Collapsed;
            lblSyndicate.Visibility = isStudent ? Visibility.Visible : Visibility.Collapsed;
            lblLabName.Visibility = isLabAttendant ? Visibility.Visible : Visibility.Collapsed;
        }





        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim().ToLower();
            string password = txtPassword.Password.Trim();
            string? role = (cbRole.SelectedItem as ComboBoxItem)?.Content.ToString();
            string phoneNumber = txtPhoneNumber.Text.Trim();
            string department = txtDepartment.Text.Trim();


            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(department) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (role == "Lab Attendant")
            {
                string selectedLab = (cbLabName.SelectedItem as ComboBoxItem)?.Content?.ToString();
                if (string.IsNullOrWhiteSpace(selectedLab))
                {
                    MessageBox.Show("Please select a lab.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int labId = DatabaseHelper.GetLabIdByName(selectedLab);
                if (!DatabaseHelper.IsLabAvailable(labId))
                {
                    MessageBox.Show("Selected lab is already assigned to another Lab Attendant.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Proceed with user registration
                if (DatabaseHelper.RegisterUser(username, email, password, role, phoneNumber, department, labId))
                {
                    MessageBox.Show("Registration successful! Please login.");
                    MainWindow login = new MainWindow();
                    login.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Registration failed. User may already exist.");
                }
            }
            else if (role == "Student")
            {
                string regNo = txtRegNo.Text.Trim();
                string degree = txtDegree.Text.Trim();
                string syndicate = txtSyndicate.Text.Trim();

                if (string.IsNullOrWhiteSpace(regNo) || string.IsNullOrWhiteSpace(degree) || string.IsNullOrWhiteSpace(syndicate))
                {
                    MessageBox.Show("Please enter registration number, degree, and syndicate.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (DatabaseHelper.RegisterUser(username, email, password, role, phoneNumber, department))
                {
                    int userId = DatabaseHelper.GetUserIdByEmail(email);
                    if (userId == -1)
                    {
                        MessageBox.Show("Failed to retrieve user ID after registration.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (DatabaseHelper.InsertStudentInfo(userId, regNo, degree, syndicate))
                    {
                        MessageBox.Show("Student registration successful! Please login.");
                        MainWindow login = new MainWindow();
                        login.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Failed to save student-specific details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Registration failed. User may already exist.");
                }
            }
            else
            {
                if (DatabaseHelper.RegisterUser(username, email, password, role, phoneNumber, department))
                {
                    MessageBox.Show("Registration successful! Please login.");
                    MainWindow login = new MainWindow();
                    login.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Registration failed. User may already exist.");
                }
            }
        }
            private void txtUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtUsername.Text == "Username" || txtUsername.FontStyle == FontStyles.Italic)
            {
                txtUsername.Text = "";
                txtUsername.FontStyle = FontStyles.Normal;
                txtUsername.Opacity = 1;
            }
        }

        private void txtUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                txtUsername.Text = "Username";
                txtUsername.FontStyle = FontStyles.Italic;
                txtUsername.Opacity = 0.7;
            }
        }

        // Email TextBox events
        private void txtEmail_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtEmail.Text == "Email" || txtEmail.FontStyle == FontStyles.Italic)
            {
                txtEmail.Text = "";
                txtEmail.FontStyle = FontStyles.Normal;
                txtEmail.Opacity = 1;
            }
        }

        private void txtEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                txtEmail.Text = "Email";
                txtEmail.FontStyle = FontStyles.Italic;
                txtEmail.Opacity = 0.7;
            }
        }

        // Password PasswordBox events
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

        // Phone Number TextBox events
        private void txtPhoneNumber_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtPhoneNumber.Text == "Phone Number" || txtPhoneNumber.FontStyle == FontStyles.Italic)
            {
                txtPhoneNumber.Text = "";
                txtPhoneNumber.FontStyle = FontStyles.Normal;
                txtPhoneNumber.Opacity = 1;
            }
        }

        private void txtPhoneNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPhoneNumber.Text))
            {
                txtPhoneNumber.Text = "Phone Number";
                txtPhoneNumber.FontStyle = FontStyles.Italic;
                txtPhoneNumber.Opacity = 0.7;
            }
        }

        // Department TextBox events
        private void txtDepartment_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtDepartment.Text == "Department" || txtDepartment.FontStyle == FontStyles.Italic)
            {
                txtDepartment.Text = "";
                txtDepartment.FontStyle = FontStyles.Normal;
                txtDepartment.Opacity = 1;
            }
        }

        private void txtDepartment_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDepartment.Text))
            {
                txtDepartment.Text = "Department";
                txtDepartment.FontStyle = FontStyles.Italic;
                txtDepartment.Opacity = 0.7;
            }
        }

        // Registration No TextBox events
        private void txtRegNo_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtRegNo.Text == "Registration No" || txtRegNo.FontStyle == FontStyles.Italic)
            {
                txtRegNo.Text = "";
                txtRegNo.FontStyle = FontStyles.Normal;
                txtRegNo.Opacity = 1;
            }
        }

        private void txtRegNo_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRegNo.Text))
            {
                txtRegNo.Text = "Registration No";
                txtRegNo.FontStyle = FontStyles.Italic;
                txtRegNo.Opacity = 0.7;
            }
        }

        // Degree TextBox events
        private void txtDegree_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtDegree.Text == "Degree" || txtDegree.FontStyle == FontStyles.Italic)
            {
                txtDegree.Text = "";
                txtDegree.FontStyle = FontStyles.Normal;
                txtDegree.Opacity = 1;
            }
        }

        private void txtDegree_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDegree.Text))
            {
                txtDegree.Text = "Degree";
                txtDegree.FontStyle = FontStyles.Italic;
                txtDegree.Opacity = 0.7;
            }
        }

        // Syndicate TextBox events
        private void txtSyndicate_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSyndicate.Text == "Syndicate" || txtSyndicate.FontStyle == FontStyles.Italic)
            {
                txtSyndicate.Text = "";
                txtSyndicate.FontStyle = FontStyles.Normal;
                txtSyndicate.Opacity = 1;
            }
        }

        private void txtSyndicate_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSyndicate.Text))
            {
                txtSyndicate.Text = "Syndicate";
                txtSyndicate.FontStyle = FontStyles.Italic;
                txtSyndicate.Opacity = 0.7;
            }
        }

        // Role Selection Changed
        

        
        private void LoginLink_Click(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }

    }
}
