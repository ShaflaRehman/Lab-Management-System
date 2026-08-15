using System;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace LabManagement
{
    public partial class ComponentReturnWindow : Window
    {
        private string connectionString = "Server=DESKTOP-7C4SMS3\\SQLEXPRESS;Database=LabInventoryManagemnet;User Id=sa;Password=abc123;TrustServerCertificate=True;";

        public ComponentReturnWindow()
        {
            InitializeComponent();
        }

        private void ReturnComponent_Click(object sender, RoutedEventArgs e)
        {
            int componentId;
            if (!int.TryParse(ComponentIdBox.Text.Trim(), out componentId))
            {
                MessageBox.Show("Please enter a valid numeric Component ID.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // 1. Update Components table to set AvailabilityStatus to 'Available'
                var updateComponentCmd = new SqlCommand("UPDATE Components SET AvailabilityStatus = 'Available' WHERE Id = @id", conn);
                updateComponentCmd.Parameters.AddWithValue("@id", componentId);
                int rowsAffected = updateComponentCmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    MessageBox.Show("Component not found.");
                    return;
                }

                // 2. Update Transactions table - set DateReturned for the most recent issued one
                var updateTransactionCmd = new SqlCommand(@"
                    WITH ClosestTransaction AS (
                        SELECT TOP 1 TransactionId
                        FROM Transactions
                        WHERE ComponentId = @id AND DateReturned IS NULL
                        ORDER BY ABS(DATEDIFF(DAY, DateIssued, GETDATE()))
                    )
                    UPDATE Transactions
                    SET DateReturned = GETDATE()
                    WHERE TransactionId = (SELECT TransactionId FROM ClosestTransaction);
                ", conn);
                updateTransactionCmd.Parameters.AddWithValue("@id", componentId);

                int txnRows = updateTransactionCmd.ExecuteNonQuery();
                if (txnRows == 0)
                {
                    MessageBox.Show("No matching transaction found to update.");
                }
                else
                {
                    MessageBox.Show("Component marked as returned and status updated successfully.");
                }
            }

            this.Close();
        }
    }
}
