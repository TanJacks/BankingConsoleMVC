using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace BankingWebApp.Controllers
{
    public class HomeController : Controller
    {
        private string cacheDirectory = ConfigurationManager.AppSettings["localCacheDirectory"];
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Simple banking MVC web app interfacing a console banking app.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Feel free to call if you have any questions.  Did I get the job?";

            return View();
        }


        [Authorize]
        public ActionResult Balance(BankingWebApp.Models.BankAccountModels.BalanceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string sessionUsername = (string)Session["Username"];
            Int64 sessionAccountNumber = (Int64)Session["AccountNumber"];
            BankingConsole.Account account;
            if ((account = BankingConsole.AccountManager.GetAccount(sessionUsername, sessionAccountNumber)) != null)
            {
                model.Account = account;
                return View(model);
            }
            ModelState.AddModelError("", "Error processing account information.");
            
            return View(model);
        }

        [Authorize]
        public ActionResult Deposit(BankingWebApp.Models.BankAccountModels.DepositViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (model.DepositAmount <= 0)
            {
                ModelState.AddModelError("", "Deposit amount cannot be less than 0.");
                return View(model);
            }
            
            string sessionUsername = (string)Session["Username"];
            Int64 sessionAccountNumber = (Int64)Session["AccountNumber"];
            BankingConsole.Account account;
            if ((account = BankingConsole.AccountManager.GetAccount(sessionUsername, sessionAccountNumber)) != null)
            {
                double newBalance = account.Deposit(model.DepositAmount);
                model.Account = account;
                return RedirectToAction("Balance");
            }
            ModelState.AddModelError("", "Error processing account information.");
            
            return View(model);
        }

        [Authorize]
        public ActionResult Withdraw(BankingWebApp.Models.BankAccountModels.WithdrawViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
                        
            string sessionUsername = (string)Session["Username"];
            Int64 sessionAccountNumber = (Int64)Session["AccountNumber"];
            BankingConsole.Account account;
            if ((account = BankingConsole.AccountManager.GetAccount(sessionUsername, sessionAccountNumber)) != null)
            {
                if (model.WithdrawAmount > account.Balance)
                {
                    ModelState.AddModelError("", "Insufficient funds. No withdrawal made.");
                    return View(model);
                }
                double newBalance = account.Withdrawal(model.WithdrawAmount);
                model.Account = account;
                return RedirectToAction("Balance");
            }
            ModelState.AddModelError("", "Error processing account information.");
            
            return View(model);
        }

        [Authorize]
        public ActionResult TransactionList()
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            string sessionUsername = (string)Session["Username"];
            Int64 sessionAccountNumber = (Int64)Session["AccountNumber"];
            BankingConsole.Account account;
            if ((account = BankingConsole.AccountManager.GetAccount(sessionUsername, sessionAccountNumber)) != null)
            {
                List<BankingWebApp.Models.BankAccountModels.TransactionsViewModel> tranList = new List<Models.BankAccountModels.TransactionsViewModel>();
                foreach (BankingConsole.BankTransaction bt in account.TransactionHistory)
                {
                    tranList.Add(new Models.BankAccountModels.TransactionsViewModel(((int)bt.TransactionType) == 0 ? "Deposit" : "Withdrawal", bt.Amount, bt.PreviousBalance, bt.NewBalance));
                }                    
                return View(tranList);
            }
            ModelState.AddModelError("", "Error processing account information.");
            return View();            
        }

        [Authorize]
        public ActionResult CreateBankAccount(Models.BankAccountModels.CreateBankViewModel model)
        {
            string username = (string)TempData["Username"];
            string password = (string)TempData["Password"];
            string firstname = ((string)TempData["FirstName"]) ?? model.FirstName;
            string lastname = ((string)TempData["LastName"]) ?? model.LastName;

            if (!String.IsNullOrEmpty(firstname) && !String.IsNullOrEmpty(lastname) && !String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password) && model.CreateBank)
            {
                BankingConsole.Account account;
                if ((account = BankingConsole.AccountManager.createAccount(username, password, firstname, lastname)) != null)
                {
                    Session["Username"] =  username;
                    Session["AccountNumber"] = account.AccountNumber;

                    TempData.Clear();

                    return RedirectToAction("Balance", "Home");
                }
                ModelState.AddModelError("", "Error creating bank account.");
                return View(model);
            }

            if (!String.IsNullOrEmpty(firstname) || !String.IsNullOrEmpty(lastname) || (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password)))
            {
                TempData["Username"] = username;
                TempData["Password"] = password;
                BankingWebApp.Models.BankAccountModels.CreateBankViewModel newModel = new Models.BankAccountModels.CreateBankViewModel("", "", username, password);

                if (!String.IsNullOrEmpty(firstname))
                {
                    TempData["FirstName"] = firstname;
                    newModel.FirstName = firstname;
                }
                if (!String.IsNullOrEmpty(lastname))
                {
                    TempData["LastName"] = lastname;
                    newModel.LastName = lastname;
                }

                return View(newModel);
            }
            return View(model);
        }
    }
}