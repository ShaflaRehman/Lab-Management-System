using System;
using System.Data;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace LabManagement
{
    public partial class ComponentIssuanceWindow : Window
    {
        private string connectionString = "Server=DESKTOP-7C4SMS3\\SQLEXPRESS;Database=LabInventoryManagemnet;User Id=sa;Password=abc123;TrustServerCertificate=True;";
        private int selectedComponentId;

        public ComponentIssuanceWindow()
        {
            InitializeComponent();
        }

        private void SearchComponents_Click(object sender, RoutedEventArgs e)
        {
            string componentName = ComponentNameBox.Text.Trim();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Id FROM Components WHERE Name = @name AND AvailabilityStatus = 'Available'", conn);
                cmd.Parameters.AddWithValue("@name", componentName);

                SqlDataReader reader = cmd.ExecuteReader();
                ComponentList.Items.Clear();

                while (reader.Read())
                {
                    ComponentList.Items.Add(reader["Id"].ToString());
                }

                if (ComponentList.Items.Count == 0)
                {
                    MessageBox.Show("No available components found with this name.");
                }
            }
        }

        private void IssueComponent_Click(object sender, RoutedEventArgs e)
        {
            if (ComponentList.SelectedItem == null)
            {
                MessageBox.Show("Please select a component to issue.");
                return;
            }

            selectedComponentId = int.Parse(ComponentList.SelectedItem.ToString());

            string studentName = StudentNameBox.Text.Trim();
            string regNo = RegNoBox.Text.Trim();
            string degree = DegreeBox.Text.Trim();
            string department = DepartmentBox.Text.Trim();
            string syndicate = SyndicateBox.Text.Trim();

            int userId;
            int studentId;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Step 1: Check if student (by Registration Number) exists
                SqlCommand checkStudentCmd = new SqlCommand("SELECT UserId FROM Students WHERE RegistrationNumber = @regNo", conn);
                checkStudentCmd.Parameters.AddWithValue("@regNo", regNo);
                object result = checkStudentCmd.ExecuteScalar();

                if (result == null)
                {
                    // Step 2: Insert into Users table
                    SqlCommand insertUserCmd = new SqlCommand("INSERT INTO Users (Username, Department, Role) OUTPUT INSERTED.Id VALUES (@username, @dept, 'Student')", conn);
                    insertUserCmd.Parameters.AddWithValue("@username", studentName);
                    insertUserCmd.Parameters.AddWithValue("@dept", department);
                    userId = (int)insertUserCmd.ExecuteScalar();

                    // Step 3: Insert into Students table
                    SqlCommand insertStudentCmd = new SqlCommand("INSERT INTO Students (UserId, RegistrationNumber, Degree, Syndicate) VALUES (@userId, @regNo, @degree, @syn)", conn);
                    insertStudentCmd.Parameters.AddWithValue("@userId", userId);
                    insertStudentCmd.Parameters.AddWithValue("@regNo", regNo);
                    insertStudentCmd.Parameters.AddWithValue("@degree", degree);
                    insertStudentCmd.Parameters.AddWithValue("@syn", syndicate);
                    insertStudentCmd.ExecuteNonQuery();

                    studentId = userId;
                }
                else
                {
                    studentId = (int)result;
                }

                // Step 4: Update Component Status
                SqlCommand updateComponentCmd = new SqlCommand("UPDATE Components SET AvailabilityStatus = 'Issued' WHERE Id = @id", conn);
                updateComponentCmd.Parameters.AddWithValue("@id", selectedComponentId);
                updateComponentCmd.ExecuteNonQuery();

                // Step 5: Insert Transaction
                SqlCommand insertTransactionCmd = new SqlCommand(@"INSERT INTO Transactions 
                    (ComponentId, StudentId, LabId, LabAttendantId, DateIssued, DateReturned, Purpose, Remarks)
                    VALUES (@compId, @studentId, @labId, @attendantId, GETDATE(), NULL, @purpose, @remarks)", conn);

                insertTransactionCmd.Parameters.AddWithValue("@compId", selectedComponentId);
                insertTransactionCmd.Parameters.AddWithValue("@studentId", studentId);
                insertTransactionCmd.Parameters.AddWithValue("@labId", SessionManager.LabId);
                insertTransactionCmd.Parameters.AddWithValue("@attendantId", SessionManager.LabAttendantId);
                insertTransactionCmd.Parameters.AddWithValue("@purpose", "General Issuance");
                insertTransactionCmd.Parameters.AddWithValue("@remarks", "Issued from dashboard");

                insertTransactionCmd.ExecuteNonQuery();

                MessageBox.Show("Component issued successfully!");
                this.Close();
            }
        }
    }
}
