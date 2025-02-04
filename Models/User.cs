using System;
using System.Collections.Generic;

namespace ParkMate2._0.Models;

public partial class User
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();

    public virtual ICollection<UserCar> UserCars { get; set; } = new List<UserCar>();
}
