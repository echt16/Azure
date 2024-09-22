using lab3.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Newtonsoft.Json;

namespace lab3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private List<string> currencies;

        private string queueName;

        
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            queueName = "lots-queue";
            currencies = new List<string>()
            {
                "usd", "eur", "cad", "cny", "sek"
            };
        }

        public async Task<IActionResult> Index()
        {
            MainModel model = new MainModel()
            {
                Lots = await QueueService.PeekMessagesAsync(queueName, 10)
            };
            return View(model);
        }



        [HttpPost]
        public async Task<IActionResult> LotPlacing(string currency, double sum, string lastname)
        {
            if (sum < 0)
            {
                return BadRequest("Currency must be positiv");
            }

            Lot lot = new Lot() { Currency = currency, Lastname = lastname, Sum = sum };
            await QueueService.SendMessageAsync(queueName, JsonConvert.SerializeObject(lot), 86400);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult LotPlacing()
        {
            return View(new LotPlacingModel() { Currencies = this.currencies});
        }

        [HttpGet]
        public IActionResult LotGet()
        {
            return View(new LotGetModel() { Currencies = this.currencies});
        }

        [HttpGet]
        public async Task<IActionResult> LotGetByCurrancy(string currency)
        {
            List<Lot> lots = await QueueService.PeekMessagesAsync(queueName, 10);
            LotGetModel lotGetModel = new LotGetModel() { Currencies = this.currencies };
            lotGetModel.CurrentCurrency = currency;
            foreach (Lot lot in lots)
            {
                if (lot.Currency == currency)
                {
                    lotGetModel.Lots.Add(lot);
                }
            }

            return View("LotGet", lotGetModel);
        }

        [HttpPost]
        public async Task<IActionResult> LotTake(string lotId)
        {
            await QueueService.DeleteMessageAsync(queueName, lotId);
            return RedirectToAction("Index");
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
