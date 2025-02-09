﻿using Microsoft.EntityFrameworkCore;
using ParkMate2._0.Models;
using Spectre.Console;

namespace ParkMate2._0.Helpers
{
    public static class ParkingHelper
    {
        public static void StartParking(User user)
        {
            Console.Clear();
            using (var db = new ParkMate20Context())
            {
                var cars = db.Cars.Where(c => c.UserId == user.UserId).ToList();

                if (!cars.Any()) //If the user doesnt have any registered cars
                {
                    AnsiConsole.MarkupLine("[red]You have no registered car! Register a car first.[/]");
                    Console.ReadKey();
                    return;
                }

                // show parking spot in a table
                AnsiConsole.MarkupLine("[yellow]Available Parking Spots in Gothenburg:[/]");
                ShowParkingSpots();

                // let user choose parking spot
                int selectedIndex;
                while (true) // Loop until a valid choice is made
                {
                    try
                    {
                        selectedIndex = AnsiConsole.Ask<int>("[yellow]Enter the number of your chosen parking spot:[/]") - 1;

                        if (selectedIndex >= 0 && selectedIndex < ParkingSpotData.GetAllSpots().Count)
                        {
                            break; // brake the loop if a choice is valid
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[red]Invalid choice! Please select a valid number.[/]");
                        }
                    }
                    catch (Exception)
                    {
                        AnsiConsole.MarkupLine("[red]Invalid input! Please enter a number.[/]");
                    }
                }
                Console.Clear();

                // use proper index for getting the correct parking spot
                var parkingSpot = ParkingSpotData.GetAllSpots()[selectedIndex];
                AnsiConsole.MarkupLine($"[green]You have selected {parkingSpot.Name} at {parkingSpot.PricePerHour} SEK/hour.[/]");


                // select car to park
                var carChoices = cars.Select(c => $"{c.Model} ({c.LicensePlate})").ToList();
                var selectedCar = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Select a car to start parking:[/]")
                        .AddChoices(carChoices));

                var car = cars.FirstOrDefault(c => $"{c.Model} ({c.LicensePlate})" == selectedCar);

                // choose paymethod
                var paymentMethods = new List<string> { "Swish", "Credit Card", "Invoice" };
                var selectedPaymentMethod = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Select a payment method:[/]")
                        .AddChoices(paymentMethods));

                // Start parking and save in DB
                var newParking = new Parking
                {
                    CarId = car.CarId,
                    Timestamp = DateTime.Now,
                    Duration = 0,
                    PayMethod = selectedPaymentMethod
                };

                db.Parkings.Add(newParking);
                db.SaveChanges();

                AnsiConsole.MarkupLine($"[green]Parking started at {parkingSpot.Name} for {car.Model} ({car.LicensePlate})![/]");
                AnsiConsole.MarkupLine($"[yellow]Rate:[/] {parkingSpot.PricePerHour} SEK/hour | [yellow]Payment Method:[/] {selectedPaymentMethod}");
                Console.ReadKey();
            }
        }
        public static void ShowParkingSpots()  // table for parkingspot
        {
            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.AddColumn("[yellow]#[/]");
            table.AddColumn("[yellow]Location[/]");
            table.AddColumn("[yellow]Price (SEK/hour)[/]");

            var spots = ParkingSpotData.GetAllSpots();

            for (int i = 0; i < spots.Count; i++)
            {
                var spot = spots[i];
                table.AddRow($"[blue]{i + 1}[/]", $"[green]{spot.Name}[/]", $"[red]{spot.PricePerHour}[/]");
            }

            AnsiConsole.Write(table);
        }


        public static void EndParking(User user)
        {
            Console.Clear();
            AnsiConsole.MarkupLine("[blue]Ending parking session...press enter for summary[/]");
            Console.ReadKey();

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

                var parkingSpot = ParkingSpotData.GetAllSpots().FirstOrDefault(p => activeParking.PayMethod != null);
                if (parkingSpot == null)
                {
                    AnsiConsole.MarkupLine("[red]Error retrieving parking spot details![/]");
                    Console.ReadKey();
                    return;
                }

                // calculate time and cost
                DateTime startTime = activeParking.Timestamp ?? DateTime.Now;
                DateTime endTime = DateTime.Now;
                TimeSpan duration = endTime - startTime;
                decimal totalHours = (decimal)duration.TotalHours;
                decimal totalCost = totalHours * parkingSpot.PricePerHour;

                activeParking.Duration = totalHours;
                db.SaveChanges();

                // summary
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

        public static void ShowParkingHistory(User user)
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
                    var table = new Table();
                    table.AddColumn("[yellow]Car[/]");
                    table.AddColumn("[yellow]License Plate[/]");
                    table.AddColumn("[blue]Start Time[/]");
                    table.AddColumn("[blue]End Time[/]");
                    table.AddColumn("[yellow]Duration (hours)[/]");
                    table.AddColumn("[red]Total Cost (SEK)[/]");

                    foreach (var parking in userParkings)
                    {
                        var car = db.Cars.FirstOrDefault(c => c.CarId == parking.CarId);
                        if (car == null) continue;

                        DateTime startTime = parking.Timestamp ?? DateTime.Now;
                        DateTime endTime = startTime.AddHours((double)parking.Duration);
                        decimal totalCost = parking.Duration * 10m;

                        table.AddRow(
                            $"[green]{car.Model}[/]",
                            $"[green]{car.LicensePlate}[/]",
                            $"[blue]{startTime}[/]",
                            $"[blue]{endTime}[/]",
                            $"[yellow]{parking.Duration:F2}[/]",
                            $"[red]{totalCost:C}[/]"
                        );
                    }

                    AnsiConsole.Write(table);
                }
            }

            Console.ReadKey();
        }
        public static void ShowAllParkings()
        {
            using (var db = new ParkMate20Context())
            {
                var parkings = db.Parkings.Include(p => p.Car).ToList();

                if (!parkings.Any())
                {
                    AnsiConsole.MarkupLine("[red]No parking records found![/]");
                    Console.ReadKey();
                    return;
                }

                var table = new Table();
                table.AddColumn("[yellow]Parking ID[/]");
                table.AddColumn("[yellow]Car Model[/]");
                table.AddColumn("[yellow]License Plate[/]");
                table.AddColumn("[yellow]Start Time[/]");
                table.AddColumn("[yellow]Duration (Hours)[/]");

                foreach (var parking in parkings)
                {
                    table.AddRow(
                        parking.ParkingId.ToString(),
                        $"[green]{parking.Car.Model}[/]",
                        $"[blue]{parking.Car.LicensePlate}[/]",
                        (parking.Timestamp ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm"),
                        (parking.Duration != null ? parking.Duration : 0m).ToString("F2")
                    );
                }

                AnsiConsole.Write(table);
                Console.ReadKey();
            }
        }
    }
}
