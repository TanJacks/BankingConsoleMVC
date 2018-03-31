using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Configuration;
using System.Threading.Tasks;
using System.Runtime.Caching;

namespace BankingConsole
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Dictionary<string, List<string>> validCommandsWithParameters = getCommandListWithParameters();
                
                // Test if input arguments were supplied:
                if (!verifyArgs(args, validCommandsWithParameters)) 
                {
                    Main(promptForCommand());
                }
                else
                {
                    Dictionary<string, string> commandParameters = parseCommandParameters(args);
                    Account account = null;
                    double? accountBalance;
                    switch (commandParameters["command"])
                    {
                        case "-CreateAccount":
                            account = AccountManager.createAccount(commandParameters);
                            AccountManager.AddOrUpdateAccount(account);
                            Console.WriteLine("Successfully created account number: {0}", account.AccountNumber);
                            break;
                        case "-Login":
                            account = AccountManager.Login(commandParameters);
                            if (account == null)
                            {
                                Console.WriteLine("Login failed.");
                            }
                            Console.WriteLine("Successfully logged in account number: {0}", account.AccountNumber);
                            break;
                        case "-Deposit":
                            accountBalance = AccountManager.Deposit(commandParameters);
                            if (accountBalance != null)
                            {
                                Console.WriteLine("Deposit: ${0}\tNew Balance: ${1}", commandParameters["amount"], Convert.ToString(accountBalance));
                            }
                            break;
                        case "-Withdrawal":
                            accountBalance = AccountManager.Withdrawal(commandParameters);
                            if (accountBalance != null)
                            {
                                Console.WriteLine("Withdrawal: ${0}\tNew Balance: ${1}", commandParameters["amount"], Convert.ToString(accountBalance));
                            }                            
                            break;
                        case "-Balance":
                            accountBalance = AccountManager.Balance(commandParameters);
                            if(accountBalance != null)
                            {
                                Console.WriteLine("Balance: ${0}", Convert.ToString(accountBalance));
                            }
                            break;
                        case "-Transactions":
                            displayTransactionLog(commandParameters);
                            break;
                        case "-Logout":
                            AccountManager.Logout(commandParameters);
                            break;
                        case "-Help":
                            displayHelp();
                            Main(promptForCommand());
                            break;
                        case "-Exit":
                            return;
                        default:
                            Main(promptForCommand());
                            break;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: {0}", ex.Message);
                Console.WriteLine();
                Main(promptForCommand());
                return;
            }
            Main(promptForCommand());
            return;
        }

        private static string[] promptForCommand()
        {
            Console.WriteLine();
            Console.WriteLine("type '-Help' to list commands");
            Console.WriteLine("type '-Exit' to quit");
            Console.WriteLine();
            Console.WriteLine("-Enter Command-");
            return Console.ReadLine().Split(" ".ToCharArray());
        }

        public static void displayHelp()
        {
            Console.WriteLine();
            Console.WriteLine("Alphanumeric values only");
            Console.WriteLine("Example syntax: BankingConsole.exe -CreateAccount username=BobJohnsonTheFirst password=somevalue firstname=Bob lastname=Johnson");
            Console.WriteLine();
            Console.WriteLine("Please enter a command (-Help will display these commands again):");
            Console.WriteLine();
            Console.WriteLine("-CreateAccount username=<string value> password=<string value> firstname=<string value> lastname=<string value>");
            Console.WriteLine("-Login username=<string value> password=<string value>");
            Console.WriteLine("-Deposit accountNumber=<sting value> amount=<decimal dollar amount to deposit>");
            Console.WriteLine("-Withdrawal accountNumber=<sting value> amount=<decimal dollar amount to deposit>");
            Console.WriteLine("-Balance accountNumber=<sting value>");
            Console.WriteLine("-Transactions accountNumber=<sting value>");
            Console.WriteLine("-Logout accountNumber=<sting value>");
            Console.WriteLine("-Help");
            Console.WriteLine();
            Console.WriteLine();
        }

        private static void displayTransactionLog(Dictionary<string, string> args)
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
                    displayTransactions(account);
                    return;
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
            return;
        }

        private static void displayTransactions(Account account)
        {
            Console.WriteLine("====================");
            Console.WriteLine("number-type-previous-amount-balance");
            foreach (BankTransaction tran in account.TransactionHistory)
            {
                Console.WriteLine("number: {0}-type: {1}-previous balance: ${2}-amount: ${3}-new balance: ${4}", account.TransactionHistory.IndexOf(tran).ToString(), ((int)tran.TransactionType == 0 ? " Deposit  " : "Withdrawal"), tran.PreviousBalance.ToString(), tran.Amount.ToString(), tran.NewBalance.ToString());
            }
            Console.WriteLine("====================");
        }

        

        private static bool isAlphaNumeric(string testVal)
        {
            Regex r = new Regex("^[a-zA-Z0-9]*$");
            return r.IsMatch(testVal);
        }

        private static Dictionary<string, List<string>> getCommandListWithParameters()
        {
            
            Dictionary<string, List<string>> commandListWithParameters = new Dictionary<string, List<string>>();
            List<string> commands = getCommandList();
            foreach (string command in commands)
            {
                commandListWithParameters.Add(command, new List<string>());
                if (String.Equals(command, "-CreateAccount"))
                {
                    commandListWithParameters[command].Add("firstname");
                    commandListWithParameters[command].Add("lastname");
                }
                if (String.Equals(command, "-Login") || String.Equals(command, "-CreateAccount"))
                {
                    commandListWithParameters[command].Add("username");
                    commandListWithParameters[command].Add("password");
                }
                if (String.Equals(command, "-Deposit") || String.Equals(command, "-Withdrawal"))
                {
                    commandListWithParameters[command].Add("amount");
                }
                if (String.Equals(command, "-Deposit") || String.Equals(command, "-Withdrawal") || String.Equals(command, "-Balance") || String.Equals(command, "-Transactions") || String.Equals(command, "-Logout"))
                {
                    commandListWithParameters[command].Add("accountNumber");
                }
            }
            return commandListWithParameters;
        }

        
        private static bool verifyCommandParameter(string command, string commandParameter, Dictionary<string, List<string>> commandListWithParameters)
        {
            if(!commandParameter.Contains("="))
            {
                return false;
            }
                        
            string[] temp = commandParameter.Split("=".ToCharArray());

            if(!commandListWithParameters.Keys.Contains(command))
            {
                return false;
            }
            else
            {
                if(!commandListWithParameters[command].Contains(temp[0]))
                {
                    return false;
                }
                else
                {
                    double tempDouble;
                    if(temp[0] == "amount")
                    {
                        if (!Double.TryParse(temp[1], out tempDouble))
                        {
                            return false;
                        }   
                    }
                    else if ((temp[0] == "firstname" || temp[0] == "lastname") && (String.IsNullOrWhiteSpace(temp[1]) || !isAlphaNumeric(temp[1])))
                    {
                        return false;
                    }                    
                }
            }
            return true;
        }

        private static List<string> getCommandList()
        {
            return new List<string>() { "-CreateAccount"
                    ,"-Login"
                    ,"-Deposit"
                    ,"-Withdrawal"
                    ,"-Balance"
                    ,"-Transactions"
                    ,"-Logout"
                    ,"-Help"
                    ,"-Exit" };
        }

        private static bool verifyArgs(string[] args, Dictionary<string, List<string>> validCommandDictionary)
        {
            if (args == null || args.Length == 0)
            { return false; }
            if (!args[0].StartsWith("-"))
            { return false; }
            List<string> validCommandList = getCommandList();
            string command = args[0];
            if (!validCommandList.Contains(command))
            { 
                return false;
            }            
            for(int x=1; x < args.Count(); x++)
            {
                if(!verifyCommandParameter(command, args[x], validCommandDictionary))
                { return false; }
            }
            return true;
        }

        private static Dictionary<string, string> parseCommandParameters(string[] args)
        {
            String[] tempArray;
            Dictionary<string, string> parsedParameters = new Dictionary<string, string>();
            parsedParameters.Add("command", args[0]);
            for (int x=1; x < args.Count(); x++)
            {
                tempArray = args[x].Split("=".ToCharArray());
                parsedParameters.Add(tempArray[0], tempArray[1]);
            }
            return parsedParameters;
        }
    }
}
