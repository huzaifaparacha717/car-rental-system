using System;
using System.ComponentModel.DataAnnotations;

namespace car_rental_system.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }
        public int CarId { get; set; }
        public string CarName { get; set; }
        public string CustomerName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool NeedDriver { get; set; }
        public string SelectedFuelType { get; set; }
        public int TotalRent { get; set; }
    }
}