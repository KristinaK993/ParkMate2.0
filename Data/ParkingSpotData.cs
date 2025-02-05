public static class ParkingSpotData
{
    private static List<ParkingSpot> parkingSpots = new List<ParkingSpot>
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

    // Metod för att hämta alla parkeringsplatser
    public static List<ParkingSpot> GetAllSpots()
    {
        return parkingSpots;
    }
}
