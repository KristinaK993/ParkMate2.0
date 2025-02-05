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

            //Lägg till bilregistrering direkt efter registrering
            CarHelper.RegisterCar(newUser);

            AnsiConsole.MarkupLine($"[green]Welcome, {newUser.UserName}! You are now registered and a car is linked to your account.[/]");
            Console.ReadKey();

            return newUser; // Returnerar den nya användaren
        }
    }
}
