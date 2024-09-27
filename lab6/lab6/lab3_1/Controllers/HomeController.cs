using lab3_1.Models.Database;
using lab3_1.Models.Services;
using lab3_1.Models.Services.DatabaseServices;
using lab3_1.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace lab3_1.Controllers
{
    public class HomeController : Controller
    {
        private AppService AppService { get; set; }
        private IServiceScopeFactory ScopeFactory { get; set; }

        public HomeController(IServiceScopeFactory scopeFactory)
        {
            ScopeFactory = scopeFactory;

            var databaseService = new DatabaseService(ScopeFactory);

            AppService = new AppService(databaseService);
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
            if (await AppService.RegisterUser(login, password, firstname, lastname))
            {
                return View("Login");
            }
            return View();
        }

        [HttpGet]
        public IActionResult SendFile()
        {
            return View("SendFiles");
        }

        [HttpPost]
        public IActionResult UploadFile(IFormFile file)
        {

            return View("SendFiles");
        }

        public IActionResult Privacy()
        {
            //AppService.Bsp();
            return View();
        }

        public IActionResult Allo()
        {
            //AppService.Bsp2();
            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
