using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using vtortola.WebSockets;


namespace BackgroundSocketAndPrinterApp
{
    class Program
    {

        static void Main(string[] args)
        {

            SocketInterface.runSocketServer();
           
            Console.ReadKey(true);



        }

    }



}
