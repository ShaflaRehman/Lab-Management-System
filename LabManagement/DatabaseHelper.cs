using System;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace LabManagement
{
    public static class DatabaseHelper
    {
        public static string connectionString = "Server=DESKTOP-7C4SMS3\\SQLEXPRESS;Database=LabInventoryManagemnet;User Id=sa;Password=abc123;TrustServerCertificate=True;";

        // Method to hash the password using SHA-256 (you can replace it with a more secure algorithm like bcrypt if needed)
        private static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Method to validate the user during login
        public static bool ValidateUser(string username, string password, string role)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT Password FROM Users WHERE (Username = @username OR Email = @username) AND Role = @role";
                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@role", role);

                con.Open();
                var storedPassword = cmd.ExecuteScalar()?.ToString();

                if (storedPassword != null)
                {
                    // Hash the input password and compare with the stored password
                    string hashedInputPassword = HashPassword(password);
                    return storedPassword == hashedInputPassword;
                }
                return false;
            }
        }

        // Method to register a new user
        public static bool RegisterUser(string username, string email, string password, string role, string phoneNumber, string department)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Check if user/email already exists
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @username OR Email = @email";
                SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@username", username);
                checkCmd.Parameters.AddWithValue("@email", email);

                con.Open();
                int count = (int)checkCmd.ExecuteScalar();
                if (count > 0)
                    return false;

                // Hash the password before storing
                string hashedPassword = HashPassword(password);

                // Insert new user
                string insertQuery = "INSERT INTO Users (Username, Email, Password, Role, PhoneNumber, Department) VALUES (@username, @email, @password, @role, @phonenum, @dept)";
                SqlCommand insertCmd = new SqlCommand(insertQuery, con);
                insertCmd.Parameters.AddWithValue("@username", username);
                insertCmd.Parameters.AddWithValue("@email", email);
                insertCmd.Parameters.AddWithValue("@password", hashedPassword);
                insertCmd.Parameters.AddWithValue("@role", role);
                insertCmd.Parameters.AddWithValue("@phonenum", phoneNumber);
                insertCmd.Parameters.AddWithValue("@dept", department);

                insertCmd.ExecuteNonQuery();
                return true;
            }
        }
        public static bool RegisterUser(string username, string email, string password, string role, string phoneNumber, string department, int labId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // 1. Check if user/email already exists
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @username OR Email = @email";
                SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@username", username);
                checkCmd.Parameters.AddWithValue("@email", email);

                con.Open();
                int count = (int)checkCmd.ExecuteScalar();
                if (count > 0)
                    return false;

                // 2. Hash password
                string hashedPassword = HashPassword(password);

                // 3. Insert user into Users table (without LabId)
                string insertUserQuery = "INSERT INTO Users (Username, Email, Password, Role, PhoneNumber, Department) " +
                                         "OUTPUT INSERTED.Id VALUES (@username, @email, @password, @role, @phonenum, @dept)";
                SqlCommand insertUserCmd = new SqlCommand(insertUserQuery, con);
                insertUserCmd.Parameters.AddWithValue("@username", username);
                insertUserCmd.Parameters.AddWithValue("@email", email);
                insertUserCmd.Parameters.AddWithValue("@password", hashedPassword);
                insertUserCmd.Parameters.AddWithValue("@role", role);
                insertUserCmd.Parameters.AddWithValue("@phonenum", phoneNumber);
                insertUserCmd.Parameters.AddWithValue("@dept", department);

                // Get newly inserted UserId
                int userId = (int)insertUserCmd.ExecuteScalar();
               

                // 4. Insert into LabAttendants table
                string insertAttendantQuery = "INSERT INTO LabAttendants (UserId, LabId) VALUES (@userId, @labId)";
                SqlCommand insertAttendantCmd = new SqlCommand(insertAttendantQuery, con);
                insertAttendantCmd.Parameters.AddWithValue("@userId", userId);
                insertAttendantCmd.Parameters.AddWithValue("@labId", labId);

                // 5. Update lab_status in Labs table to 'Assigned'
                string updateLabStatusQuery = "UPDATE Labs SET lab_status = 'Assigned' WHERE LabId = @labId";
                SqlCommand updateLabCmd = new SqlCommand(updateLabStatusQuery, con);
                updateLabCmd.Parameters.AddWithValue("@labId", labId);
                updateLabCmd.ExecuteNonQuery();

                return true;
            
        }
        }
        public static bool RegisterStudent(int userId, string registrationNumber, string degree, string syndicate)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "INSERT INTO Students (UserId, RegistrationNumber, Degree, Syndicate) VALUES (@userId, @regNo, @degree, @syn)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@regNo", registrationNumber);
                cmd.Parameters.AddWithValue("@degree", degree);
                cmd.Parameters.AddWithValue("@syn", syndicate);

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }


        public static int GetLabIdByName(string labName)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT LabId FROM Labs WHERE LabName = @labName";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@labName", labName);
                con.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
        }

        public static int GetUserIdByEmail(string email)
        {
            string query = "SELECT Id FROM Users WHERE LOWER(Email) = @Email";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);

                try
                {
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
                catch
                {
                    return -1;
                }
            }
        }

        public static bool InsertStudentInfo(int userId, string regNo, string degree, string syndicate)
        {
            string query = "INSERT INTO Students (UserId, RegistrationNumber, Degree, Syndicate) VALUES (@UserId, @RegNo, @Degree, @Syndicate)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@RegNo", regNo);
                cmd.Parameters.AddWithValue("@Degree", degree);
                cmd.Parameters.AddWithValue("@Syndicate", syndicate);

                try
                {
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch
                {
                    return false;
                }
            }
        }


        public static bool IsLabAvailable(int labId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM LabAttendants WHERE LabId = @labId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@labId", labId);
                con.Open();
                int count = (int)cmd.ExecuteScalar();
                return count == 0; // Lab is available if no one is assigned
            }
        }

    }
}
