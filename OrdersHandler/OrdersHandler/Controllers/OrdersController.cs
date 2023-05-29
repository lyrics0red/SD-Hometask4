using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrdersHandler.Data;
using OrdersHandler.Models;
using OrdersHandler.Tools;
using System.Runtime.CompilerServices;

namespace OrdersHandler.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly Context _context;
        static private int idOrder = 1;
        static private int idOrderDish = 1;

        public OrdersController(Context context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("FindOrder/{id}")]
        public async Task<ActionResult<OrderInfo>> FindOrder(int id)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(order => order.Id == id);
            if (order == null)
            {
                return BadRequest("Invalid order ID.");
            }
            var info = new OrderInfo();
            info.Status = order.Status;
            var dishes = _context.OrderDishes.Where(orderDish => orderDish.OrderId == order.Id);
            foreach (var dish in dishes)
            {
                info.Dishes.Add(_context.Dishes.FirstOrDefault(meal => meal.Id == dish.DishId));
            }
            return Ok(info);
        }

        [HttpPost("CreateOrder")]
        public async Task<ActionResult<string>> CreateOrder(OrderForm order)
        {
            if (order.Counts.Count != order.DishIds.Count)
            {
                return BadRequest("Invalid number of dishes counts.");
            }
            for (int i = 0; i < order.DishIds.Count; ++i) 
            {
                var dish = await _context.Dishes.FirstOrDefaultAsync(dish => dish.Id == order.DishIds[i]);
                if (dish == null)
                {
                    return BadRequest("Order includes non-existing dish IDs");
                }
                if (dish.Quantity - order.Counts[i] < 0)
                {
                    return BadRequest("There are no enough portions.");
                }
            }
            for (int i = 0; i < order.DishIds.Count; ++i)
            {
                var dish = await _context.Dishes.FirstOrDefaultAsync(dish => dish.Id == order.DishIds[i]);
                dish.Quantity -= order.Counts[i];
                _context.SaveChanges();
                var orderDish = new OrderDish();
                orderDish.Id = idOrderDish;
                orderDish.OrderId = idOrder;
                orderDish.DishId = dish.Id;
                orderDish.Quantity = order.Counts[i];
                orderDish.Price = order.Counts[i] * dish.Price;
                _context.OrderDishes.Add(orderDish);
                _context.SaveChanges();
            }
            var newOrder = new Order();
            newOrder.Id = idOrder;
            newOrder.UserId = order.UserId;
            newOrder.Status = "Preparing";
            newOrder.CreatedAt = DateTime.Now;
            newOrder.UpdatedAt = DateTime.Now;
            _context.SaveChanges();
            idOrder++;
            return Ok("Order is adopted.");
        }
    }
}
