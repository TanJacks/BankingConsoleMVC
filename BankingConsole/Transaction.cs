using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingConsole
{
    public class BankTransaction
    {
        public enum BankTransactionTypeEnum { Deposit, Withdrawal };
        private int _transactionType;
        private double _previousBalance;
        private double _newBalance;
        private double _amount;
        
        public double Amount
        {
            get { return _amount; }
            set { this._amount = value; }
        }

        public double PreviousBalance
        {
            get { return _previousBalance; }
            set { this.PreviousBalance = value; }
        }

        public double NewBalance
        {
            get { return _newBalance; }
            set { this._newBalance = value; }
        }

        public int TransactionType
        {
            get { return _transactionType; }
            set { this._transactionType = value; }
        }
        public BankTransaction(BankTransactionTypeEnum tranType, double amount, double balance)
        {
            this._transactionType = (int)tranType;
            this._amount = amount;
            this._previousBalance = balance;
            if (tranType == BankTransactionTypeEnum.Withdrawal)
            {
                this._newBalance = balance - amount;
            }
            else if(tranType == BankTransactionTypeEnum.Deposit)
            {
                this._newBalance = balance + amount;
            }
            else
            {
                throw new Exception("Invalid Bank Transaction Type");
            }            
        }
    }
}
