using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NewStore.Models;
using Store.Models;
using Store.Services;

namespace Store.Controllers
{
    public class StoreController : Controller
    {
        private readonly AppDbContext context;
        private readonly int pageSize = 8;
        public StoreController(AppDbContext context)
        {
            this.context = context;
        }
        public IActionResult Index(int pageIndex, string? search, string? brand, string? category, string? sort)
        {
            IQueryable<Product> query = context.Products;

            var brands = query.Select(x => x.Brand).Distinct();
            var categories = query.Select(x => x.Category).Distinct();


            //search
            if (search != null && search.Length > 0) 
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            //brand filtering
            if(brand != null && brand.Length > 0)
            {
                query = query.Where(p => p.Brand.Contains(brand));
            }


            //category filtering
            if (category != null && category.Length > 0)
            {
                query = query.Where(p => p.Category.Contains(category));
            }


            //sort
            if (sort == "price_asc")
            {
                query = query.OrderBy(p => p.Price);
            }
            else if (sort == "price_desc")
            {
                query = query.OrderByDescending(p => p.Price);
            }
            else
            {
                //newest
                query = query.OrderByDescending(p => p.Id);
            }

            //pagination
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }
            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count/ pageSize);
            query = query.Skip((pageIndex-1)*pageSize).Take(pageSize);
            var products = query.ToList();
            ViewBag.TotalPages = totalPages;
            ViewBag.PageIndex = pageIndex;
            //
            ViewBag.Brands = brands;
            ViewBag.Categories = categories;
            ViewBag.Products = products;

            //search & sort

            var storeSearchModel = new StoreSearchModel()
            {
                Search = search,
                Brand = brand,
                Category = category,
                Sort = sort
            };
            return View(storeSearchModel);
        }


        public IActionResult Details(int id)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                RedirectToAction("Index", "Store");
            }
            return View(product);
        }
    }

    
}
