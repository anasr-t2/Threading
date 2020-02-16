using Signaling.BankTerminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Signaling
{
    public class Protocol2
    {
        private readonly IPEndPoint _endpoint;
        public event EventHandler<ProtocolMessage> OnMessageRecieved;

        public Protocol2(IPEndPoint endpoint)
        {
            _endpoint = endpoint;
        }

        public void Send(int opCode, object args)
        {
            Task.Run(() => {
                //do some work
                Console.WriteLine("protocol: started working");
                Thread.Sleep(5000);
                Console.WriteLine("protocol: work done");

                OnMessageRecieved?.Invoke(this, new ProtocolMessage(OperationStatus.Finished));
            });
           
        }
    }

    public class BankTerminal2
    {
        private readonly Protocol2 _protocol;
        private readonly AutoResetEvent _operationSignal = new AutoResetEvent(false);

        public BankTerminal2(IPEndPoint endpoint)
        {
            _protocol = new Protocol2(endpoint);
            _protocol.OnMessageRecieved += Protocol_OnMessageRecieved;
        }

        private void Protocol_OnMessageRecieved(object sender, ProtocolMessage e)
        {
            Thread.Sleep(5000);

            //Console.WriteLine("Bank Terminal: Signal Recieved");
            if (e.Status == OperationStatus.Finished)
            {
                Console.WriteLine("Bank Terminal: Signaling");
                _operationSignal.Set();
            }

        }

        public Task<decimal> Purchase(decimal amount)
        {
           return  Task.Run(() => {
                const int purchaseCode = 1;
               //_operationSignal.Reset();
                _protocol.Send(purchaseCode, amount);
               Console.WriteLine("Bank Terminal: Waiting Signal");
               _operationSignal.WaitOne();
               return amount;
           });
        }
    }
}
