using System;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace LabManagement
{
    public partial class RemoveComponentWindow : Window
    {
        public RemoveComponentWindow()
        {
            InitializeComponent();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            string componentId = ComponentIdTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(componentId))
            {
                MessageBox.Show("Please enter a valid Component ID.");
                return;
            }

            string connectionString = "Server=DESKTOP-7C4SMS3\\SQLEXPRESS;Database=LabInventoryManagemnet;User Id=sa;Password=abc123;TrustServerCertificate=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // First delete from Transactions table
                    SqlCommand deleteTransactions = new SqlCommand("DELETE FROM Transactions WHERE ComponentId = @ComponentId", conn);
                    deleteTransactions.Parameters.AddWithValue("@ComponentId", componentId);
                    deleteTransactions.ExecuteNonQuery();

                    // Then delete from Components table
                    SqlCommand deleteComponent = new SqlCommand("DELETE FROM Components WHERE Id = @ComponentId", conn);
                    deleteComponent.Parameters.AddWithValue("@ComponentId", componentId);
                    int rowsAffected = deleteComponent.ExecuteNonQuery();

                    if (rowsAffected > 0)
                        MessageBox.Show("Component removed successfully.");
                    else
                        MessageBox.Show("Component ID not found.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }

            this.Close();
        }
    }
}
