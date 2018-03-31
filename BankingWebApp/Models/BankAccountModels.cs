using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BankingConsole;
using System.ComponentModel.DataAnnotations;

namespace BankingWebApp.Models
{
    public class BankAccountModels
    {
        public class BalanceViewModel
        {
            public Account Account { get; set; }

            [Display(Name = "Balance: ")]
            public String Balance
            {
                get { return this.Account == null ? "0.00" : this.Account.Balance.ToString("C"); }                
            }
        }


        public class DepositViewModel
        {
            public Account Account { get; set; }

            [Required]
            [Range(0.01, Double.MaxValue,
            ErrorMessage = "Deposit amount must be greater than $0.00.")]
            [Display(Name = "Deposit Amount: $")]
            public double DepositAmount { get; set; }
        }

        public class WithdrawViewModel
        {
            public Account Account { get; set; }

            [Required]
            [Range(0.01, 1000000,
            ErrorMessage = "Withdrawal amount cannot be greater than $1 million.")]
            [Display(Name = "Withdraw Amount: $")]
            public double WithdrawAmount { get; set; }
        }

        public class TransactionsViewModel
        {
            public TransactionsViewModel(string transactionType, double amount, double previousBalance, double newBalance)
            {
                TransactionType = transactionType;
                TransactionAmount = amount.ToString("C");
                PreviousBalance = previousBalance.ToString("C");
                NewBalance = newBalance.ToString("C");
            }
           
            [Display(Name = "Transaction Type")]
            public string TransactionType { get; set; }
            [Display(Name = "Transaction Amount")]
            public string TransactionAmount { get; set; }
            [Display(Name = "Previous Balance")]
            public string PreviousBalance { get; set; }
            [Display(Name = "New Balance")]
            public string NewBalance { get; set; }

        }

        public class CreateBankViewModel
        {
            public CreateBankViewModel() { }

            public CreateBankViewModel(string firstname, string lastname, string username, string password)
            {
                FirstName = firstname;
                LastName = lastname;
                UserName = username;
                Password = password;
                CreateBank = true;
            }
            
            [Display(Name = "No bank account was found, would you like to create a bank now?")]
            public bool CreateBank { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
            [Display(Name = "User Name")]
            public string UserName { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }
        }
    }
}