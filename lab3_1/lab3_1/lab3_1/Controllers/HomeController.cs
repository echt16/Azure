using lab3_1.Models.Database;
using lab3_1.Models.Services;
using lab3_1.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace lab3_1.Controllers
{
    public class HomeController : Controller
    {
        private AppService AppService { get; set; }
        private StorageSystemDbContext Context { get; set; }
        public HomeController(StorageSystemDbContext db)
        {
            Context = db;
            AppService = new AppService(db);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string? token = HttpContext.Session.GetString("token");
            string? key = HttpContext.Session.GetString("key");

            if (AppService.IsAuthorizathed(token, key) == true)
            {
                int id = AppService.GetUserId(token, key);
                AppService.SetUserId(id);
            }

            base.OnActionExecuting(context);
        }

        [HttpPost]
        public IActionResult CheckLogin(string login)
        {
            if (AppService.IsLoginAvailable(login))
                return Ok();
            else
                return BadRequest();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string login, string password)
        {
            AuthorizationModel am = await AppService.AuthorizeUser(login, password);
            if (am == null)
            {
                return View();
            }
            HttpContext.Session.SetString("token", am.Token);
            HttpContext.Session.SetString("key", am.Key);
            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Registration(string login, string password, string firstname, string lastname)
        {
            if(await AppService.RegisterUser(login, password, firstname, lastname))
            {
                return View("Login");
            }  
            return View();
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
