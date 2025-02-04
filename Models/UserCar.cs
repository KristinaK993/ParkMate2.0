using System;
using System.Collections.Generic;

namespace ParkMate2._0.Models;

public partial class UserCar
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CarId { get; set; }

    public virtual Car Car { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
