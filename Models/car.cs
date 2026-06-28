namespace car_rental_system.Models
{
    public class car
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public int PriceWithoutFuel { get; set; }
        public int PriceWithFuel { get; set; }
        public int DriverCharges { get; set; } = 1500;
        public bool IsBooked { get; set; } = false;
    }
}