using Ainvar.Bus.InProcess;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace SandBusTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var dispatch = new Dispatch(Guid.NewGuid(), "dispatch");
            var owner = dispatch.DispatchOwner;

            SandBus.Instance.Subscribe(d =>
            {
                //string prova = d.Content;
                //StringBuilder prova2 = new StringBuilder(d.TimeStamp.ToLongTimeString());
                //Console.WriteLine(SandBus.Instance.DispatchesCount.ToString() + " " + prova);
                //Console.Clear();
            });


            SandBus.Instance.Subscribe(d =>
            {
                //string prova = d.Content;
                //StringBuilder prova2 = new StringBuilder(d.TimeStamp.ToLongTimeString());
            });

            SandBus.Instance.Subscribe(d =>
            {
                //string prova = d.Content;
                //StringBuilder prova2 = new StringBuilder(d.TimeStamp.ToLongTimeString());
                //Console.WriteLine(prova2.ToString());
            });

            SandBus.Instance.SubscribeActionByOwnerGuid(owner, d =>
            {
                //Console.Clear();

                //Console.WriteLine(owner.ToString());
                //Console.WriteLine("owner:" + d.DispatchOwner);

                //Thread.Sleep(1000);
            }
            );

            

            //DispatchesBus.Instance.SubscribeAndGetSubscription((d) =>
            //    {

            //    }
            //);



            //var p = Enumerable.Range(0, 20000).ToObservable();
            var time = new Stopwatch();

            time.Start();

            var tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;


            for (var i = 0; i < 10000000; i++)
            {
                //Action ec = async () => await DispatchesBus.Instance.DispatchAsync(new InDispatch(Guid.NewGuid(), "dispaccio"), token);
                //Console.WriteLine("Invio dispaccio n: " + i.ToString());
                Action ec = async () => await SandBus.Instance.DispatchAsync(dispatch, token);
                ec();

                //if (i % 10000000 == 0)
                //{
                //    GC.Collect();
                //    GC.WaitForPendingFinalizers();
                //}
            }
            //DispatchesBus.Instance.ReceiveAll();

            time.Stop();

            Console.WriteLine(string.Format("Tempo per l'Invio di tutti i dispacci:{0:mm\\:ss}", time.Elapsed));
            Console.ReadKey();
        }
    }
}
