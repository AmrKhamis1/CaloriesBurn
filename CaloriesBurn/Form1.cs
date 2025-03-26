using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using System.Data.SQLite;

namespace CaloriesBurn
{
    public partial class Form1 : Form
    {
        private Timer updateTimer;
        private string connectionString = "Data Source=DailyCaluries.db;Version=3;";

        public Form1()
        {
            InitializeComponent();
            updateTimer = new Timer();
            updateTimer.Interval = 2000; // Update every 2 seconds
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS ActivityLog (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT,
                    Activity TEXT,
                    Duration TEXT,
                    CaloriesBurned TEXT
                )";
                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        private Dictionary<string, double> metValues = new Dictionary<string, double>
        {
            { "Running", 9.8 },
            { "Walking", 3.3 },
            { "Swimming", 8.0 },
            { "Cycling", 7.5 },
            { "Gym", 6.0 }
        };

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string activity = lstActivity.SelectedItem?.ToString();
            double duration = double.TryParse(txtDuration.Text, out double dur) ? dur : 0;
            double activityCalories = GetDailyActivityCalories();
            // Retrieve optional inputs (weight, height, age)
            double weight = double.TryParse(txtWeight.Text, out double w) ? w : 40; // Default: 70 kg
            double height = double.TryParse(txtHeight.Text, out double h) ? h : 150; // Default: 170 cm
            int age = int.TryParse(txtAge.Text, out int a) ? a : 12; // Default: 25 years
            double bmr = 88.362 + (13.397 * weight) + (4.799 * height) - (5.677 * age);
            double totalDailyBurn = bmr + activityCalories;
            // Validate inputs
            if (string.IsNullOrEmpty(activity) || duration <= 0)
            {
                MessageBox.Show("Please select an activity and enter a valid duration.");
                return;
            }

            // Retrieve MET value
            if (!metValues.ContainsKey(activity))
            {
                MessageBox.Show("Activity not recognized.");
                return;
            }

            double metValue = metValues[activity];
            double durationHours = duration / 60.0; // Convert minutes to hours

            // Base calorie calculation
            double caloriesBurned = metValue * weight * durationHours;

            // Adjust based on optional inputs if all are provided
            if (!string.IsNullOrEmpty(txtWeight.Text) && !string.IsNullOrEmpty(txtHeight.Text) && !string.IsNullOrEmpty(txtAge.Text))
            {
                // Additional adjustment based on user details (example adjustment logic)
                double bmrMultiplier = (10 * weight) + (6.25 * height) - (5 * age); // Simplified BMR factor
                caloriesBurned += bmrMultiplier * 0.01; // Adjusted calorie burn
            }

            // Save to database
            string date = dtpDate.Value.ToString("yyyy-MM-dd");

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string insertQuery = @"
        INSERT INTO ActivityLog (Date, Activity, Duration, CaloriesBurned)
        VALUES (@Date, @Activity, @Duration, @CaloriesBurned)";
                using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Date", date);
                    command.Parameters.AddWithValue("@Activity", activity);
                    command.Parameters.AddWithValue("@Duration", duration);
                    command.Parameters.AddWithValue("@CaloriesBurned", caloriesBurned);
                    command.ExecuteNonQuery();
                }
            }

            MessageBox.Show(
                $"Activity added successfully!\n\n" +
                $"Calories burned from activities today: {caloriesBurned:F2} kcal\n" +
                $"Estimated daily calorie burn (including BMR): {totalDailyBurn:F2} kcal\n" +
                $"Note: Average daily calorie requirement is 2000–2500 kcal for grown man.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            ); ClearInputs();
            UpdateChart();
        }
        private void ClearInputs()
        {
            lstActivity.Text = string.Empty;
            txtDuration.Text = string.Empty;
            dtpDate.Value = DateTime.Now;
        }
        private double GetDailyActivityCalories()
        {
            double totalCalories = 0;
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"
        SELECT SUM(CaloriesBurned) AS TotalCalories
        FROM ActivityLog
        WHERE Date = @Date";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Date", currentDate);
                    object result = command.ExecuteScalar();
                    totalCalories = result != DBNull.Value ? Convert.ToDouble(result) : 0;
                }
            }

            return totalCalories;
        }

        private void btnShow_Click(object sender, EventArgs e)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string selectQuery = "SELECT Date, Activity, Duration, CaloriesBurned FROM ActivityLog";
                using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                {
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dgvActivities.DataSource = dataTable;
                    }
                }
            }
        }
        private DataTable FetchCalorieData()
        {
            DataTable dataTable = new DataTable();
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"
        SELECT Date, SUM(CaloriesBurned) AS TotalCalories
        FROM ActivityLog
        GROUP BY Date
        ORDER BY Date";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            return dataTable;
        }
        private void UpdateChart()
        {
            DataTable calorieData = FetchCalorieData();

            // Clear existing data in the chart
            chartCalories.Series.Clear();

            // Add a new series to the chart
            var series = new System.Windows.Forms.DataVisualization.Charting.Series("Calories Burned");
            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chartCalories.Series.Add(series);

            // Add data points to the series
            foreach (DataRow row in calorieData.Rows)
            {
                string date = Convert.ToDateTime(row["Date"]).ToString("yyyy-MM-dd");
                double totalCalories = Convert.ToDouble(row["TotalCalories"]);
                series.Points.AddXY(date, totalCalories);
            }

            // Set chart title and axis labels
            chartCalories.Titles.Clear();
            chartCalories.Titles.Add("Calories Burned Over Time");
            chartCalories.ChartAreas[0].AxisX.Title = "Date";
            chartCalories.ChartAreas[0].AxisY.Title = "Calories Burned";
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateChart();

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void txtDuration_TextChanged(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            // Fetch the data from the database
            DataTable calorieData = FetchCalorieData();

            // Create a new Excel workbook
            using (XLWorkbook workbook = new XLWorkbook())
            {
                // Add a worksheet for the data
                var worksheet = workbook.Worksheets.Add("Calories Data");

                // Add headers to the first row
                worksheet.Cell(1, 1).Value = "Date";
                worksheet.Cell(1, 2).Value = "Total Calories Burned";

                // Add data to the worksheet
                int row = 2;
                foreach (DataRow dataRow in calorieData.Rows)
                {
                    worksheet.Cell(row, 1).Value = Convert.ToDateTime(dataRow["Date"]).ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 2).Value = Convert.ToDouble(dataRow["TotalCalories"]);
                    row++;
                }

                // Save the Excel file
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Excel Files|*.xlsx";
                saveFileDialog.DefaultExt = "xlsx";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    workbook.SaveAs(saveFileDialog.FileName);
                    MessageBox.Show("Data exported successfully to Excel.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
