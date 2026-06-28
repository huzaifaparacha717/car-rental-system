using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using car_rental_system.Data;
using car_rental_system.Models;
using System.Collections.Generic;
using System.Linq;

namespace car_rental_system.Controllers
{
    public class CarController : Controller
    {
        public static List<car> CarList = new List<car>
        {
            new car { Id = 1, Name = "Land Cruiser", Model = "V8 ZX", PriceWithoutFuel = 35000, PriceWithFuel = 50000, IsBooked = false },
            new car { Id = 2, Name = "Fortuner", Model = "Legender", PriceWithoutFuel = 20000, PriceWithFuel = 30000, IsBooked = false },
            new car { Id = 3, Name = "Honda City", Model = "Aspire", PriceWithoutFuel = 6000, PriceWithFuel = 9500, IsBooked = false },
            new car { Id = 4, Name = "Honda BR-V", Model = "i-VTEC", PriceWithoutFuel = 8000, PriceWithFuel = 12000, IsBooked = false },
            new car { Id = 5, Name = "Suzuki Swift", Model = "GLX", PriceWithoutFuel = 5000, PriceWithFuel = 8000, IsBooked = false },
            new car { Id = 6, Name = "Toyota Yaris", Model = "ATIV X", PriceWithoutFuel = 5500, PriceWithFuel = 8500, IsBooked = false }
        };

        public IActionResult Index()
        {
            return View(CarList);
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new car());
        }

        [Authorize(Roles = IdentitySeed.AdminRole)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(car newCar)
        {
            if (!ModelState.IsValid)
                return View(newCar);

            newCar.Id = CarList.Count == 0 ? 1 : CarList.Max(c => c.Id) + 1;
            newCar.IsBooked = false;
            CarList.Add(newCar);
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public IActionResult Book(int id)
        {
            var car = CarList.FirstOrDefault(x => x.Id == id);
            if (car == null) return NotFound();

            var b = new Booking { CarId = car.Id, CarName = car.Name };
            return View(b);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Book(Booking b)
        {
            var car = CarList.FirstOrDefault(x => x.Id == b.CarId);
            if (car != null)
            {
                car.IsBooked = true;
                int days = (b.EndDate - b.StartDate).Days;
                if (days <= 0) days = 1;

                // Fuel aur Driver Logic
                int baseRate = (b.SelectedFuelType == "WithFuel") ? car.PriceWithFuel : car.PriceWithoutFuel;
                int total = days * baseRate;

                if (b.NeedDriver) { total += (days * car.DriverCharges); }

                b.TotalRent = total;
                return View("BookingSuccess", b);
            }
            return RedirectToAction("Index");
        }
    }
}