using System;
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace LabManagement
{
    public partial class AddComponentWindow : Window
    {
        private int labId;
        private int labAttendantId;

        public AddComponentWindow(int currentLabId, int currentAttendantId)
        {
            InitializeComponent();
            labId = currentLabId;
            labAttendantId = currentAttendantId;
        }

        private void GenerateFields_Click(object sender, RoutedEventArgs e)
        {
            ComponentIdPanel.Children.Clear();
            if (int.TryParse(QuantityTextBox.Text, out int quantity) && quantity > 0)
            {
                for (int i = 1; i <= quantity; i++)
                {
                    var label = new TextBlock { Text = $"Component ID {i}:", Margin = new Thickness(0, 5, 0, 0) };
                    var textBox = new TextBox { Name = $"ComponentIdBox{i}", Margin = new Thickness(0, 2, 0, 5) };
                    ComponentIdPanel.Children.Add(label);
                    ComponentIdPanel.Children.Add(textBox);
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid quantity.");
            }
        }

        private void AddMore_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInputs())
            {
                bool success = SaveComponents();
                if (success)
                {
                    ClearForm();
                    MessageBox.Show("Components added! You can now add more.");
                }
                // If not successful, let user correct inputs without clearing form
            }
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInputs())
            {
                bool success = SaveComponents();
                if (success)
                {
                    MessageBox.Show("All components saved successfully.");
                    this.Close();
                }
                // If not successful, keep the window open and let user fix inputs
            }
        }


        private bool ValidateInputs()
        {
            // Check if name and subtype are filled
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) || string.IsNullOrWhiteSpace(SubtypeTextBox.Text))
            {
                MessageBox.Show("Please enter both Name and Subtype.");
                return false;
            }

            // Check if quantity is valid and fields are generated
            if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Please enter a valid quantity and generate fields first.");
                return false;
            }

            // Check if all component ID fields are filled
            bool anyComponentIdFilled = false;
            foreach (var child in ComponentIdPanel.Children)
            {
                if (child is TextBox textBox)
                {
                    if (!string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        anyComponentIdFilled = true;
                    }
                    else
                    {
                        MessageBox.Show("Please fill in all Component IDs.");
                        return false;
                    }
                }
            }

            // Check if at least one component ID was filled (in case there are no textboxes)
            if (!anyComponentIdFilled)
            {
                MessageBox.Show("Please generate fields and enter at least one Component ID.");
                return false;
            }

            return true;
        }

        private bool SaveComponents()
        {
            foreach (var child in ComponentIdPanel.Children)
            {
                if (child is TextBox box)
                    box.ClearValue(TextBox.BackgroundProperty);
            }

            string name = NameTextBox.Text.Trim();
            string subtype = SubtypeTextBox.Text.Trim();

            string connectionString = "Server=DESKTOP-7C4SMS3\\SQLEXPRESS;Database=LabInventoryManagemnet;User Id=sa;Password=abc123;TrustServerCertificate=True;";
            List<TextBox> duplicateBoxes = new List<TextBox>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    SqlCommand enableIdentityInsert = new SqlCommand("SET IDENTITY_INSERT Components ON", conn);
                    enableIdentityInsert.ExecuteNonQuery();

                    foreach (var child in ComponentIdPanel.Children)
                    {
                        if (child is TextBox textBox)
                        {
                            string componentIdStr = textBox.Text.Trim();

                            if (!string.IsNullOrWhiteSpace(componentIdStr) && int.TryParse(componentIdStr, out int componentId))
                            {
                                try
                                {
                                    InsertComponent(conn, componentId, name, subtype);
                                }
                                catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                                {
                                    // Duplicate key
                                    duplicateBoxes.Add(textBox);
                                }
                            }
                        }
                    }

                    SqlCommand disableIdentityInsert = new SqlCommand("SET IDENTITY_INSERT Components OFF", conn);
                    disableIdentityInsert.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving components: " + ex.Message);
                    return false;
                }
            }

            if (duplicateBoxes.Count > 0)
            {
                MessageBox.Show($"Some Component IDs already exist. Please enter unique values and try again.", "Duplicate IDs", MessageBoxButton.OK, MessageBoxImage.Warning);

                // Highlight duplicate fields
                foreach (var box in duplicateBoxes)
                {
                    box.Background = System.Windows.Media.Brushes.MistyRose;
                }

                return false;
            }

            return true;
        }




        private void InsertComponent(SqlConnection conn, int componentId, string name, string subtype)
        {
            string query = @"
        INSERT INTO Components (Id, Name, Subtype, LabId, LabAttendantId, WorkingCondition, AvailabilityStatus)
        VALUES (@Id, @Name, @Subtype, @LabId, @LabAttendantId, 'Working', 'Available');";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", componentId);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Subtype", subtype);
                cmd.Parameters.AddWithValue("@LabId", labId);
                cmd.Parameters.AddWithValue("@LabAttendantId", labAttendantId);

                cmd.ExecuteNonQuery();
            }
        }

        private void ClearForm()
        {
            NameTextBox.Clear();
            SubtypeTextBox.Clear();
            QuantityTextBox.Clear();
            ComponentIdPanel.Children.Clear();
        }
    }
}