using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AsyncCSharp_Course
{
    class Program
    {
        static void WhenAny()
        {
            var cts = new CancellationTokenSource();

            var t1 = Task.Run<int>(() => Print(true, cts.Token), cts.Token);
            var t2 = Task.Run<int>(() => Print(false, cts.Token), cts.Token);

            Console.WriteLine("Started t1");
            Console.WriteLine("Started t2");

            var tr = Task.WhenAny(t1, t2);
            tr.ContinueWith(x => { Console.WriteLine($"The id of a task which completed first = {tr.Result.Id}"); });

            Console.WriteLine("After when any");
        }
        static void WaitAny()
        {
            var cts = new CancellationTokenSource();

            var t1 = Task.Run<int>(() => Print(true, cts.Token), cts.Token);
            var t2 = Task.Run<int>(() => Print(false, cts.Token), cts.Token);

            Console.WriteLine("Started t1");
            Console.WriteLine("Started t2");

            int result = Task.WaitAny(t1, t2);

            Console.WriteLine($"After wait any. First finished task id={result}");
        }
        static void Wait()
        {
            var cts = new CancellationTokenSource();

            var t1 = Task.Run<int>(() => Print(true, cts.Token), cts.Token);

            Console.WriteLine("Started t1");

            t1.Wait();

            Console.WriteLine("After wait");
        }
        static void ContinueWhenAll()
        {
            var cts = new CancellationTokenSource();

            var t1 = Task.Run<int>(() => Print(true, cts.Token), cts.Token);
            var t2 = Task.Run<int>(() => Print(false, cts.Token), cts.Token);

            Task.Factory.ContinueWhenAll(new[] { t1, t2 }, tasks =>
            {
                var t1Task = tasks[0];
                var t2Task = tasks[1];

                Console.WriteLine($"t1Task:{t1Task.Result}, t2Task:{t2Task.Result}");
            });
        }
        static void ContinueWith()
        {
            var cts = new CancellationTokenSource();

            var t1 = Task.Run<int>(() => Print(true, cts.Token), cts.Token);

            Task t2 = t1.ContinueWith(prevTask =>
            {
                Console.WriteLine($"How many numbers were processed by prev. task={prevTask.Result}");
                Task.Run<int>(() => Print(false, cts.Token), cts.Token);
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
            t1.ContinueWith(t =>
            {
                Console.WriteLine("Finally, we are here!");
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
        private static void Delay()
        {
            var t1 = Task.Run(() => Print(true, CancellationToken.None));
            Task t2 = null;

            Console.WriteLine("Started t1");

            Task.Delay(5000).ContinueWith(x =>
            {
                t2 = Task.Run(() => Print(false, CancellationToken.None));
                Console.WriteLine("Started t2");
            });
        }

        private static void TestAggregateException()
        {
            var parent = Task.Factory.StartNew(() =>
            {
                // We'll throw 3 exceptions at once using 3 child tasks: 
                int[] numbers = { 0 };
                var childFactory = new TaskFactory(TaskCreationOptions.AttachedToParent,
                    TaskContinuationOptions.None);
                childFactory.StartNew(() => 5 / numbers[0]); // Division by zero
                childFactory.StartNew(() => numbers[1]); // Index out of range
                childFactory.StartNew(() => { throw null; }); // Null reference
            });
            try
            {
                parent.Wait();
            }
            catch (AggregateException aex)
            {
                aex.Flatten().Handle(ex =>
                {
                    if (ex is DivideByZeroException)
                    {
                        Console.WriteLine("Divide by zero");
                        return true;
                    }

                    if (ex is IndexOutOfRangeException)
                    {
                        Console.WriteLine("Index out of range");
                        return true;
                    }

                    return false;
                });
            }
        }

        public Task ImportXmlFilesAsync(string dataDirectory, CancellationToken ct)
        {
            return Task.Factory.StartNew(() =>
            {
                foreach (FileInfo file in new DirectoryInfo(dataDirectory).GetFiles("*.xml"))
                {
                    XElement doc = XElement.Load(file.FullName);
                    InternalProcessXml(doc, CancellationToken.None);
                }
            }, ct);
        }

        public Task ImportXmlFilesAsync2(string dataDirectory, CancellationToken ct)
        {
            return Task.Factory.StartNew(() =>
            {
                foreach (FileInfo file in new DirectoryInfo(dataDirectory).GetFiles("*.xml"))
                {
                    string fileToProcess = file.FullName;
                    Task.Factory.StartNew(_ =>
                    {
                        ct.ThrowIfCancellationRequested();

                        XElement doc = XElement.Load(fileToProcess);
                        InternalProcessXml(doc, ct);
                    }, ct, TaskCreationOptions.AttachedToParent);
                }
            }, ct);
        }


        private void InternalProcessXml(XElement doc, CancellationToken ct)
        {

        }

        private void DumpWebPage(string uri)
        {
            WebClient wc = new WebClient();
            string page = wc.DownloadString(uri);
            Console.WriteLine(page);
        }

        private async void DumpWebPageAsync(string uri)
        {
            WebClient wc = new WebClient();
            string page = await wc.DownloadStringTaskAsync(uri);
            //Task<string> DownloadStringTaskAsync(string address)
            Console.WriteLine(page);
        }

        private void DumpWebPageTaskBased(string uri)
        {
            WebClient webClient = new WebClient();
            Task<string> task = webClient.DownloadStringTaskAsync(uri);
            task.ContinueWith(t => { Console.WriteLine(t.Result); });
        }

        public async void Test()
        {
            Task operation1 = Operation1();
            Task operation2 = Operation2();
            await operation1;
            await operation2;

        }

        private Task Operation()
        {
            return Task.Delay(TimeSpan.FromMilliseconds(500));
        }

        private Task Operation1()
        {
            return Task.Delay(TimeSpan.FromMilliseconds(500));
        }

        private Task Operation2()
        {
            return Task.Delay(TimeSpan.FromMilliseconds(1000));
        }


        static void Main(string[] args)
        {
            //CatchMultipleExceptionsWithAwait();

            //Process.Start("notepad.exe","c:\\helloworld.txt");

            //var app = new Process
            //{
            //    StartInfo = { FileName = "notepad.exe", Arguments = "c:\\helloworld.txt" }
            //};
            //app.Start();
            //app.PriorityClass = ProcessPriorityClass.High;

            //var processes = Process.GetProcesses();
            //foreach (var process in processes)
            //{
            //    if (process.ProcessName.ToLower() == "notepad")
            //    {
            //        Console.WriteLine(process.ProcessName + " " + process.Id);
            //        process.Kill();
            //    }
            //}

            //Task<int> t1 = Task.Factory.StartNew(() => Print(true),CancellationToken.None,TaskCreationOptions.LongRunning,TaskScheduler.Default);

            //var parentcts = new CancellationTokenSource();
            //var childcts = CancellationTokenSource.CreateLinkedTokenSource(parentcts.Token);


            //Task<int> t1 = Task.Factory.StartNew(() => Print(true, parentcts.Token), parentcts.Token);

            //Task<int> t2 = Task.Factory.StartNew(() => Print(false, childcts.Token), childcts.Token);

            //Task.Factory.ContinueWhenAll(new[] { t1, t2 }, tasks => {

            //    var t1task = tasks[0];
            //    var t2task = tasks[1];
            //    Console.WriteLine($"result  Number: {t1task.Result}, {t2task.Result}");

            //});
            //Task.Factory.ContinueWhenAny(new[] { t1, t2 }, task =>
            //{

            //    var t1task = task;
            //    //var t2task = tasks[1];
            //    Console.WriteLine($"result  Number: {t1task.Result}");

            //});


            //Task t2 = t1.ContinueWith(prevTask => {
            //    Console.WriteLine($"Processed Number: {prevTask.Result}");
            //    Task.Factory.StartNew(() => Print(false, childcts.Token), childcts.Token);
            //},TaskContinuationOptions.OnlyOnRanToCompletion);


            //t2.ContinueWith(prevTask => {
            //    Console.WriteLine("Both Tasks Done");
            //}, TaskContinuationOptions.OnlyOnRanToCompletion);




            //parentcts.CancelAfter(10);





            //Console.WriteLine("task started");
            //Thread t1 = new Thread(() => Print(false))
            //{
            //    Name = "t1 thread"
            //};

            //Thread t2 = new Thread(() => Print(true))
            //{
            //    Name = "t2 thread"
            //};


            //t1.Start();
            ////t2.Start();

            //t1.Join();

            //if (t1.Join(TimeSpan.FromMilliseconds(2)))
            //    Console.WriteLine("aaa");
            //else
            //    Console.WriteLine("aaa222");




            string x = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Praesent posuere odio posuere, vehicula ipsum sit amet, dignissim mi. Nullam at tempus nunc. Nulla vitae rhoncus mauris, vel pretium eros. Sed ullamcorper justo tortor, sit amet bibendum eros porta vel. Nullam mollis felis metus, id viverra tellus faucibus ac. Etiam in leo et felis scelerisque convallis sagittis nec mauris. Nullam ac accumsan orci, vitae mollis sapien. Vivamus lacinia, velit a posuere auctor, quam lectus semper sapien, et cursus est arcu vitae nunc. Fusce commodo aliquet dui, non pellentesque tortor maximus nec. Integer egestas mollis neque, sed euismod ligula malesuada et. Phasellus et lectus a elit auctor ullamcorper. Suspendisse id ligula euismod, eleifend velit ac, ornare mauris. Ut quis nisi quis erat pellentesque ultrices. Quisque nulla elit, tristique in orci vel, accumsan efficitur sapien. Nulla sed ante vitae magna vulputate blandit nec sit amet velit. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae;

Sed molestie accumsan commodo. Curabitur id arcu at leo rutrum consequat.Donec efficitur nisi magna, fermentum rhoncus lacus vehicula in. Pellentesque pharetra est in justo bibendum euismod.Phasellus eget convallis erat, vel pharetra odio. Interdum et malesuada fames ac ante ipsum primis in faucibus.Nunc bibendum augue ex, efficitur ullamcorper lectus vestibulum et. Phasellus tempus orci mauris, eget tempus felis tristique ac. Suspendisse congue, tortor et varius laoreet, eros lectus maximus turpis, at rutrum tellus sem in lorem.Aenean et urna a lorem vulputate tincidunt.Cras pellentesque elit et ligula facilisis, at hendrerit risus euismod.Vestibulum malesuada metus vel augue efficitur varius.Aliquam vestibulum, magna a vulputate ornare, elit sapien commodo nisi, nec venenatis nisl enim eu ante.

Morbi aliquet leo vel auctor hendrerit. Morbi neque justo, ultrices quis posuere non, efficitur ac mauris. Sed lacus odio, mattis eu euismod ut, porta vitae mi. Morbi in tristique ante. Duis faucibus ex eget magna ultrices, ultricies tempor eros vulputate.Suspendisse tincidunt enim at libero sollicitudin vestibulum.Aliquam ut mollis dui, a tincidunt sapien.

Aliquam tincidunt magna sed felis viverra, non consectetur dui gravida.In finibus fermentum velit et condimentum. Morbi molestie rhoncus purus quis posuere. Cras arcu libero, posuere a arcu id, fermentum efficitur odio. Etiam laoreet, tortor eget faucibus rutrum, tortor neque malesuada leo, sed vehicula odio urna ac justo.Donec a purus diam. Suspendisse potenti. Pellentesque lobortis aliquet imperdiet. Nulla facilisi. Aliquam posuere eget neque ac ultrices. Maecenas quam massa, dapibus vel gravida eu, blandit non ante. Phasellus iaculis venenatis porttitor. In mattis egestas efficitur.

Nam consectetur porttitor feugiat. Nulla molestie eros mauris, vehicula pellentesque purus placerat pharetra. Donec commodo bibendum massa eu sagittis. Suspendisse tristique leo imperdiet, finibus nunc at, mattis ante.Vivamus eu velit a nibh efficitur tristique.Quisque eget tellus porttitor, semper risus ut, tempor quam.Nam pulvinar suscipit dignissim. Mauris vel sapien vestibulum dui pharetra eleifend.

Nullam porttitor nibh ac tempor luctus. Aenean volutpat cursus aliquam. In accumsan purus vitae viverra varius. Donec justo eros, dapibus sit amet porta sit amet, dictum fringilla est. Aenean justo libero, commodo in egestas eu, ornare vel dolor. Nullam efficitur quis augue vitae blandit. Curabitur ac nisl vitae nisl commodo pretium non vitae nibh.

Nunc hendrerit lorem leo, a sodales massa varius in. Sed metus tellus, vulputate in mi id, viverra imperdiet lorem. Quisque commodo augue odio, nec ornare magna blandit sed. Quisque in consequat mi, vitae iaculis ipsum. Curabitur ac tortor vel ligula faucibus rutrum quis quis leo. Morbi rhoncus justo odio, nec semper odio auctor a. Integer faucibus quam ac urna sollicitudin vestibulum.Nunc mattis tincidunt tellus. Vestibulum lobortis, libero sed vestibulum consequat, purus mi condimentum neque, a euismod lacus felis et nisl.Cras scelerisque lacus ac libero varius porttitor.Phasellus risus libero, scelerisque eu venenatis non, luctus a lacus. Cras pellentesque condimentum varius.

In hac habitasse platea dictumst.Cras at hendrerit lorem. Phasellus porta semper urna non pretium. Aenean scelerisque commodo turpis ac semper. Donec vehicula, mauris et rutrum placerat, tortor sem sodales nisi, ut dignissim augue nunc id sapien.Vestibulum porttitor sit amet lorem non gravida.Vivamus blandit libero sit amet neque sollicitudin, id vulputate massa ullamcorper. Nunc tincidunt sollicitudin mi, eget imperdiet enim malesuada ac. Morbi malesuada luctus viverra. Vivamus malesuada nisi a orci luctus condimentum.Donec consequat id magna vel consectetur. Cras non feugiat enim. Curabitur pretium odio orci. Sed mi augue, mattis ac placerat ultricies, fringilla sed tellus. Nam vehicula ligula vulputate quam ultrices, in pulvinar metus vestibulum.Lorem ipsum dolor sit amet, consectetur adipiscing elit.

Nulla ac sodales libero. Fusce mollis massa turpis, id finibus risus finibus non. Morbi vel porttitor odio. Pellentesque sapien orci, luctus scelerisque efficitur a, porta quis ante. Quisque eget porta turpis. Duis a eros sed risus ultricies eleifend ut eget diam. Integer vulputate consectetur nunc, et faucibus diam lacinia et. Quisque volutpat quam turpis, eget tempus lorem condimentum in. Cras ornare egestas magna a aliquam. Nunc nec tellus nunc. Fusce pellentesque imperdiet risus, sit amet faucibus neque euismod vitae.Maecenas fringilla lorem nibh.

Vestibulum feugiat massa ac ante cursus, eget rutrum quam accumsan.Ut fermentum, metus eu tempor malesuada, nulla tellus congue nulla, ut suscipit metus ante et diam.Donec pulvinar iaculis nisi, quis dictum leo ultrices eu. Suspendisse tristique vehicula ipsum, luctus porta ipsum viverra eu. Aliquam sagittis ultricies dignissim. Donec at tortor quis nunc tempor semper.Pellentesque facilisis, neque quis lobortis molestie, nulla purus auctor tellus, eu congue nisi ante vel leo.Duis congue arcu mollis erat mollis varius.Phasellus ut fermentum lectus, sit amet suscipit orci.Donec vitae semper tortor. In quis porttitor metus. Aliquam sit amet magna eget lacus elementum feugiat at et lorem.Phasellus et rutrum erat. Nulla pretium mi ac mi faucibus, eu facilisis nunc imperdiet.Nullam eget viverra erat. Aliquam sit amet fringilla orci.";
            Console.ReadLine();
            string[] words = x.Split(' ');

            var watch = System.Diagnostics.Stopwatch.StartNew();
            foreach (var word in words)
            {
                
                Console.WriteLine($"word \"{word}\" is of len {word.Length} -- Thread is {Thread.CurrentThread.ManagedThreadId}");
            }
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Elapsed: {elapsedMs}");

            var watch2 = System.Diagnostics.Stopwatch.StartNew();
            Parallel.ForEach(words, word =>
            {
                Console.WriteLine($"word \"{word}\" is of len {word.Length} -- Thread is {Thread.CurrentThread.ManagedThreadId}");
            });
            watch2.Stop();
            var elapsedMs2 = watch.ElapsedMilliseconds;
            Console.WriteLine($"Elapsed: {elapsedMs2}");

            Console.WriteLine("program running");
            //try
            //{
            //    Console.WriteLine($"task result {t1.Result}");
            //    Console.WriteLine($"task result {t2.Result}");

            //    Console.WriteLine($"task result {t1.Status}");
            //    Console.WriteLine($"task result {t2.Status}");
            //}
            //catch
            //{
            //    Console.WriteLine($"task result {t1.Status}");
            //    Console.WriteLine($"task result {t2.Status}");
            //}


            //Console.WriteLine("waiting to read");
            //ApmEap.Test();
            Console.ReadLine();

            Console.Read();
        }

        private static async void CatchMultipleExceptionsWithAwait()
        {
            int[] numbers = { 0 };

            Task<int> t1 = Task.Run(() => 5 / numbers[0]);
            Task<int> t2 = Task.Run(() => numbers[1]);

            Task<int[]> allTask = Task.WhenAll(t1, t2);
            try
            {
                await allTask;
            }
            catch
            {
                foreach (var ex in allTask.Exception.InnerExceptions)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        static async Task Catcher()
        {
            try
            {
                Task thrower = Thrower();
                await thrower;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex);
            }
        }

        async static Task Thrower()
        {
            await Task.Delay(100);
            throw new InvalidOperationException();
        }

        private static void AttachedToParent()
        {
            Task.Factory.StartNew(() =>
            {
                Task nested = Task.Factory.StartNew(() =>
                    Console.WriteLine("hello world"), TaskCreationOptions.AttachedToParent);
            }).Wait();

            Thread.Sleep(100);
        }

        private static int Print(bool isEven, CancellationToken token)
        {
            Console.WriteLine($"Is thread pool thread:{Thread.CurrentThread.IsThreadPoolThread}");
            int total = 0;
            if (isEven)
            {
                for (int i = 0; i < 99; i += 2)
                {
                    if (token.IsCancellationRequested)
                    {
                        Console.WriteLine("Cancellation Requested");
                    }
                    token.ThrowIfCancellationRequested();
                    total++;
                    Console.WriteLine($"Current task id = {Task.CurrentId}. Value={i}");
                }
            }
            else
            {
                for (int i = 1; i < 99; i += 2)
                {
                    if (token.IsCancellationRequested)
                    {
                        Console.WriteLine("Cancellation Requested");
                    }
                    token.ThrowIfCancellationRequested();
                    total++;
                    Console.WriteLine($"Current task id = {Task.CurrentId}. Value={i}");
                }
            }

            return total;
        }

        private static int Print(bool isEven)
        {
            Console.WriteLine("background:" + Thread.CurrentThread.IsBackground);
            Console.WriteLine("pool threaded:" + Thread.CurrentThread.IsThreadPoolThread);
            int total = 0;
            if (isEven)
            {
                for (int i = 0; i < 100; i += 2)
                {
                    total++;
                    Console.WriteLine($"Current task id = {Thread.CurrentThread.ManagedThreadId}. Value={i}");
                }
            }
            else
            {
                for (int i = 1; i < 10000; i += 2)
                {
                    total++;
                    Console.WriteLine($"Current task id = {Thread.CurrentThread.ManagedThreadId}. Value={i}");
                }
            }

            return total;
        }

        private static int hamada()
        {
            Console.WriteLine("background:" + Thread.CurrentThread.IsBackground);
            Console.WriteLine("pool threaded:" + Thread.CurrentThread.IsThreadPoolThread);
            while (true)
            {
                Thread.Sleep(3000);
                Console.WriteLine("1");
            }
            return 1;
        }


        /*
        private static void TokenWaitHandle(CancellationToken token)
        {
            if (token.WaitHandle.WaitOne(2000))
            {
                token.ThrowIfCancellationRequested();
            }
        }
        */
        /*
        private static void RunningTasks()
        {
            Task<int> t1 = Task.Factory.StartNew(() => Print(true), CancellationToken.None,
                TaskCreationOptions.DenyChildAttach | TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            Task<int> t2 = Task.Factory.StartNew(() => Print(false));

            Console.WriteLine($"The first task processed:{t1.Result}");
            Console.WriteLine($"The second task processed:{t2.Result}");
        }*/
    }
}