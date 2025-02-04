using System;
using System.Collections.Generic;

namespace ParkMate2._0.Models;

public partial class Parking
{
    public int ParkingId { get; set; }

    public int CarId { get; set; }

    public DateTime? Timestamp { get; set; }

    public decimal Duration { get; set; }

    public string PayMethod { get; set; } = null!;

    public virtual Car Car { get; set; } = null!;
}
