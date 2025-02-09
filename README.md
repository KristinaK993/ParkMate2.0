ParkMate 2.0 🚗
ParkMate 2.0 is a console-based parking management application built with C# and .NET Core. It allows users to manage parking sessions, register vehicles, and offers a comprehensive admin interface for managing users and parking activity.

Features 🌟

User Features:
User Registration: Create an account and register your vehicle instantly.
Vehicle Management: Add, remove, and view registered vehicles.
Start and End Parking: Track your parking duration and calculate costs based on the selected parking location.
Parking History: View your past parking sessions, including duration and total cost.

Admin Features:
View All Users: List all registered users.
View All Parking Sessions: Access a log of all parking activities.
Delete Users: Remove users and their associated data.
Promote Users: Grant admin privileges to standard users.

Technology Stack 🛠
C# .NET 8.0
Entity Framework Core for database access
Microsoft SQL Server
Spectre.Console for enhanced console UI
Stored Procedures for efficient database operations

How to Use 📖:

User Login & Registration
New users can register with their username, email, and vehicle information.
Existing users can log in with their email and password.
Admins can log in to access additional admin functionalities.

Admin Menu
Manage users and parking sessions from the admin panel.
Options include viewing all users, promoting users to admin, and deleting user accounts.

Database Structure 📊:

The application database consists of these tables:
Users: Stores user details and roles (admin or regular user).
Cars: Manages registered vehicles.
Parkings: Logs parking sessions, including duration and total cost.
