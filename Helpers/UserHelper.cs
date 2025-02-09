using System;
using System.Linq;
using ParkMate2._0.Models;
using ParkMate2._0.Helpers;
using Spectre.Console;

public static class UserHelper
{
    public static User RegisterUser()
    {
        Console.Clear();
        AnsiConsole.MarkupLine("[green]Register a new account on ParkingMate:[/]");

        while (true)
        {
            string userName = AnsiConsole.Ask<string>("[yellow]Username:[/]").Trim();
            string email = AnsiConsole.Ask<string>("[yellow]Email:[/]").Trim();

            using (var db = new ParkMate20Context())
            {
                if (db.Users.Any(u => u.Email == email))
                {
                    AnsiConsole.MarkupLine("[red]This email is already registered, try another one![/]");
                    continue;
                }
            }

            string password = AnsiConsole.Prompt(
                new TextPrompt<string>("[yellow]Password:[/]").Secret()).Trim();

            User newUser;
            using (var db = new ParkMate20Context())
            {
                newUser = new User
                {
                    UserName = userName,
                    Email = email,
                    Password = PasswordHelper.HashPassword(password)
                };

                db.Users.Add(newUser);
                db.SaveChanges();
            }

            //Here the user must register a car after registration
            CarHelper.RegisterCar(newUser);

            AnsiConsole.MarkupLine($"[green]Welcome, {newUser.UserName}! You are now registered and a car is linked to your account.[/]");
            Console.ReadKey();

            return newUser; // Return the new user
        }
    }

    public static void Login() //Login
    {
        Console.Clear();
        AnsiConsole.MarkupLine("[blue]Log in to your ParkingMate:[/]");

        while (true)
        {
            string email = AnsiConsole.Ask<string>("[yellow]Email:[/]").Trim();
            string password = AnsiConsole.Prompt(
                new TextPrompt<string>("[yellow]Password:[/]").Secret()).Trim();

            using (var db = new ParkMate20Context())
            {
                var user = db.Users.FirstOrDefault(u => u.Email == email);

                if (user == null)
                {
                    AnsiConsole.MarkupLine("[red]No account found with this email, try again![/]");
                    continue;
                }

                if (!PasswordHelper.VerifyPassword(password, user.Password))
                {
                    AnsiConsole.MarkupLine("[red]Incorrect password, try again![/]");
                    continue;
                }

                // Welcome message
                AnsiConsole.MarkupLine($"[green]Welcome back, {user.UserName}![/]");
                Console.ReadKey();

                // Check if user is admin
                if (user.IsAdmin)
                {
                    AdminMenu(user);  // Send admin to AdminMenu
                }
                else
                {
                    UserMenu(user);  // Send regular user to UserMenu
                }

                return;  // Exit the loop after successful login
            }
        }
    }

    public static void UserMenu(User loggedInUser)
    {
        while (true)
        {
            Console.Clear();
            AnsiConsole.MarkupLine($"[green]Welcome, {loggedInUser.UserName}![/]");
            AnsiConsole.MarkupLine("[blue]Select an option:[/]");

            var options = new List<string>
        {
            "Start Parking",
            "End Parking",
            "Parking History",
            "Manage Cars",
            "Log Out"
        };

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]What do you want to do?[/]")
                    .AddChoices(options));

            switch (choice)
            {
                case "Start Parking":
                    ParkingHelper.StartParking(loggedInUser);
                    break;
                case "End Parking":
                    ParkingHelper.EndParking(loggedInUser);
                    break;
                case "Parking History":
                    ParkingHelper.ShowParkingHistory(loggedInUser);
                    break;
                case "Manage Cars":
                    CarHelper.ManageCars(loggedInUser);
                    break;
                case "Log Out":
                    return;
            }
        }
    }

    public static void AdminMenu(User adminUser)
    {
        while (true)
        {
            Console.Clear();
            AnsiConsole.MarkupLine($"[red]Admin Panel - Welcome {adminUser.UserName}[/]");
            AnsiConsole.MarkupLine("[blue]Select an admin action:[/]");

            var options = new List<string>
        {
            "View All Users",
            "View All Parkings",
            "Delete a User",
            "Promote User to Admin",
            "Back to Main Menu"
        };

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]What do you want to do?[/]")
                    .AddChoices(options));

            switch (choice)
            {
                case "View All Users":
                    ViewAllUsers();
                    break;
                case "View All Parkings":
                    ParkingHelper.ShowAllParkings();
                    break;
                case "Delete a User":
                    DeleteUser();
                    break;
                case "Promote User to Admin": 
                    PromoteUserToAdmin(adminUser);
                    break;
                case "Back to Main Menu":
                    return;
            }
        }
    }
    public static void PromoteUserToAdmin(User adminUser)
    {
        using (var db = new ParkMate20Context())
        {
            // Fetch all non-admin users from the database
            var users = db.Users.Where(u => !u.IsAdmin).ToList();

            if (!users.Any())
            {
                // Display a message if no users are available for promotion
                AnsiConsole.MarkupLine("[red]No regular users available for promotion.[/]");
                Console.ReadKey();
                return;
            }

            // Display a table with all regular users
            AnsiConsole.MarkupLine("[yellow]Select a user to promote to admin:[/]");
            var table = new Table();
            table.AddColumn("User ID");
            table.AddColumn("Username");
            table.AddColumn("Email");

            foreach (var user in users)
            {
                table.AddRow(user.UserId.ToString(), user.UserName, user.Email);
            }
            AnsiConsole.Write(table);

            // Add a separate "Go Back" option, not part of the main list
            AnsiConsole.MarkupLine("[blue]Type [green]0[/] to go back to the admin menu.[/]");

            int userId;
            while (true)
            {
                // Ask for a User ID to promote
                userId = AnsiConsole.Ask<int>("[yellow]Enter the User ID to promote:[/]");

                if (userId == 0)
                {
                    // Return to the admin menu if the user enters 0
                    return;
                }

                // Find the user with the given User ID
                var userToPromote = db.Users.FirstOrDefault(u => u.UserId == userId);

                if (userToPromote != null)
                {
                    // Promote the selected user to admin
                    userToPromote.IsAdmin = true;
                    db.SaveChanges();
                    AnsiConsole.MarkupLine($"[green]User {userToPromote.UserName} has been promoted to admin.[/]");
                    break;
                }
                else
                {
                    // If the User ID is invalid, ask again
                    AnsiConsole.MarkupLine("[red]Invalid User ID. Please try again.[/]");
                }
            }
            Console.ReadKey();
        }
    }


    public static void ViewAllUsers()
    {
        using (var db = new ParkMate20Context()) //Db connection
        {
            var users = db.Users.ToList(); //get all users

            if (!users.Any())
            {
                AnsiConsole.MarkupLine("[red]No users found in the system![/]");
                Console.ReadKey();
                return;
            }

            var table = new Table();
            table.AddColumn("[yellow]User ID[/]");
            table.AddColumn("[yellow]Username[/]");
            table.AddColumn("[yellow]Email[/]");
            table.AddColumn("[yellow]Is Admin[/]");

            foreach (var user in users)
            {
                table.AddRow(
                    user.UserId.ToString(),
                    $"[green]{user.UserName}[/]",
                    $"[blue]{user.Email}[/]",
                    user.IsAdmin ? "[red]Yes[/]" : "[green]No[/]"
                );
            }

            AnsiConsole.Write(table);
            Console.ReadKey();
        }
    }
    public static void DeleteUser()
    {
        using (var db = new ParkMate20Context())
        {
            var users = db.Users.ToList();

            if (!users.Any())
            {
                AnsiConsole.MarkupLine("[red]No users found![/]");
                Console.ReadKey();
                return;
            }

            // print userList
            AnsiConsole.MarkupLine("[yellow]Select a user to delete:[/]");
            var table = new Table();
            table.AddColumn("User ID");
            table.AddColumn("Username");
            table.AddColumn("Email");

            foreach (var user in users)
            {
                table.AddRow(user.UserId.ToString(), user.UserName, user.Email);
            }
            AnsiConsole.Write(table);

            // Go back alternative
            AnsiConsole.MarkupLine("[blue]Type [yellow]0[/] to go back to the admin menu.[/]");

            int userId;
            while (true)
            {
                userId = AnsiConsole.Ask<int>("[yellow]Enter the User ID to delete:[/]");

                if (userId == 0)
                {
                    return;  // Go back to AdminMenu
                }

                var userToDelete = db.Users.FirstOrDefault(u => u.UserId == userId);
                if (userToDelete != null)
                {
                    db.Users.Remove(userToDelete);
                    db.SaveChanges();
                    AnsiConsole.MarkupLine($"[green]User {userToDelete.UserName} has been deleted.[/]");
                    break;
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Invalid User ID. Please try again.[/]");
                }
            }

            Console.ReadKey();
        }
    }



}
