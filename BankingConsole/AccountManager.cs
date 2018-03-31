using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace BankingConsole
{
    public class AccountManager
    {
        private static long accountNumberSeed = 314159;
        private static Dictionary<long, Account> _accountListByAccountNumber = new Dictionary<long, Account>();
        private static Dictionary<string, Account> _accountListByLogin = new Dictionary<string, Account>();
        

        public Dictionary<long, Account> AccountListByAccountNumber
        {
            get { return _accountListByAccountNumber; }
        }

        public static Dictionary<string, Account> AccountListByLogin
        {
            get { return _accountListByLogin; }
        }

        public static Account GetAccount(long accountNumber)
        {
            if (_accountListByAccountNumber.ContainsKey(accountNumber))
            {
                return _accountListByAccountNumber[accountNumber];
            }

            Dictionary<long, string> tempDict = AccountManager.AccountNumberUsernameDictionary();
            if (tempDict.ContainsKey(accountNumber))
            {
                return GetAccount(tempDict[accountNumber], accountNumber);
            }
            return null;
        }

        public static Account GetAccount(string username, long accountNumber)
        {
            return ReadAccountFromFile(username, accountNumber);
        }

        public static bool AccountExists(long accountNumber)
        {
            return AccountManager.AccountNumberUsernameDictionary().ContainsKey(accountNumber);
        }

        public static bool AccountExists(Account account)
        {
            return AccountManager.UsernameAccountNumberDictionary().ContainsKey(account.UserName);
        }

        public static Account GetAccount(string username, string password)
        {   
            return GetAccountEncryptedStrings(username, EncryptionHelper.EncryptString(password));
        }

        public static Account GetAccountEncryptedStrings(string username, string password)
        {
            Account temp = null;
            string key = username + "/" + password;
            if (_accountListByLogin.ContainsKey(key))
            {
                return _accountListByLogin[key];
            }
            else
            {
                Dictionary<string, long> tempDict = UsernameAccountNumberDictionary();
                if (tempDict.Keys.Contains(username))
                {
                    temp = AccountManager.ReadAccountFromFile(username, AccountManager.UsernameAccountNumberDictionary()[username]);
                }
                    
                if (temp != null)
                {
                    AddOrUpdateAccount(temp);
                }
            }
            return temp;
        }

        public static long GetNextAccountNumber(string username)
        {
            Dictionary<string, long> tempDict = AccountManager.UsernameAccountNumberDictionary();
            if (tempDict.ContainsKey(username))
            {
                accountNumberSeed = tempDict[username];
                accountNumberSeed += 1;
            }
            return accountNumberSeed;
        }

        public static long? GetAccountNumber(string username)
        {
            if (AccountManager.UsernameAccountNumberDictionary().ContainsKey(username))
            {
                return AccountManager.UsernameAccountNumberDictionary()[username];
            }
            return null;
        }

        public static string GetUsername(long accountNumber)
        {
            Dictionary<long, string> tempDict = AccountManager.AccountNumberUsernameDictionary();
            if (tempDict.ContainsKey(accountNumber))
            {
                return tempDict[accountNumber];
            }
            return null;
        }

        public static Dictionary<string, long> UsernameAccountNumberDictionary()
        {
            Dictionary<string, long> usernameAccountNumberDictionary = new Dictionary<string, long>();
            string filename = null;
            string[] filenameArr = null;
            foreach(string s in Directory.GetFiles(ConfigurationManager.AppSettings["localCacheDirectory"]))
            {
                filename = Path.GetFileName(s);
                filenameArr = filename.Split("-".ToCharArray());
                string username = filenameArr[0];
                long accountNumber = Convert.ToInt64((filenameArr[1].Split(".".ToCharArray()))[0]);
                usernameAccountNumberDictionary.Add(username,accountNumber);
            }
            return usernameAccountNumberDictionary;
        }

        public static Dictionary<long, string> AccountNumberUsernameDictionary()
        {
            Dictionary<long, string> accountNumberUsernameDictionary = new Dictionary<long,string>();
            string filename = null;
            string[] filenameArr = null;
            foreach (string s in Directory.GetFiles(ConfigurationManager.AppSettings["localCacheDirectory"]))
            {
                filename = Path.GetFileName(s);
                filenameArr = filename.Split("-".ToCharArray());
                long accountNumber = Convert.ToInt64((filenameArr[1].Split(".".ToCharArray()))[0]);
                string username = filenameArr[0];
                while (!accountNumberUsernameDictionary.ContainsKey(accountNumber))
                {
                    if (accountNumberUsernameDictionary.ContainsKey(accountNumber))
                    {
                        break;
                    }
                    accountNumber++;
                }                    
                accountNumberUsernameDictionary.Add(accountNumber, username);
            }
            return accountNumberUsernameDictionary;
        }
        
        public static Account Login(string username, string password)
        {
            string hashedPassword = EncryptionHelper.EncryptString(password);
            Account acct = GetAccount(username, hashedPassword);
            String cacheKey = username +"/" + hashedPassword;
            
            if (acct != null)
            {
                cacheKey = acct.UserName + "/" + acct.Password;
                Cache.Set(cacheKey, acct, 30);
                return acct;
            }
            return acct;        
        }

        public static void AddOrUpdateAccount(Account account)
        {
            string cacheKeyLogin = account.UserName + "/" + account.Password;

            if (_accountListByAccountNumber.ContainsKey(account.AccountNumber))
                _accountListByAccountNumber[account.AccountNumber] = account;
            else
                _accountListByAccountNumber.Add(account.AccountNumber, account);

            if (_accountListByLogin.ContainsKey(cacheKeyLogin))
                _accountListByLogin[cacheKeyLogin] = account;
            else            
                _accountListByLogin.Add(cacheKeyLogin, account);
                        
        }

        public static Account createAccount(Dictionary<string, string> args)
        {
            string username, password, firstname, lastname;
            username = args["username"];
            password = args["password"];
            firstname = args["firstname"];
            lastname = args["lastname"];

            return createAccount(username, password, firstname, lastname);
        }

        public static Account createAccount(string username, string password, string firstname, string lastname)
        {
            Account acct = new Account(username, password, firstname, lastname, AccountManager.GetNextAccountNumber(username));
            string directoryName = ConfigurationManager.AppSettings["localCacheDirectory"];
            acct.Save();
            return acct;
        }

        public static Account Login(Dictionary<string, string> args)
        {
            return Login(args["username"], args["password"]);            
        }

        public static void Logout(Dictionary<string, string> args)
        {
            Logout(Convert.ToInt64(args["accountNumber"]));
        }

        public static void Logout(long accountNumber)
        {
            Account account;
            
            string cacheKey;
            if (AccountExists(accountNumber))
            {
                account = GetAccount(accountNumber);
                cacheKey = account.UserName + "/" + account.Password;
                if (Cache.Exists(cacheKey))
                {
                    account.Save();
                    Cache.Remove(cacheKey);
                    return;
                }
                else
                {
                    Console.WriteLine("Account is not logged in.");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Account does not exist.");
                return;
            }
        }
        
        public static double? Deposit(Dictionary<string, string> args)
        {
            Account account;
            Int64 accountNumber = Convert.ToInt64(args["accountNumber"]);
            double amount = Convert.ToDouble(args["amount"]);
            string cacheKey;
            if (AccountExists(accountNumber))
            {
                account = AccountManager.GetAccount(accountNumber);
                cacheKey = account.UserName + "/" + account.Password;
                if (Cache.Exists(cacheKey))
                {
                    if (amount <= 0)
                    {
                        Console.WriteLine("Insufficient funds - no deposit made.");
                        return null;
                    }
                    return Deposit(account, amount);
                }
                else
                {
                    Console.WriteLine("Account is not logged in.");
                }
            }
            else
            {
                Console.WriteLine("Account does not exist.");
            }
            return null;
        }

        private static double Deposit(Account account, double amount)
        {
            string cacheKey = account.UserName + "/" + account.Password;
            double balance = account.Deposit(amount);
            Cache.Set(cacheKey, account, 30);
            return balance;            
        }

        public static double? Withdrawal(Dictionary<string, string> args)
        {
            Account account;
            Int64 accountNumber = Convert.ToInt64(args["accountNumber"]);
            double amount = Convert.ToDouble(args["amount"]);
            string cacheKey;
            if (AccountExists(accountNumber))
            {
                account = AccountManager.GetAccount(accountNumber);
                cacheKey = account.UserName + "/" + account.Password;
                if (Cache.Exists(cacheKey))
                {
                    if ((account.Balance - amount) <= 0)
                    {
                        Console.WriteLine("Insufficient funds - no withdrawal made.");
                        return null;
                    }
                    return Withdrawal(account, amount);
                }
                else
                {
                    Console.WriteLine("Account is not logged in.");
                }
            }
            else
            {
                Console.WriteLine("Account does not exist.");
            }
            return null;
        }

        public static double Withdrawal(Account account, double amount)
        {
            string cacheKey = account.UserName + "/" + account.Password;
            double balance = account.Withdrawal(amount);
            Cache.Set(cacheKey, account, 30);
            return balance;
        }

        public static double? Balance(Dictionary<string, string> args)
        {
            Account account;
            long accountNumber = Convert.ToInt64(args["accountNumber"]);
            string cacheKey;
            if (AccountManager.AccountExists(accountNumber))
            {
                account = AccountManager.GetAccount(accountNumber);
                cacheKey = account.UserName + "/" + account.Password;
                if (Cache.Exists(cacheKey))
                {
                    Cache.Set(cacheKey, account, 30);
                    return account.Balance;
                }
                else
                {
                    Console.WriteLine("Account is not logged in.");
                }
            }
            else
            {
                Console.WriteLine("Account does not exist.");
            }
            return null;
        }

        public static void SaveAccountToFile(Account account, string filename)
        {
            string directoryName = ConfigurationManager.AppSettings["localCacheDirectory"];
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }            

            using (StreamWriter writer = File.CreateText(filename))
            {
                writer.WriteLine("{0}|{1}|{2}|{3}|{4}|{5}", account.AccountNumber.ToString(), account.LastName, account.FirstName, account.UserName, account.Password, account.Balance.ToString());
                if (account.TransactionHistory != null)
                {
                    foreach (BankTransaction tran in account.TransactionHistory)
                    {
                        writer.WriteLine("{0}|{1}|{2}|{3}", tran.TransactionType, tran.Amount, tran.PreviousBalance, tran.NewBalance);
                    }
                }                
            }
        }

        
        public static Account ReadAccountFromFile(string username, long accountNumber)
        {
            Account account = null;
            string filename = ConfigurationManager.AppSettings["localCacheDirectory"] + username + "-" + accountNumber.ToString() + ".txt";
            if (File.Exists(filename))
            {
                string[] lineArray;
                string line;

                using (StreamReader sr = new StreamReader(filename))
                {
                    lineArray = sr.ReadLine().Split("|".ToCharArray());
                    account = new Account(lineArray[3], lineArray[4], lineArray[2], lineArray[1], Convert.ToInt64(lineArray[0]));
                    double currentBalance = Convert.ToDouble(lineArray[5]);
                    List<BankTransaction> btList = new List<BankTransaction>();
                    BankTransaction bt = null;
                    while ((line = sr.ReadLine()) != null)
                    {
                        lineArray = line.Split("|".ToCharArray());
                        bt = new BankTransaction(Convert.ToInt32(lineArray[0]) == 0 ? BankTransaction.BankTransactionTypeEnum.Deposit : BankTransaction.BankTransactionTypeEnum.Withdrawal, Convert.ToDouble(lineArray[1]), Convert.ToDouble(lineArray[2]));
                        btList.Add(bt);
                    }
                    account.Balance = currentBalance;
                    account.TransactionHistory = btList;
                }
            }
            AddOrUpdateAccount(account);
            return account;
        }
    }
}
