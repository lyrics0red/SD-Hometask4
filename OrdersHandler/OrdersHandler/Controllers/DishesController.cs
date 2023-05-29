using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrdersHandler.Data;
using OrdersHandler.Models;
using OrdersHandler.Tools;

namespace OrdersHandler.Controllers
{
    public class DishesController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly Context _context;
        static private int idDish = 1;

        public DishesController(Context context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("FindCountOfDish")]
        public async Task<ActionResult<string>> FindDishById(int dishId, int userId)
        {
            var dish = await _context.Dishes.FirstOrDefaultAsync(dish => dish.Id == dishId);
            if (dish == null)
            {
                return BadRequest("Invalid dish ID.");
            }
            return Ok($"Dish is available for {dish.Quantity} portions.");
        }

        [HttpDelete("RemoveDishFromMenu")]
        public async Task<ActionResult<string>> RemoveFromMenu(int dishId, int userId)
        {
            var dish = await _context.Dishes.FirstOrDefaultAsync(dish => dish.Id == dishId);
            if (dish == null)
            {
                return BadRequest("Invalid dish ID.");
            }
            _context.Dishes.Remove(dish);
            _context.SaveChanges();
            return Ok("Dish has been removed successfully");
        }

        [HttpPost("AddDishToMenu")]
        public async Task<ActionResult<string>> AddToMenu(int userId, DishForm newDish)
        {
            var dish = await _context.Dishes.FirstOrDefaultAsync(dish => dish.Name == newDish.Name);
            if (dish != null)
            {
                return BadRequest("Dish with this name already exists.");
            }
            dish = new Models.Dish();
            dish.Id = idDish;
            dish.Price = newDish.Price;
            dish.Description = newDish.Description;
            dish.Quantity = newDish.Quantity;
            dish.Name = newDish.Name;
            idDish++;
            _context.Dishes.Add(dish);
            _context.SaveChanges();
            return Ok("New dish added successfully.");
        }

        [HttpPut("ChangeDishInfo")]
        public async Task<ActionResult<string>> ChangeDishInfo(int userId, DishForm newDish)
        {
            var dish = await _context.Dishes.FirstOrDefaultAsync(dish => dish.Name == newDish.Name);
            if (dish == null)
            {
                return BadRequest("Dish with this name does not exist.");
            }
            dish.Price = newDish.Price;
            dish.Description = newDish.Description;
            dish.Quantity = newDish.Quantity;
            dish.Name = newDish.Name;
            _context.SaveChanges();
            return Ok("Dish information has been updated successfully.");
        }

        [HttpGet("ShowMenu")]
        public async Task<ActionResult<List<Dish>>> ShowMenu()
        {
            var menu = _context.Dishes.Where(dish => dish.Quantity > 0);
            return Ok(menu.ToList());
        }
    }
}
