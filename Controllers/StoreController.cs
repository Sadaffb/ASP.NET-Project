using Microsoft.AspNetCore.Mvc;
using Store.Services;

namespace Store.Controllers
{
    public class StoreController : Controller
    {
        private readonly AppDbContext context;

        public StoreController(AppDbContext context)
        {
            this.context = context;
        }
        public IActionResult Index()
        {
            var products = context.Products.OrderByDescending(p=>p.Id).ToList();
            ViewBag.Products = products;
            return View();
        }
    }
}
