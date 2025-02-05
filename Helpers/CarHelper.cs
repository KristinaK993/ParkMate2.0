using System.Linq;
using Microsoft.EntityFrameworkCore;
using ParkMate2._0.Models;
using Spectre.Console;

namespace ParkMate2._0.Helpers
{
    public static class CarHelper
    {
        public static void RegisterCar(User user)
        {
            Console.Clear();
            AnsiConsole.MarkupLine("[blue]Register a car to your account:[/]");

            string licensePlate;
            while (true)
            {
                licensePlate = AnsiConsole.Ask<string>("[yellow]Enter License Plate (ex. ABC123):[/]").Trim().ToUpper();

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
                db.Database.ExecuteSqlRaw(
                    "EXEC dbo.AddCar @UserId = {0}, @LicensePlate = {1}, @Model = {2}",
                    user.UserId, licensePlate, model);
            }

            AnsiConsole.MarkupLine($"[green]Car {model} ({licensePlate}) registered successfully![/]");
            Console.ReadKey();
        }

        public static void RemoveCar(User user)
        {
            Console.Clear();
            using (var db = new ParkMate20Context())
            {
                var cars = db.Cars.Where(c => c.UserId == user.UserId).ToList();

                if (!cars.Any())
                {
                    AnsiConsole.MarkupLine("[red]You have no registered cars to remove![/]");
                    Console.ReadKey();
                    return;
                }

                var carChoices = cars.Select(c => $"{c.Model} ({c.LicensePlate})").ToList();
                carChoices.Add("Cancel");

                var selectedCar = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Select a car to remove:[/]")
                        .AddChoices(carChoices));

                if (selectedCar == "Cancel") return;

                var carToRemove = cars.FirstOrDefault(c => $"{c.Model} ({c.LicensePlate})" == selectedCar);
                if (carToRemove != null)
                {
                    db.Cars.Remove(carToRemove);
                    db.SaveChanges();
                    AnsiConsole.MarkupLine($"[green]Car {carToRemove.Model} ({carToRemove.LicensePlate}) removed![/]");
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

                // Skapa en snygg tabell
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

                // Skriv ut tabellen
                AnsiConsole.Write(table);
            }

            Console.ReadKey();
        }

    }
}
