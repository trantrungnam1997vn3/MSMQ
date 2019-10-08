using System;
using System.Collections.Generic;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueueNet
{
    public class Program
    {

        //**************************************************
        // Provides an entry point into the application.
        //		 
        // This example demonstrates several ways to set
        // a queue's path.
        //**************************************************

        public class Order
        {
            public int orderId;
            public DateTime orderTime;
        };



        public static void Main()
        {
            // Create a new instance of the class.
            //MyNewQueue myNewQueue = new MyNewQueue();

            //myNewQueue.SendPublic();
            //myNewQueue.SendPrivate();
            //myNewQueue.SendByLabel();
            //myNewQueue.SendByFormatName();
            //myNewQueue.MonitorComputerJournal();
            //myNewQueue.MonitorQueueJournal();
            //myNewQueue.MonitorDeadLetter();
            //myNewQueue.MonitorTransactionalDeadLetter();

            //return;
            MyNewQueue myNewQueue = new MyNewQueue();

            myNewQueue.SendMessage();

            myNewQueue.ReceiveMessage();

            return;
        }

    }
    public class MyNewQueue
    {
        public void SendMessage()
        {
            Program.Order sentOrder = new Program.Order();
            sentOrder.orderId = 3;
            sentOrder.orderTime = DateTime.Now;
            MessageQueue myQueue = new MessageQueue(".\\private$\\testConnect");
            myQueue.Send(sentOrder);
            myQueue.Send(sentOrder);
            return;
        }


        //**************************************************
        // Receives a message containing an Order.
        //**************************************************

        public void ReceiveMessage()
        {
            // Connect to the a queue on the local computer.
            MessageQueue myQueue = new MessageQueue(".\\private$\\testConnect");

            // Set the formatter to indicate body contains an Order.
            myQueue.Formatter = new XmlMessageFormatter(new Type[]
                {typeof(Program.Order)});

            try
            {
                // Receive and format the message. 
                Message myMessage = myQueue.Receive();
                Program.Order myOrder = (Program.Order)myMessage.Body;

                // Display message information.
                Console.WriteLine("Order ID: " +
                                  myOrder.orderId.ToString());
                Console.WriteLine("Sent: " +
                                  myOrder.orderTime.ToString());
            }

            catch (MessageQueueException)
            {
                // Handle Message Queuing exceptions.
            }

            // Handle invalid serialization format.
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
            }

            // Catch other exceptions as necessary.

            return;
        }

        public class MessageQueue<T>
        {
            private Queue<T> _msgQueue;
            private Action<T> _processMsg;
            private Task _runUpdateDynamicData;
            private CancellationTokenSource _tokenSource;
            private CancellationToken _ct;

            public MessageQueue(Action<T> processMsg)
            {
                _processMsg = processMsg;
                _msgQueue = new Queue<T>();
                _tokenSource = new CancellationTokenSource();
                _ct = _tokenSource.Token;
                _runUpdateDynamicData = new Task(ProcessMsgInQueue, _ct);
                _runUpdateDynamicData.Start();
            }

            private readonly object _token = new object();
            public void EnqueueAMessage(T msg)
            {
                lock (_token)
                {
                    _msgQueue.Enqueue(msg);
                    Monitor.Pulse(_token);
                }
            }

            private void ProcessMsgInQueue()
            {
                while (true)
                {
                    T msg;
                    lock (_token)
                    {
                        if (_msgQueue.Count == 0) Monitor.Wait(_token);
                        if (_ct.IsCancellationRequested)
                            _ct.ThrowIfCancellationRequested();
                        msg = _msgQueue.Dequeue();
                    }

                    _processMsg(msg);
                }
            }

            public void TerminateQueue()
            {
                Monitor.PulseAll(_token);
                _tokenSource.Cancel();
            }
        }

        // References public queues.
        public void SendPublic()
        {
            MessageQueue myQueue = new MessageQueue(".\\myQueue");
            myQueue.Send("Public queue by path name.");

            return;
        }

        // References private queues.
        public void SendPrivate()
        {
            MessageQueue myQueue = new
                MessageQueue(".\\Private$\\myQueue");
            myQueue.Send("Private queue by path name.");

            return;
        }

        // References queues by label.
        public void SendByLabel()
        {
            MessageQueue myQueue = new MessageQueue("Label:TheLabel");
            myQueue.Send("Queue by label.");

            return;
        }

        // References queues by format name.
        public void SendByFormatName()
        {
            MessageQueue myQueue = new
                MessageQueue("FormatName:Public=5A5F7535-AE9A-41d4" +
                "-935C-845C2AFF7112");
            myQueue.Send("Queue by format name.");

            return;
        }

        // References computer journal queues.
        public void MonitorComputerJournal()
        {
            MessageQueue computerJournal = new
                MessageQueue(".\\Journal$");
            while (true)
            {
                Message journalMessage = computerJournal.Receive();
                // Process the journal message.
            }
        }

        // References queue journal queues.
        public void MonitorQueueJournal()
        {
            MessageQueue queueJournal = new
                MessageQueue(".\\myQueue\\Journal$");
            while (true)
            {
                Message journalMessage = queueJournal.Receive();
                // Process the journal message.
            }
        }

        // References dead-letter queues.
        public void MonitorDeadLetter()
        {
            MessageQueue deadLetter = new
                MessageQueue(".\\DeadLetter$");
            while (true)
            {
                Message deadMessage = deadLetter.Receive();
                // Process the dead-letter message.
            }
        }

        // References transactional dead-letter queues.
        public void MonitorTransactionalDeadLetter()
        {
            MessageQueue TxDeadLetter = new
                MessageQueue(".\\XactDeadLetter$");
            while (true)
            {
                Message txDeadLetter = TxDeadLetter.Receive();
                // Process the transactional dead-letter message.
            }

        }
    }


}
