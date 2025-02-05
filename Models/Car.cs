using System;
using System.Collections.Generic;

namespace ParkMate2._0.Models;

public partial class Car
{
    public int CarId { get; set; }

    public int UserId { get; set; }

    public string LicensePlate { get; set; } = null!;

    public string Model { get; set; } = null!;

    public virtual ICollection<Parking> Parkings { get; set; } = new List<Parking>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<UserCar> UserCars { get; set; } = new List<UserCar>();

}
