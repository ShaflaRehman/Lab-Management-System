using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace LabManagement
{
    public partial class ComponentQuantityWindow : Window
    {
        private string connectionString = "Server=DESKTOP-7C4SMS3\\SQLEXPRESS;Database=LabInventoryManagemnet;User Id=sa;Password=abc123;TrustServerCertificate=True;";

        public ComponentQuantityWindow()
        {
            InitializeComponent();
            LoadComponentNames();
        }

        private void LoadComponentNames()
        {
            List<string> names = new List<string>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string nameQuery = "SELECT DISTINCT Name FROM Components WHERE Name IS NOT NULL";

                try
                {
                    conn.Open();
                    using (SqlCommand nameCmd = new SqlCommand(nameQuery, conn))
                    using (SqlDataReader reader = nameCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            names.Add(reader.GetString(0));
                        }
                    }

                    NameComboBox.ItemsSource = names;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading component names: {ex.Message}");
                }
            }
        }

        private void NameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SubtypeComboBox.ItemsSource = null;
            string selectedName = NameComboBox.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(selectedName)) return;

            List<string> subtypes = new List<string>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string subtypeQuery = "SELECT DISTINCT Subtype FROM Components WHERE Name = @name AND Subtype IS NOT NULL";

                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(subtypeQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", selectedName);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                subtypes.Add(reader.GetString(0));
                            }
                        }
                    }

                    SubtypeComboBox.ItemsSource = subtypes;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading subtypes: {ex.Message}");
                }
            }
        }

        private void FetchQuantity_Click(object sender, RoutedEventArgs e)
        {
            string selectedName = NameComboBox.SelectedItem as string;
            string selectedSubtype = SubtypeComboBox.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(selectedName) && string.IsNullOrWhiteSpace(selectedSubtype))
            {
                MessageBox.Show("Please select at least a Name or Subtype.");
                return;
            }

            string query = "SELECT COUNT(*) FROM Components WHERE AvailabilityStatus = 'Available'";
            if (!string.IsNullOrWhiteSpace(selectedName))
                query += " AND Name = @name";
            if (!string.IsNullOrWhiteSpace(selectedSubtype))
                query += " AND Subtype = @subtype";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                if (!string.IsNullOrWhiteSpace(selectedName))
                    cmd.Parameters.AddWithValue("@name", selectedName);
                if (!string.IsNullOrWhiteSpace(selectedSubtype))
                    cmd.Parameters.AddWithValue("@subtype", selectedSubtype);

                try
                {
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    QuantityResult.Text = $"Available Quantity: {count}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error fetching quantity: {ex.Message}");
                }
            }
        }
    }
}
