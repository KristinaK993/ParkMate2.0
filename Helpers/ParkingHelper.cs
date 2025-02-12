using Microsoft.EntityFrameworkCore;
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

                if (!cars.Any()) // If the user doesn't have any registered cars
                {
                    AnsiConsole.MarkupLine("[red]You have no registered car! Register a car first.[/]");
                    Console.ReadKey();
                    return;
                }

                // Show available parking spots in a table
                AnsiConsole.MarkupLine("[yellow]Available Parking Spots in Gothenburg:[/]");
                ShowParkingSpots();

                // Let user choose parking spot
                int selectedIndex;
                while (true)
                {
                    try
                    {
                        selectedIndex = AnsiConsole.Ask<int>("[yellow]Enter the number of your chosen parking spot:[/]") - 1;

                        if (selectedIndex >= 0 && selectedIndex < ParkingSpotData.GetAllSpots().Count)
                        {
                            break;
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

                // Use proper index to get the correct parking spot
                var parkingSpot = ParkingSpotData.GetAllSpots()[selectedIndex];
                AnsiConsole.MarkupLine($"[green]You have selected {parkingSpot.Name} at {parkingSpot.PricePerHour} SEK/hour.[/]");

                // Select a car to park
                var carChoices = cars.Select(c => $"{c.Model} ({c.LicensePlate})").ToList();
                var selectedCar = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Select a car to start parking:[/]")
                        .AddChoices(carChoices));

                var car = cars.FirstOrDefault(c => $"{c.Model} ({c.LicensePlate})" == selectedCar);

                // Check if the selected car is already in an active parking session
                var existingParking = db.Parkings
                    .Where(p => p.CarId == car.CarId && p.Duration == 0)
                    .FirstOrDefault();

                if (existingParking != null)
                {
                    AnsiConsole.MarkupLine("[red]This car is already in an active parking session! End the current session before starting a new one.[/]");
                    Console.ReadKey();
                    return;
                }

                // Choose payment method
                var paymentMethods = new List<string> { "Swish", "Credit Card", "Invoice" };
                var selectedPaymentMethod = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Select a payment method:[/]")
                        .AddChoices(paymentMethods));

                // Start parking and save it in the database
                var newParking = new Parking
                {
                    CarId = car.CarId,
                    Timestamp = DateTime.Now,
                    Duration = 0,
                    PayMethod = selectedPaymentMethod,
                    ParkingSpotName = parkingSpot.Name
                };

                db.Parkings.Add(newParking);
                db.SaveChanges();

                AnsiConsole.MarkupLine($"[green]Parking started at {parkingSpot.Name} for {car.Model} ({car.LicensePlate})![/]");
                AnsiConsole.MarkupLine($"[yellow]Rate:[/] {parkingSpot.PricePerHour} SEK/hour | [yellow]Payment Method:[/] {selectedPaymentMethod}");
                Console.ReadKey();
            }
        }

        public static bool HasActiveParking(User user)
        {
            using (var db = new ParkMate20Context())
            {
                return db.Parkings.Any(p => p.Car.UserId == user.UserId && p.Duration == 0);
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
                // Get active parking session
                var activeParking = db.Parkings
                    .Include(p => p.Car) // Include car details
                    .Where(p => p.Car.UserId == user.UserId && p.Duration == 0)
                    .OrderByDescending(p => p.Timestamp)
                    .FirstOrDefault();

                if (activeParking == null)
                {
                    AnsiConsole.MarkupLine("[red]No active parking session found![/]");
                    Console.ReadKey();
                    return;
                }

                // Double-check if ParkingSpotName is valid
                if (string.IsNullOrEmpty(activeParking.ParkingSpotName) || activeParking.ParkingSpotName == "Unknown")
                {
                    AnsiConsole.MarkupLine("[red]Parking spot details are missing. Please verify your session.[/]");
                    Console.ReadKey();
                    return;
                }

                // Retrieve the correct parking spot
                var parkingSpot = ParkingSpotData.GetAllSpots()
                    .FirstOrDefault(p => p.Name == activeParking.ParkingSpotName);

                if (parkingSpot == null)
                {
                    AnsiConsole.MarkupLine("[red]Parking spot details not found in the system![/]");
                    Console.ReadKey();
                    return;
                }

                // Calculate duration and cost
                DateTime startTime = activeParking.Timestamp ?? DateTime.Now;
                DateTime endTime = DateTime.Now;
                TimeSpan duration = endTime - startTime;
                decimal totalHours = Math.Max(0.01m, (decimal)duration.TotalHours); // Minimum 0.01 hours
                decimal totalCost = totalHours * parkingSpot.PricePerHour;

                // Update parking with duration and save changes
                activeParking.Duration = totalHours;
                db.SaveChanges();

                // Display parking summary
                Console.Clear();
                AnsiConsole.MarkupLine("[green]Parking Summary:[/]");
                AnsiConsole.MarkupLine($"[yellow]Car:[/] {activeParking.Car.Model} ({activeParking.Car.LicensePlate})");
                AnsiConsole.MarkupLine($"[yellow]Start Time:[/] {startTime:yyyy-MM-dd HH:mm}");
                AnsiConsole.MarkupLine($"[yellow]End Time:[/] {endTime:yyyy-MM-dd HH:mm}");
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
                .Where(p => p.Car.UserId == user.UserId && p.Duration > 0)  // Visa bara avslutade parkeringar
                .OrderByDescending(p => p.Timestamp)
                .ToList();


                if (userParkings == null || !userParkings.Any())
                {
                    AnsiConsole.MarkupLine("[red]No parking history found![/]");
                    Console.ReadKey();
                    return;
                }

                // Create a table to display parking history
                var table = new Table();
                table.AddColumn("[yellow]Car Model[/]");
                table.AddColumn("[yellow]License Plate[/]");
                table.AddColumn("[yellow]Start Time[/]");
                table.AddColumn("[yellow]End Time[/]");
                table.AddColumn("[yellow]Duration (Hours)[/]");
                table.AddColumn("[yellow]Total Cost (SEK)[/]");

                foreach (var parking in userParkings)
                {
                    // Get the car associated with the current parking
                    var car = db.Cars.FirstOrDefault(c => c.CarId == parking.CarId);
                    if (car == null) continue;

                    DateTime startTime = parking.Timestamp ?? DateTime.Now;

                    // Ensure duration is safely converted, even if null
                    decimal? duration = parking.Duration;
                    decimal durationValue = duration ?? 0m;

                    DateTime endTime = startTime.AddHours((double)durationValue);
                    decimal totalCost = durationValue * 10m; // Assuming 10 SEK per hour as the cost

                    // Add a row to the table for this parking record
                    table.AddRow(
                        car.Model ?? "N/A",
                        car.LicensePlate ?? "N/A",
                        $"{startTime:yyyy-MM-dd HH:mm}",
                        $"{endTime:yyyy-MM-dd HH:mm}",
                        $"{durationValue:F2}",
                        $"{totalCost:C}"
                    );
                }
                AnsiConsole.Write(table);
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
        public static void ViewCurrentParking(User user)
        {
            using (var db = new ParkMate20Context())
            {
                // Hämta aktiv parkering
                var activeParking = db.Parkings
                    .Include(p => p.Car) // Inkludera bilar
                    .Where(p => p.Car.UserId == user.UserId && p.Duration == 0)
                    .OrderByDescending(p => p.Timestamp)
                    .FirstOrDefault();

                if (activeParking == null)
                {
                    AnsiConsole.MarkupLine("[red]You do not have an active parking session.[/]");
                    Console.ReadKey();
                    return;
                }

                var parkingSpot = ParkingSpotData.GetAllSpots()
                    .FirstOrDefault(p => p.Name == activeParking.ParkingSpotName);

                if (parkingSpot == null)
                {
                    AnsiConsole.MarkupLine("[red]Error retrieving parking spot details![/]");
                    Console.ReadKey();
                    return;
                }

                // Beräkna aktuell tid och kostnad
                DateTime startTime = activeParking.Timestamp ?? DateTime.Now;
                TimeSpan duration = DateTime.Now - startTime;
                decimal totalHours = (decimal)duration.TotalHours;
                decimal estimatedCost = totalHours * parkingSpot.PricePerHour;

                // Visa aktuell parkering
                Console.Clear();
                AnsiConsole.MarkupLine("[blue]Current Parking Session:[/]");

                var table = new Table();
                table.AddColumn("[yellow]Car Model[/]");
                table.AddColumn("[yellow]License Plate[/]");
                table.AddColumn("[yellow]Start Time[/]");
                table.AddColumn("[yellow]Elapsed Time (Hours)[/]");
                table.AddColumn("[yellow]Estimated Cost (SEK)[/]");

                table.AddRow(
                    activeParking.Car?.Model ?? "N/A",
                    activeParking.Car?.LicensePlate ?? "N/A",
                    $"{startTime:yyyy-MM-dd HH:mm}",
                    $"{totalHours:F2}",
                    $"{estimatedCost:C}"
                );

                AnsiConsole.Write(table);
                Console.ReadKey();
            }
        }


    }
}
