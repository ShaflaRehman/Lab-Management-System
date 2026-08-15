using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace LabManagement
{
    public partial class StatusUpdateWindow : Window
    {
        private string connectionString = "Server=DESKTOP-7C4SMS3\\SQLEXPRESS;Database=LabInventoryManagemnet;User Id=sa;Password=abc123;TrustServerCertificate=True;";
        private string componentId;

        public StatusUpdateWindow()
        {
            InitializeComponent();
        }

        private void FetchStatus_Click(object sender, RoutedEventArgs e)
        {
            componentId = ComponentIdBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(componentId))
            {
                MessageBox.Show("Please enter a valid Component ID.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT WorkingCondition FROM Components WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", componentId);
                var result = cmd.ExecuteScalar();

                if (result != null)
                {
                    CurrentStatusTextBlock.Text = result.ToString();
                    StatusComboBox.Visibility = Visibility.Visible;
                    UpdateStatusButton.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageBox.Show("Component not found.");
                }
            }
        }

        private void UpdateStatus_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = StatusComboBox.SelectedItem;

            if (selectedItem != null)
            {
                string newStatus;

                if (selectedItem is ComboBoxItem comboBoxItem)
                {
                    newStatus = comboBoxItem.Content.ToString();
                }
                else
                {
                    newStatus = selectedItem.ToString();
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE Components SET WorkingCondition = @status WHERE Id = @id", conn);
                    cmd.Parameters.AddWithValue("@status", newStatus);
                    cmd.Parameters.AddWithValue("@id", componentId);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show($"Status updated to: {newStatus}");
                this.Close();
            }
            else
            {
                MessageBox.Show("Please select a status to update.");
            }
        }

    }
}
