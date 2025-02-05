using ParkMate2._0.Helpers;
using ParkMate2._0.Models;
using Spectre.Console;

class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Clear();
            AnsiConsole.Write(
                new FigletText("Welcome to ParkingMate")
                    .Color(Color.Green)
                    .Centered());

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Select an option:[/]")
                    .AddChoices(new[] { "Login", "Register", "Exit" }));

            switch (choice)
            {
                case "Login":
                    Login();
                    break;
                case "Register":
                    var newUser = UserHelper.RegisterUser(); //call on UserHelper
                    UserMenu(newUser); // sending user to menu
                    break;
                case "Exit":
                    return;
            }
        }

        static void Login() //Login
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

                    //Send user to usermenu
                    AnsiConsole.MarkupLine($"[green]Welcome back, {user.UserName}![/]");
                    Console.ReadKey();
                    UserMenu(user);
                    return;
                }
            }
        }
        static void UserMenu(User loggedInUser)
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
                        CarMenu(loggedInUser);
                        break;
                    case "Log Out":
                        return;
                }
            }
        }
        static void CarMenu(User user) //manage cars
        {
            while (true)
            {
                Console.Clear();
                AnsiConsole.MarkupLine($"[green]Manage Your Cars, {user.UserName}![/]");

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Select an option:[/]")
                        .AddChoices(new[] { "Add Car", "Remove Car", "View Cars", "Back to Menu" }));

                switch (choice)
                {
                    case "Add Car":
                        CarHelper.RegisterCar(user);
                        break;
                    case "Remove Car":
                        CarHelper.RemoveCar(user);
                        break;
                    case "View Cars":
                        CarHelper.ViewCars(user);
                        break;
                    case "Back to Menu":
                        return;
                }
            }
        }
    }
}

