using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace LabManagement
{
    public partial class ComponentByLabWindow : Window
    {
        private string connectionString = "Server=DESKTOP-7C4SMS3\\SQLEXPRESS;Database=LabInventoryManagemnet;User Id=sa;Password=abc123;TrustServerCertificate=True;";
        private Dictionary<string, string> courseNameToCode = new Dictionary<string, string>();

        public ComponentByLabWindow()
        {
            InitializeComponent();
            LoadCourses();
        }

        private void LoadCourses()
        {
            courseNameToCode.Clear();
            List<string> courses = new List<string>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT DISTINCT CourseName, CourseCode FROM Courses WHERE CourseName IS NOT NULL";

                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        string code = reader.GetString(1);
                        courseNameToCode[name] = code;
                        courses.Add(name);
                    }

                    CourseComboBox.ItemsSource = courses;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading courses: {ex.Message}");
                }
            }
        }

        private void CourseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LabNumberComboBox.ItemsSource = null;

            if (CourseComboBox.SelectedItem is string selectedCourse && courseNameToCode.ContainsKey(selectedCourse))
            {
                string selectedCourseCode = courseNameToCode[selectedCourse];
                LoadLabNumbersForCourse(selectedCourseCode);
            }
        }

        private void LoadLabNumbersForCourse(string courseCode)
        {
            List<string> labNumbers = new List<string>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT DISTINCT LabNumber FROM LabManuals WHERE CourseCode = @courseCode AND LabNumber IS NOT NULL";

                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@courseCode", courseCode);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        labNumbers.Add(reader[0].ToString());
                    }

                    LabNumberComboBox.ItemsSource = labNumbers;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading lab numbers: {ex.Message}");
                }
            }
        }

        private void FetchComponents_Click(object sender, RoutedEventArgs e)
        {
            string selectedCourse = CourseComboBox.SelectedItem as string;
            string selectedLab = LabNumberComboBox.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(selectedCourse) || string.IsNullOrWhiteSpace(selectedLab))
            {
                MessageBox.Show("Please select both course and lab number.");
                return;
            }

            List<ComponentDisplay> components = new List<ComponentDisplay>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Step 1: Get CourseCode
                string courseCode = courseNameToCode.ContainsKey(selectedCourse) ? courseNameToCode[selectedCourse] : null;
                if (string.IsNullOrEmpty(courseCode))
                {
                    MessageBox.Show("Course code not found.");
                    return;
                }

                int labManualId = 0;
                string queryGetLabManualId = @"
            SELECT LabManualId 
            FROM LabManuals 
            WHERE CourseCode = @courseCode AND LabNumber = @labNumber";

                try
                {
                    conn.Open();

                    // Step 2: Get LabManualId
                    using (SqlCommand cmd = new SqlCommand(queryGetLabManualId, conn))
                    {
                        cmd.Parameters.AddWithValue("@courseCode", courseCode);
                        cmd.Parameters.AddWithValue("@labNumber", selectedLab);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            labManualId = Convert.ToInt32(result);
                        }
                        else
                        {
                            MessageBox.Show("Lab manual not found for the selected course and lab.");
                            return;
                        }
                    }

                    // Step 3: Read component data into a temp list
                    List<(string Name, string Subtype, int Required)> tempComponents = new List<(string, string, int)>();

                    string queryFetchComponents = @"
                SELECT ComponentName, Subtype, Quantity 
                FROM LabManual_RequiredComponents 
                WHERE LabManualId = @labManualId";

                    using (SqlCommand cmd = new SqlCommand(queryFetchComponents, conn))
                    {
                        cmd.Parameters.AddWithValue("@labManualId", labManualId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string name = reader.IsDBNull(0) ? "-" : reader.GetString(0);
                                string subtype = reader.IsDBNull(1) ? "-" : reader.GetString(1);
                                int quantity = reader.GetInt32(2);
                                tempComponents.Add((name, subtype, quantity));
                            }
                        } // reader is closed here
                    }

                    // Step 4: Now fetch available quantity for each component
                    foreach (var item in tempComponents)
                    {
                        int availableQuantity = 0;

                        string queryGetAvailableQuantity = @"
                    SELECT COUNT(*) 
                    FROM Components 
                    WHERE Name = @componentName 
                      AND Subtype = @componentSubtype 
                      AND AvailabilityStatus = 'Available'";

                        using (SqlCommand countCmd = new SqlCommand(queryGetAvailableQuantity, conn))
                        {
                            countCmd.Parameters.AddWithValue("@componentName", item.Name);
                            countCmd.Parameters.AddWithValue("@componentSubtype", item.Subtype);

                            availableQuantity = (int)countCmd.ExecuteScalar();
                        }

                        components.Add(new ComponentDisplay
                        {
                            Name = string.IsNullOrWhiteSpace(item.Name) ? "-" : item.Name,
                            Subtype = string.IsNullOrWhiteSpace(item.Subtype) ? "-" : item.Subtype,
                            RequiredQuantity = item.Required,
                            AvailableQuantity = availableQuantity
                        });
                    }

                    ComponentsDataGrid.ItemsSource = components;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error fetching components: {ex.Message}");
                }
            }
        }



        public class ComponentDisplay
        {
            public string Name { get; set; }
            public string Subtype { get; set; }
            public int RequiredQuantity { get; set; }
            public int AvailableQuantity { get; set; }
        }
    }
}
