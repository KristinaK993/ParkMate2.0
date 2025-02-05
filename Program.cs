using Microsoft.EntityFrameworkCore;
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
                    Register();
                    break;
                case "Exit":
                    return;
            }
        }
    }

    static void Login() //Logga in
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

                //Skicka användaren till användarmenyn
                AnsiConsole.MarkupLine($"[green]Welcome back, {user.UserName}![/]");
                Console.ReadKey();
                UserMenu(user);
                return;
            }
        }
    }

    //Registrera användare
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


            RegisterCar(newUser);//Registrera en bil direkt efter att användaren skapats

            //Skicka användaren till menyn efter registrering
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
            //Anropa den lagrade proceduren 'AddCar' för att lägga till en bil
            db.Database.ExecuteSqlRaw(
                "EXEC dbo.AddCar @UserId = {0}, @LicensePlate = {1}, @Model = {2}",
                user.UserId, licensePlate, model);
        }

        AnsiConsole.MarkupLine($"[green]Car {model} ({licensePlate}) registered successfully![/]");
        Console.ReadKey();
    }
    static void RemoveCar(User user) //ta bort en bil
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
    static void ViewCars(User user)
    {
        Console.Clear();
        using (var db = new ParkMate20Context())
        {
            var cars = db.Cars.Where(c => c.UserId == user.UserId).ToList();

            if (!cars.Any())
            {
                AnsiConsole.MarkupLine("[red]You have no registered cars![/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[blue]Your Registered Cars:[/]");
                foreach (var car in cars)
                {
                    AnsiConsole.MarkupLine($"[green]- {car.Model} ({car.LicensePlate})[/]");
                }
            }

            Console.ReadKey();
        }
    }

    static void UserMenu(User loggedInUser)   //Användarmenyn
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
    "Manage Cars", // ✅ Se till att detta matchar exakt
    "Log Out"
};
            var choice = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("[yellow]What do you want to do?[/]")
        .AddChoices(options));

            AnsiConsole.MarkupLine($"[yellow]DEBUG: Choice received -> {choice}[/]");

            switch (choice)
            {
                case "Start Parking":
                    StartParking(loggedInUser);
                    break;
                case "End Parking":
                    EndParking(loggedInUser);
                    break;
                case "Parking History":
                    ShowParkingHistory(loggedInUser); //historik för användaren
                    break;
                case "Manage Cars":
                    CarMenu(loggedInUser); //Hantera bilar
                    break;
                case "Log Out":
                    return; //Går tillbaka till huvudmenyn
            }
        }
    }


    static void CarMenu(User user) //Hantera bilar
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
    }
    static void StartParking(User user)
    {
        Console.Clear();
        using (var db = new ParkMate20Context())
        {
            var cars = db.Cars.Where(c => c.UserId == user.UserId).ToList();

            if (!cars.Any())
            {
                AnsiConsole.MarkupLine("[red]You have no registered cars! Register a car first.[/]");
                Console.ReadKey();
                return;
            }

            //Välj bil att parkera
            var carChoices = cars.Select(c => $"{c.Model} ({c.LicensePlate})").ToList();
            var selectedCar = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Select a car to start parking:[/]")
                    .AddChoices(carChoices));

            var car = cars.FirstOrDefault(c => $"{c.Model} ({c.LicensePlate})" == selectedCar);

            // Välj en parkeringsplats
            var selectedParkingSpot = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Select a parking spot in Gothenburg:[/]")
                    .AddChoices(parkingSpots.Select(p => $"{p.Name} - {p.PricePerHour} SEK/hour")));

            var parkingSpot = parkingSpots.First(p => selectedParkingSpot.StartsWith(p.Name));

            //Välj betalningsmetod
            var paymentMethods = new List<string> { "Swish", "Credit Card", "Invoice" };
            var selectedPaymentMethod = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Select a payment method:[/]")
                    .AddChoices(paymentMethods));

            //Starta parkeringen och spara i databasen
            var newParking = new Parking
            {
                CarId = car.CarId,
                Timestamp = DateTime.Now,
                Duration = 0, // Beräknas senare vid EndParking
                PayMethod = selectedPaymentMethod
            };

            db.Parkings.Add(newParking);
            db.SaveChanges();

            AnsiConsole.MarkupLine($"[green]Parking started at {parkingSpot.Name} for {car.Model} ({car.LicensePlate})![/]");
            AnsiConsole.MarkupLine($"[yellow]Rate:[/] {parkingSpot.PricePerHour} SEK/hour | [yellow]Payment Method:[/] {selectedPaymentMethod}");
            Console.ReadKey();
        }
    }
    //Lista över parkeringsplatser i Göteborg
    static List<ParkingSpot> parkingSpots = new List<ParkingSpot>
{
    new ParkingSpot { Name = "Nordstan Garage", PricePerHour = 30 },
    new ParkingSpot { Name = "P-Hus Ullevi", PricePerHour = 25 },
    new ParkingSpot { Name = "Avenyn Parkering", PricePerHour = 28 },
    new ParkingSpot { Name = "Liseberg Parkering", PricePerHour = 35 },
    new ParkingSpot { Name = "Heden Parkering", PricePerHour = 20 },
    new ParkingSpot { Name = "Lindholmen Science Park", PricePerHour = 18 },
    new ParkingSpot { Name = "Frölunda Torg", PricePerHour = 15 },
    new ParkingSpot { Name = "Järntorget Parkering", PricePerHour = 22 },
    new ParkingSpot { Name = "Skanstorget Parkering", PricePerHour = 24 },
    new ParkingSpot { Name = "Backaplan", PricePerHour = 17 },
    new ParkingSpot { Name = "Mölndal Galleria", PricePerHour = 19 },
    new ParkingSpot { Name = "Eriksberg Parkering", PricePerHour = 21 },
    new ParkingSpot { Name = "Angered Centrum", PricePerHour = 14 },
    new ParkingSpot { Name = "Korsvägen Parkering", PricePerHour = 27 },
    new ParkingSpot { Name = "Chalmers Campus", PricePerHour = 23 }
};
    static void EndParking(User user)
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

            // 🔥 Hämta parkeringsplatsens pris per timme
            var parkingSpot = parkingSpots.FirstOrDefault(p => activeParking.PayMethod != null);
            if (parkingSpot == null)
            {
                AnsiConsole.MarkupLine("[red]Error retrieving parking spot details![/]");
                Console.ReadKey();
                return;
            }

            // 🕒 Beräkna tid och kostnad
            DateTime startTime = activeParking.Timestamp ?? DateTime.Now;
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;
            decimal totalHours = (decimal)duration.TotalHours;
            decimal totalCost = totalHours * parkingSpot.PricePerHour;

            // 🔥 Uppdatera parkeringen i databasen
            activeParking.Duration = totalHours;
            db.SaveChanges();

            // 🏁 Visa sammanfattning
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
                Console.ReadKey();
                return;
            }

            //en tabell
            var table = new Table();
            table.AddColumn("[yellow]Car Model[/]");
            table.AddColumn("[yellow]License Plate[/]");
            table.AddColumn("[yellow]Start Time[/]");
            table.AddColumn("[yellow]End Time[/]");
            table.AddColumn("[yellow]Duration (Hours)[/]");
            table.AddColumn("[yellow]Total Cost (SEK)[/]");

            foreach (var parking in userParkings)
            {
                var car = db.Cars.FirstOrDefault(c => c.CarId == parking.CarId);
                if (car == null) continue; // Hoppa över om bilen inte hittas

                DateTime startTime = parking.Timestamp ?? DateTime.Now; //start tiden
                DateTime endTime = startTime.AddHours((double)parking.Duration); //Konverterar duration från decimal till double

                decimal totalCost = parking.Duration * 10m; //totala tiden i timmar 

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

        Console.ReadKey();
    }

}

