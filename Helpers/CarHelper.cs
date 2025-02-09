using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ParkMate2._0.Models;
using Spectre.Console;

namespace ParkMate2._0.Helpers
{
    public static class CarHelper
    {
        public static void ManageCars(User user)
        {
            Console.Clear();
            AnsiConsole.MarkupLine("[blue]Manage Your Cars[/]");

            var options = new List<string>
    {
        "Add Car",
        "Remove Car",
        "View Cars",
        "Back to Menu"
    };

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Select an option:[/]")
                    .AddChoices(options)
            );

            switch (choice)
            {
                case "Add Car":
                    RegisterCar(user);
                    break;
                case "Remove Car":
                    RemoveCar(user);
                    break;
                case "View Cars":
                    ViewCars(user);
                    break;
                case "Back to Menu":
                    return;
            }
        }

        public static void RegisterCar(User user)
        {
            Console.Clear();
            AnsiConsole.MarkupLine("[yellow]Register a new car:[/]");

            while (true)
            {
                // Ask the user to input a license plate
                string licensePlate = AnsiConsole.Ask<string>("[yellow]Enter License Plate (format: ABC123):[/]").Trim().ToUpper();

                // Validate the license plate format (3 letters + 3 digits)
                if (!Regex.IsMatch(licensePlate, @"^[A-Z]{3}\d{3}$"))
                {
                    AnsiConsole.MarkupLine("[red]Invalid license plate format! Please enter in the format ABC123 (3 letters followed by 3 digits).[/]");
                    continue;  // Ask again if the format is incorrect
                }

                // Check if the license plate is already registered
                using (var db = new ParkMate20Context())
                {
                    if (db.Cars.Any(c => c.LicensePlate == licensePlate))
                    {
                        AnsiConsole.MarkupLine("[red]This license plate is already registered! Try again.[/]");
                        continue;
                    }
                }

                // Ask the user for the car model
                string model = AnsiConsole.Ask<string>("[yellow]Enter Car Model:[/]").Trim();

                // Save the new car to the database
                using (var db = new ParkMate20Context())
                {
                    db.Database.ExecuteSqlRaw(
                        "EXEC dbo.AddCar @UserId = {0}, @LicensePlate = {1}, @Model = {2}",
                        user.UserId, licensePlate, model);

                    AnsiConsole.MarkupLine($"[green]Car {model} ({licensePlate}) registered successfully![/]");
                    Console.ReadKey();
                    break;  
                }
            }
        }


        public static void RemoveCar(User user)
        {
            Console.Clear();
            using (var db = new ParkMate20Context())
            {
                var cars = db.Cars.Where(c => c.UserId == user.UserId).ToList();

                if (!cars.Any()) //If there isnt any cars to remove
                {
                    AnsiConsole.MarkupLine("[red]You have no registered cars to remove![/]");
                    Console.ReadKey();
                    return;
                }

                var carChoices = cars.Select(c => $"{c.Model} ({c.LicensePlate})").ToList();
                carChoices.Add("Cancel");

                var selectedCar = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Select the car you want to delete:[/]")
                        .AddChoices(carChoices));

                if (selectedCar == "Cancel") return;

                var carToRemove = cars.FirstOrDefault(c => $"{c.Model} ({c.LicensePlate})" == selectedCar);
                if (carToRemove != null)
                {
                    db.Cars.Remove(carToRemove);
                    db.SaveChanges();
                    AnsiConsole.MarkupLine($"[red]Car {carToRemove.Model} ({carToRemove.LicensePlate}) is deleted![/]");
                }

                Console.ReadKey();
            }
        }

        public static void ViewCars(User user)
        {
            Console.Clear();
            using (var db = new ParkMate20Context())
            {
                var cars = db.Cars.Where(c => c.UserId == user.UserId).ToList();

                if (!cars.Any())
                {
                    AnsiConsole.MarkupLine("[red]You have no registered cars![/]");
                    Console.ReadKey();
                    return;
                }

                // a table to view cars
                var table = new Table();
                table.Border = TableBorder.Rounded;
                table.AddColumn("[yellow]Car Model[/]");
                table.AddColumn("[yellow]License Plate[/]");

                foreach (var car in cars)
                {
                    table.AddRow(
                        $"[green]{car.Model}[/]",
                        $"[blue]{car.LicensePlate}[/]"
                    );
                }

                // print out table
                AnsiConsole.Write(table);
            }

            Console.ReadKey();
        }

    }
}
