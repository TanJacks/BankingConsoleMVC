using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Configuration;

namespace BankingConsole
{
    public class Account
    {
        public Account()
        {
        }

        public Account(String username, String password, String firstname, String lastname, long accountNumber)
        {
            UserName = username;
            Password = EncryptionHelper.EncryptString(password);
            FirstName = firstname;
            LastName = lastname;
            AccountNumber = accountNumber;
            Balance = 0.0;
            TransactionHistory = new List<BankTransaction>();
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long AccountNumber { get; }

        //stores Md5 hashvalue
        public string UserName { get; }
        public string Password { get; }

        public double Balance { get; set; }
        public List<BankTransaction> TransactionHistory { get; set; }
        
        public double Withdrawal(double amount)
        {
            BankTransaction tran = new BankTransaction(BankTransaction.BankTransactionTypeEnum.Withdrawal, amount, Balance);
            TransactionHistory.Add(tran);
            Balance = tran.NewBalance;
            Save();
            return tran.NewBalance;
        }

        public double Deposit(double amount)
        {
            BankTransaction tran = new BankTransaction(BankTransaction.BankTransactionTypeEnum.Deposit, amount, Balance);
            TransactionHistory.Add(tran);
            Balance = tran.NewBalance;
            Save();
            return tran.NewBalance;
        }
        
        public void Save()
        {
            string directory = System.Configuration.ConfigurationManager.AppSettings["localCacheDirectory"];
            AccountManager.SaveAccountToFile(this, directory + this.UserName + "-" + this.AccountNumber + ".txt");
        }
    }
}
