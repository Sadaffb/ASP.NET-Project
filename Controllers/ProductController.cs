using Microsoft.AspNetCore.Mvc;
using Store.Models;
using Store.Services;
using System.Drawing.Drawing2D;

namespace Store.Controllers
{
    [Route("/Admin/[controller]/{action=Index}/{id?}")]
    public class ProductController : Controller
    {
        private readonly AppDbContext context;
		private readonly IWebHostEnvironment environment;
        private readonly int pageSize = 4;

		public ProductController(AppDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
			this.environment = environment;
		}
        public IActionResult Index(int pageIndex , string? search, string? column, string? orderBy)
        {
            IQueryable<Product> query = context.Products;

            //Search
            if (search != null)
            {
                query=query.Where(p=>p.Name.Contains(search) || p.Brand.Contains(search));
            }

            //sort
            string[] validColumn = { "Id", "Name", "Brand", "Category", "Price", "CreatedAt" };
            string[] validOrderBy = { "desc", "asc" };

            if (!validColumn.Contains(column))
            {
                column = "Id";
            }
            if (!validOrderBy.Contains(orderBy))
            {
                orderBy= "desc";
            }

            if (column == "Name")
            {
                if(orderBy == "asc")
                {
                    query = query.OrderBy(p => p.Name);
                }
                else
                {
                    query=query.OrderByDescending(p => p.Name);
                }
            }
            else if (column == "Brand")
			{
				if (orderBy == "asc")
				{
					query = query.OrderBy(p => p.Brand);
				}
				else
				{
					query = query.OrderByDescending(p => p.Brand);
				}
			}
            else if (column == "Category")
			{
				if (orderBy == "asc")
				{
					query = query.OrderBy(p => p.Category);
				}
				else
				{
					query = query.OrderByDescending(p => p.Category);
				}
			}
            else if (column == "Price")
			{
				if (orderBy == "asc")
				{
					query = query.OrderBy(p => p.Price);
				}
				else
				{
					query = query.OrderByDescending(p => p.Price);
				}
			}
            else if (column == "CreatedAt")
			{
				if (orderBy == "asc")
				{
					query = query.OrderBy(p => p.CreatedAt);
				}
				else
				{
					query = query.OrderByDescending(p => p.CreatedAt);
				}
			}
            else
			{
				if (orderBy == "asc")
				{
					query = query.OrderBy(p => p.Id);
				}
				else
				{
					query = query.OrderByDescending(p => p.Id);
				}
			}



			//pagination
			if (pageIndex < 1)
            {
                pageIndex = 1;
            }
            decimal count=query.Count();
            int totalPages=(int)Math.Ceiling(count/pageSize);
            query=query.Skip((pageIndex-1)*pageSize).Take(pageSize);
            var products = query.ToList();

            ViewData["PageIndex"] = pageIndex;
            ViewData["TotalPage"] = totalPages;

            //search
            ViewData["search"] = search ?? "";

            //sort
            ViewData["column"] = column;
            ViewData["orderBy"]=orderBy;
            return View(products);
        }

		public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
		public IActionResult Create(ProductDto productDto)
		{
            if(productDto.ImageFile == null) {
                ModelState.AddModelError("ImageFile", "the image is required");
            }
            if (!ModelState.IsValid)
            {
                return View(productDto);
            }

            string newFileName = DateTime.Now.ToString("yyyymmddhhmmssfff");
            newFileName += Path.GetExtension(productDto.ImageFile!.FileName);

            string imageFullPath= environment.WebRootPath+ "/product/" + newFileName;
            using(var stream= System.IO.File.Create(imageFullPath))
            {
				productDto.ImageFile.CopyTo(stream);
            }

            Product product = new Product()
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Category = productDto.Category,
                Price = productDto.Price,
                Description = productDto.Description,
                ImageFileName = newFileName,
                CreatedAt = DateTime.Now
            };
            context.Products.Add(product);
            context.SaveChanges();
			return RedirectToAction("index", "Product");
		}


		public IActionResult Edit(int id)
		{
            var product = context.Products.Find(id);
            if (product == null)
            {
                RedirectToAction("index", "Product");
            }

            var productDto = new ProductDto()
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description,
            };

            ViewData["productId"] = product.Id;
            ViewData["productImage"] = product.ImageFileName;
            ViewData["productCreateAt"] = product.CreatedAt.ToString("MM/dd/yyyy");

			return View(productDto);
		}

        [HttpPost]
        public IActionResult Edit(int id, ProductDto productDto)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("index", "Product");
            }
            if (!ModelState.IsValid)
            {
				ViewData["productId"] = product.Id;
				ViewData["productImage"] = product.ImageFileName;
				ViewData["productCreateAt"] = product.CreatedAt.ToString("MM/dd/yyyy");
				return View(productDto);
            }

            string newFileName = product.ImageFileName;
            if (productDto.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyymmddhhssfff");
                newFileName += Path.GetExtension(productDto.ImageFile.FileName);

                string imageFullPath = environment.WebRootPath + "/product/" + newFileName;
                using(var stream = System.IO.File.Create(imageFullPath))
                {
                    productDto.ImageFile.CopyTo(stream);
                }

                string oldImageFullPath = environment.WebRootPath + "/product/" + product.ImageFileName;
                System.IO.File.Delete(oldImageFullPath);
            }

            product.Name = productDto.Name;
            product.Brand = productDto.Brand;
            product.Category = productDto.Category;
            product.Price = productDto.Price;
            product.Description=productDto.Description;
            product.ImageFileName= newFileName;

            context.SaveChanges();
            return RedirectToAction("Index", "Product");

        }


        public IActionResult Delete (int id)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                RedirectToAction("index", "Product");
            }
            string imageFileName = environment.WebRootPath + "/product/" + product.ImageFileName;
            System.IO.File.Delete(imageFileName);

            context.Products.Remove(product);
            context.SaveChanges(true);

            return RedirectToAction("index", "Product");
        }
	}

    
}
