using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Threading;
using System.Threading.Tasks;

namespace Synchronization
{
    class Program
    {
        static readonly object firstLock = new object();
        static readonly object secondLock = new object();

        static void Main(string[] args)
        {
            var bankcard = new BankCard2(10000); //10000+500+300-500 = 10300
            var bankcard2 = new BankCard2(200); //200+300+500 = 1000


            var t1 = Task.Factory.StartNew(() => { bankcard.ReceivePayment(500); });
            var t2 = Task.Factory.StartNew(() => {
                bankcard.TransferToCard(500, bankcard2);
                bankcard.ReceivePayment(300);
                bankcard2.ReceivePayment(300);
            });

            Task.WaitAll(new[] { t1, t2 });


            Console.WriteLine($"Card 1 amount is {bankcard.MoneyAmount}, Card 2 amount is {bankcard2.MoneyAmount}");


            /////////////////////////////////////////////////////////////////
            //Character c = new Character() { Name="hamada"};
            //Character c2 = new Character() { Name="mayada"};

            //Console.WriteLine($"c name is {c.Name} and c2 name is {c2.Name}");

            //var cref = Interlocked.Exchange(ref c, c2);
            //Interlocked.Exchange(ref c2, cref);

            //Console.WriteLine($"c name is {c.Name} and c2 name is {c2.Name}");


            //Console.WriteLine($"characheter health is {c.Health}");

            //var tasks = new List<Task>();

            //for (int i = 0; i < 100; i++)
            //{
            //    Task t1 = Task.Factory.StartNew(() => {
            //        for (int j = 0; j < 100; j++)
            //        {
            //            c.Hit(10);
            //        }
            //    });

            //    tasks.Add(t1);

            //    Task t2 = Task.Factory.StartNew(() => {
            //        for (int j = 0; j < 10; j++)
            //        {
            //            c.Heal(10);
            //        }
            //    });

            //    tasks.Add(t2);
            //}

            //Task.WaitAll(tasks.ToArray());

            //Console.WriteLine($"characheter health is {c.Armor}");



            ///////////////////////////////////////////////////////////////////////////////
            //Task.Run((Action)Do);

            //// Wait until we're fairly sure the other thread // has grabbed firstLock
            //Thread.Sleep(500);
            //Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}-Locking secondLock");

            //lock (secondLock)
            //{
            //    Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}-Locked secondLock");
            //    Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}-Locking firstLock");

            //    lock (firstLock)
            //    {
            //        Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}-Locked firstLock");
            //    }
            //    Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}-Released firstLock");
            //}
            //Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}-Released secondLock");

            Console.Read();
        }

        private static void Do()
        {
            Console.WriteLine($"\t\t\t\t{Thread.CurrentThread.ManagedThreadId}-Locking firstLock");
            lock (firstLock)
            {
                Console.WriteLine($"\t\t\t\t{Thread.CurrentThread.ManagedThreadId}-Locked firstLock");

                // Wait until we're fairly sure the first thread // has grabbed secondLock
                Thread.Sleep(1000);

                Console.WriteLine($"\t\t\t\t{Thread.CurrentThread.ManagedThreadId}-Locking secondLock");
                lock (secondLock)
                {
                    Console.WriteLine($"\t\t\t\t{Thread.CurrentThread.ManagedThreadId}-Locked secondLock");
                }
                Console.WriteLine($"\t\t\t\t{Thread.CurrentThread.ManagedThreadId}-Released secondLock");
            }
            Console.WriteLine($"\t\t\t\t{Thread.CurrentThread.ManagedThreadId}-Released firstLock");
        }
    


        private static void Swap(object obj1, object obj2)
        {
            object obj1Ref = Interlocked.Exchange(ref obj1, obj2);
            Interlocked.Exchange(ref obj2, obj1Ref);
            //object tmp = obj1;
            //obj1 = obj2;
            //obj2 = tmp;
        }

        private static void TestCharacter()
        {
            Character c = new Character();
            Character c2 = new Character();

            Swap(c, c2);

            var tasks = new List<Task>();

            for (int i = 0; i < 100; i++)
            {
                Task t1 = Task.Factory.StartNew(() =>
                {
                    for (int j = 0; j < 10; j++)
                    {
                        c.CastArmorSpell(true);
                    }
                });
                tasks.Add(t1);

                Task t2 = Task.Factory.StartNew(() =>
                {
                    for (int j = 0; j < 10; j++)
                    {
                        c.CastArmorSpell(false);
                    }
                });
                tasks.Add(t2);
            }

            Task.WaitAll(tasks.ToArray());

            Console.WriteLine($"Resulting armor = {c.Armor}");
        }
    }
}