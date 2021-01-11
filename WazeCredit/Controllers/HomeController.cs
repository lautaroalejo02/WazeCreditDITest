using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WazeCredit.Models;
using WazeCredit.Models.ViewModels;
using WazeCredit.Service;
using WazeCredit.Utility.AppSettingsClasses;

namespace WazeCredit.Controllers
{
    public class HomeController : Controller
    {
        public HomeVM homeVM { get; set; }
        //Is good to use the readonly property to avoid any accident 
        //Constructor injection
        private readonly IMarketForecaster _marketForecaster;
        private readonly ICreditValidator _creditValidator;
        //stripsettings are being used with action injection. That's the reason why is commented.
        //private readonly StripeSettings _stripeOptions;
        private readonly SendGridSettings _sendGridOptions;
        private readonly TwilioSettings _twilioOptions;
        private readonly WazeForecastSettings _wazeOptions;
        [BindProperty]
        public CreditApplication CreditModel { get; set; }

        public HomeController(
            IMarketForecaster marketForecaster,
           // IOptions<StripeSettings> stripeOptions,
            IOptions<SendGridSettings> sendGridOptions,
            IOptions<TwilioSettings> twilioOptions,
            IOptions<WazeForecastSettings> wazeOptions,
            ICreditValidator creditValidator
            )
        {
            homeVM = new HomeVM();
            _marketForecaster = marketForecaster;
            //_stripeOptions is commented because we are using it with ACTION INJECTION instead constructor Injection.
            //_stripeOptions = stripeOptions.Value;
            _sendGridOptions = sendGridOptions.Value;
            _twilioOptions = twilioOptions.Value;
            _wazeOptions = wazeOptions.Value;
            _creditValidator = creditValidator;
        }

        public IActionResult Index()
        {
            //Arriba hacemos lo mismo
            //HomeVM homeVM = new HomeVM();
            //MarketForecasterV2 marketForecasterV2 = new MarketForecasterV2();
           // MarketForecaster marketForecaster = new MarketForecaster();
            MarketResult currentMarket = _marketForecaster.GetMarketPrediction();
            switch (currentMarket.MarketCondition)
            {
                case MarketCondition.StableDown:
                    homeVM.MarketForecast = "Market shows signs to go down in a stable state";
                    break;
                case MarketCondition.StableUp:
                    homeVM.MarketForecast = "Market shows signs to go up in a stable state! It is a great sign to apply for credit applications!";
                    break;
                case MarketCondition.Volatile:
                    homeVM.MarketForecast = "Market shows signs of volatility. In uncertain times, it is good to have credit handy if you need extra funds!";
                    break;
                default:
                    homeVM.MarketForecast = "Apply for a credit card using our application!";
                    break;

            }
            return View(homeVM);
        }
        public IActionResult AllConfigSettings(
            //ACTION ACTION
            [FromServices] IOptions<StripeSettings> stripeOptions
            )
        {
            
            
            List<string> messages = new List<string>();
            messages.Add($"Waze config - Forecast Tracker: " + _wazeOptions.ForecastTrackerEnabled);
            messages.Add($"Stripe Publishable Key:" + stripeOptions.Value.PublishableKey);
            messages.Add($"Stripe Secret Key:" + stripeOptions.Value.SecretKey);
            messages.Add($"Send Grid Key:" + _sendGridOptions.SendGridKey);
            messages.Add($"Twilio Phone:" + _twilioOptions.PhoneNumber);
            messages.Add($"Twilio SID" + _twilioOptions.AccountSid);
            messages.Add($"Twilio Token" + _twilioOptions.AuthToken);
            return View(messages);

        }
        public IActionResult CreditApplication()
        {
            CreditModel = new CreditApplication();
            return View(CreditModel);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]  
        [ActionName("CreditApplication")]
        public async Task<IActionResult> CreditApplicationPOST()
        {
            if (ModelState.IsValid)
            {
                var (validationPassed, errorMessages) = await _creditValidator.PassAllValidations(CreditModel);
                CreditResult creditResult = new CreditResult()
                {  
                    ErrorList = errorMessages,
                    CreditID = 0,
                    Success = validationPassed
                };
                if (validationPassed)
                {
                    //Add record to database;;
                    return RedirectToAction(nameof(CreditResult), creditResult);
                }
                else
                {
                    return RedirectToAction(nameof(CreditResult), creditResult);
                    //return RedirectToAction(nameof(CreditResult), creditResult);
                    //nameof expression prodcuce the name of a varibale,type or member as the stirng constant.
                }
            }
            return View(CreditModel);
        }
        public IActionResult CreditResult(CreditResult creditResult)
        {
            return View(creditResult);
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
