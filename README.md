# Calories Burn Tracker

## Overview
Calories Burn Tracker is a Windows Forms application designed to help users log and track their daily activities and the calories they burn. It uses SQLite for data storage and provides a graphical representation of calorie consumption over time.

## Features
- **Activity Logging:** Users can log various activities (e.g., running, walking, cycling) along with their duration.
- **Calorie Calculation:** Automatically calculates calories burned based on MET values and user details like weight, height, and age.
- **Database Storage:** Stores activity records in an SQLite database.
- **Data Visualization:** Displays a chart showing calorie burn trends over time.
- **Data Export:** Exports calorie data to an Excel file using the ClosedXML library.
- **Real-Time Updates:** Uses a timer to refresh data at regular intervals.

## Technologies Used
- **C#** (Windows Forms Application)
- **SQLite** (Database storage)
- **ClosedXML** (Excel file export)
- **Windows Forms Charting** (Graph visualization)

## Installation & Setup
### Prerequisites
- **Windows OS**
- **.NET Framework** (Ensure your system has the required .NET framework installed)
- **SQLite** (No external installation required as SQLite is embedded)

### Steps
1. Clone or download the repository.
2. Open the project in Visual Studio.
3. Build the solution to restore dependencies.
4. Run the application.

## Usage
1. Select an activity from the provided list.
2. Enter the duration in minutes.
3. (Optional) Enter weight, height, and age for a more accurate calorie estimation.
4. Click "Add" to log the activity.
5. Click "Show" to view all recorded activities.
6. Click "Export" to save data as an Excel file.
7. The calorie burn trend is displayed in the chart section.

## Database Structure
The application uses an SQLite database (`DailyCalories.db`) with a single table:
```
CREATE TABLE ActivityLog (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Date TEXT,
    Activity TEXT,
    Duration TEXT,
    CaloriesBurned TEXT
);
```

## Future Enhancements
- **User Authentication:** Allow users to create accounts and save their progress.
- **More Activities:** Expand the activity list with additional MET values.
- **Advanced Analytics:** Provide more detailed reports and insights.
- **Mobile Version:** Develop a cross-platform version using .NET MAUI or Xamarin.

## Contributing
Feel free to fork this repository and contribute by submitting pull requests. Any improvements and bug fixes are welcome!

## License
This project is licensed under the MIT License.

## Author
Amr Khamis

