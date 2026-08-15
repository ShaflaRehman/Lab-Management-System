using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace LabManagement
{
    public partial class ComponentTrackWindow : Window
    {
        private string connectionString = "Server=DESKTOP-7C4SMS3\\SQLEXPRESS;Database=LabInventoryManagemnet;User Id=sa;Password=abc123;TrustServerCertificate=True;";

        public ComponentTrackWindow()
        {
            InitializeComponent();
        }

        private void FetchTransactions_Click(object sender, RoutedEventArgs e)
        {
            string componentId = ComponentIdBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(componentId))
            {
                MessageBox.Show("Please enter a valid Component ID.");
                return;
            }

            string query = @"
                SELECT TransactionId, ComponentId, DateIssued, DateReturned, Purpose, Remarks 
                FROM Transactions 
                WHERE ComponentId = @componentId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@componentId", componentId);

                try
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        TransactionDataGrid.ItemsSource = dt.DefaultView;
                    }
                    else
                    {
                        MessageBox.Show("No transactions found for the given Component ID.");
                        TransactionDataGrid.ItemsSource = null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error fetching transaction details: {ex.Message}");
                }
            }
        }
    }
}
    

