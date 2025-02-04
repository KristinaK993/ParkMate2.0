using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using ParkMate2._0.Models;
using ParkMate2._0.Helpers;

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
                    Register();
                    break;
                case "Exit":
                    return;
            }
        }
    }

    // 🔵 Logga in
    static void Login()
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

                // 🟢 Skicka användaren till användarmenyn
                AnsiConsole.MarkupLine($"[green]Welcome back, {user.UserName}![/]");
                Console.ReadKey();
                UserMenu(user);
                return;
            }
        }
    }

    // 🟢 Registrera användare
    static void Register()
    {
        Console.Clear();
        AnsiConsole.MarkupLine("[green]Register a new account on ParkingMate:[/]");

        User newUser;

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

            //Registrera en bil direkt efter att användaren skapats
            RegisterCar(newUser);

            // 🟢 Skicka användaren till menyn efter registrering
            Console.Clear();
            AnsiConsole.MarkupLine($"[green]Welcome, {newUser.UserName}! You are now logged in.[/]");
            Console.ReadKey();
            UserMenu(newUser);
            return;
        }
    }
    static void RegisterCar(User user)
    {
        Console.Clear();
        AnsiConsole.MarkupLine("[blue]Register a car to your account:[/]");

        string licensePlate;
        while (true)
        {
            licensePlate = AnsiConsole.Ask<string>("[yellow]Enter License Plate (e.g., ABC123):[/]").Trim().ToUpper();

            using (var db = new ParkMate20Context())
            {
                if (db.Cars.Any(c => c.LicensePlate == licensePlate))
                {
                    AnsiConsole.MarkupLine("[red]This license plate is already registered! Try again.[/]");
                    continue;
                }
            }
            break;
        }

        string model = AnsiConsole.Ask<string>("[yellow]Enter Car Model:[/]").Trim();

        using (var db = new ParkMate20Context())
        {
            var newCar = new Car
            {
                UserId = user.UserId,
                LicensePlate = licensePlate,
                Model = model
            };

            db.Cars.Add(newCar);
            db.SaveChanges();
        }

        AnsiConsole.MarkupLine($"[green]Car {model} ({licensePlate}) registered successfully![/]");
        Console.ReadKey();
    }


    //Användarmenyn
    static void UserMenu(User loggedInUser)
    {
        while (true)
        {
            Console.Clear();
            AnsiConsole.MarkupLine($"[green]Welcome, {loggedInUser.UserName}![/]");
            AnsiConsole.MarkupLine("[blue]Select an option:[/]");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]What do you want to do?[/]")
                    .AddChoices(new[] { "Start Parking", "End Parking", "Parking History", "Log Out" }));

            switch (choice)
            {
                case "Start Parking":
                    StartParking(loggedInUser);
                    break;
                case "End Parking":
                    EndParking(loggedInUser);
                    break;
                case "Parking History":
                    ShowParkingHistory(loggedInUser);
                    break;
                case "Log Out":
                    return; // ⏭️ Går tillbaka till huvudmenyn
            }
        }
    }

    //Starta parkering
    static void StartParking(User user)
    {
        Console.Clear();
        AnsiConsole.MarkupLine("[blue]Starting a new parking session...[/]");

        using (var db = new ParkMate20Context())
        {
            var car = db.Cars.FirstOrDefault(c => c.UserId == user.UserId);
            if (car == null)
            {
                AnsiConsole.MarkupLine("[red]No registered car found![/]");
                Console.ReadKey();
                return;
            }

            var newParking = new Parking
            {
                CarId = car.CarId,
                Timestamp = DateTime.Now,
                Duration = 0
            };

            db.Parkings.Add(newParking);
            db.SaveChanges();
        }

        AnsiConsole.MarkupLine("[green]Parking started![/]");
        Console.ReadKey();
    }

    //Avsluta parkering och visa sammanfattning
    static void EndParking(User user)
    {
        Console.Clear();
        AnsiConsole.MarkupLine("[blue]Ending parking session...[/]");

        using (var db = new ParkMate20Context())
        {
            var activeParking = db.Parkings
                .Where(p => p.Car.UserId == user.UserId)
                .OrderByDescending(p => p.Timestamp)
                .FirstOrDefault();

            if (activeParking == null)
            {
                AnsiConsole.MarkupLine("[red]No active parking session found![/]");
                Console.ReadKey();
                return;
            }

            var car = db.Cars.FirstOrDefault(c => c.CarId == activeParking.CarId);
            if (car == null)
            {
                AnsiConsole.MarkupLine("[red]Error retrieving car details![/]");
                Console.ReadKey();
                return;
            }

           
            DateTime startTime = activeParking.Timestamp ?? DateTime.Now;
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;
            decimal totalHours = (decimal)duration.TotalHours;
            decimal ratePerHour = 10m;
            decimal totalCost = totalHours * ratePerHour;

            activeParking.Duration = totalHours;
            db.SaveChanges();

            Console.Clear();
            AnsiConsole.MarkupLine("[green]Parking Summary:[/]");
            AnsiConsole.MarkupLine($"[yellow]Car:[/] {car.Model} ({car.LicensePlate})");
            AnsiConsole.MarkupLine($"[yellow]Start Time:[/] {activeParking.Timestamp}");
            AnsiConsole.MarkupLine($"[yellow]End Time:[/] {endTime}");
            AnsiConsole.MarkupLine($"[yellow]Total Duration:[/] {totalHours:F2} hours");
            AnsiConsole.MarkupLine($"[yellow]Total Cost:[/] {totalCost:C} SEK");

            Console.ReadKey();
        }
    }

    // 📜 Visa parkeringshistorik
    static void ShowParkingHistory(User user)
    {
        Console.Clear();
        AnsiConsole.MarkupLine("[blue]Your Parking History:[/]");

        using (var db = new ParkMate20Context())
        {
            var userParkings = db.Parkings
                .Where(p => p.Car.UserId == user.UserId)
                .OrderByDescending(p => p.Timestamp)
                .ToList();

            if (!userParkings.Any())
            {
                AnsiConsole.MarkupLine("[red]No parking history found![/]");
            }
            else
            {
                foreach (var parking in userParkings)
                {
                    AnsiConsole.MarkupLine($"[green]Date: {parking.Timestamp} - Duration: {parking.Duration} hours[/]");
                }
            }
        }

        Console.ReadKey();
    }
}
