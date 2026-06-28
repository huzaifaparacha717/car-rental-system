using car_rental_system.Data;
using car_rental_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace car_rental_system.Controllers
{
    [Authorize(Roles = IdentitySeed.AdminRole)]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var list = CarController.CarList;
            var vm = new AdminDashboardViewModel
            {
                TotalCars = list.Count,
                BookedCars = list.Count(c => c.IsBooked),
                AvailableCars = list.Count(c => !c.IsBooked),
                RegisteredUsers = await _userManager.Users.CountAsync()
            };
            return View(vm);
        }

        public IActionResult Cars()
        {
            return View(CarController.CarList.OrderBy(c => c.Id).ToList());
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vehicle = CarController.CarList.FirstOrDefault(c => c.Id == id);
            if (vehicle == null)
                return NotFound();
            return View(vehicle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, car vehicle)
        {
            if (id != vehicle.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(vehicle);

            var existing = CarController.CarList.FirstOrDefault(c => c.Id == id);
            if (existing == null)
                return NotFound();

            existing.Name = vehicle.Name;
            existing.Model = vehicle.Model;
            existing.PriceWithoutFuel = vehicle.PriceWithoutFuel;
            existing.PriceWithFuel = vehicle.PriceWithFuel;
            existing.DriverCharges = vehicle.DriverCharges;
            existing.IsBooked = vehicle.IsBooked;

            TempData["AdminMessage"] = "Car updated.";
            return RedirectToAction(nameof(Cars));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var vehicle = CarController.CarList.FirstOrDefault(c => c.Id == id);
            if (vehicle == null)
                return NotFound();
            return View(vehicle);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int id)
        {
            var vehicle = CarController.CarList.FirstOrDefault(c => c.Id == id);
            if (vehicle == null)
                return NotFound();

            CarController.CarList.Remove(vehicle);
            TempData["AdminMessage"] = "Car removed from fleet.";
            return RedirectToAction(nameof(Cars));
        }
    }
}
