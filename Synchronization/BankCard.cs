﻿using System;
using System.Threading;

namespace Synchronization
{
    public class BankCard
    {
        private decimal _moneyAmount;
        private readonly object _sync = new object();
        private readonly decimal _credit;

        public decimal TotalMoneyAmount
        {
            get
            {
                using (ReaderWriterLockSlimExt.TakeReaderLock(TimeSpan.FromMilliseconds(3)))
                {
                    return _moneyAmount + _credit;
                }                   
            }
        }

        public BankCard(decimal moneyAmount, decimal credit)
        {
            _moneyAmount = moneyAmount;
            _credit = credit;
        }

        public void ReceivePayment(decimal amount)
        {
            //using (ReaderWriterLockSlimExt.TakeWriterLock(TimeSpan.FromMilliseconds(3)))
            //{
            //_moneyAmount += amount;
            //}
            
            var rw = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            rw.EnterWriteLock();

            _moneyAmount += amount;
            
            rw.ExitWriteLock();
            
        }

        public void TransferToCard(decimal amount, BankCard recipient)
        {
            lock (_sync)
            {
                _moneyAmount -= amount;
                recipient.ReceivePayment(amount);
            }
        }

        private void WaysOfUsingMonitor()
        {
            //Monitor with lock keyword
            //lock (_sync)
            //{
            //_moneyAmount -= amount;
            //recipient.ReceivePayment(amount);
            //}
            //Monitor with exntesion
            //using (_sync.Lock(TimeSpan.FromSeconds(3)))
            //{
            //    _moneyAmount -= amount;
            //    recipient.ReceivePayment(amount);
            //}
            /*Full-blown pattern with Monitor
            bool lockTaken = false;
            try
            {
                Monitor.Enter(_sync, ref lockTaken);
                _moneyAmount -= amount;
                recipient.ReceivePayment(amount);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(_sync);
            }*/
        }
    }

    public class BankCard2
    {
        private decimal _moneyAmount;
        private readonly object _sync = new object();

        public decimal MoneyAmount {
            get {
                return _moneyAmount;
            }
        }

        public BankCard2(decimal moneyAmount)
        {
            _moneyAmount = moneyAmount;
        }

        public void ReceivePayment(decimal amount)
        {
            bool lockTaken = false;

            try
            {
                Monitor.TryEnter(_sync, TimeSpan.FromSeconds(1), ref lockTaken);
                //if (amount == 300)
                //throw new ArgumentNullException();
                VerifyCard(this);
                _moneyAmount += amount;
            }
            finally
            {
                if(lockTaken)
                    Monitor.Exit(_sync);
            }
        }

        public void TransferToCard(decimal amount, BankCard2 recipient)
        {
            bool lockTaken = false;

            try
            {
                Monitor.TryEnter(_sync, TimeSpan.FromSeconds(1), ref lockTaken);
                VerifyCard(recipient);
                _moneyAmount -= amount;
                recipient.ReceivePayment(amount);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(_sync);
            }
        }

        private bool VerifyCard(BankCard2 card)
        {
            Thread.Sleep(5000);
            return true;
        }


        private void WaysOfUsingMonitor()
        {
            //Monitor with lock keyword
            //lock (_sync)
            //{
            //_moneyAmount -= amount;
            //recipient.ReceivePayment(amount);
            //}
            //Monitor with exntesion
            //using (_sync.Lock(TimeSpan.FromSeconds(3)))
            //{
            //    _moneyAmount -= amount;
            //    recipient.ReceivePayment(amount);
            //}
            /*Full-blown pattern with Monitor
            bool lockTaken = false;
            try
            {
                Monitor.Enter(_sync, ref lockTaken);
                _moneyAmount -= amount;
                recipient.ReceivePayment(amount);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(_sync);
            }*/
        }
    }
}