using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using vtortola.WebSockets;
using vtortola.WebSockets.Rfc6455;
using System.Json;
using System.Runtime.InteropServices;

namespace BackgroundSocketAndPrinterApp
{




    class SocketInterface
    {


       



        public static object JsonConvert { get; private set; }

        public static void runSocketServer()
        {

            CancellationTokenSource cancellation = new CancellationTokenSource();

            var endpoint = new IPEndPoint(IPAddress.Loopback, 8005);
            WebSocketListener server = new WebSocketListener(endpoint);
            var rfc6455 = new vtortola.WebSockets.Rfc6455.WebSocketFactoryRfc6455(server);
            server.Standards.RegisterStandard(rfc6455);
            server.Start();

            Log("Echo Server started at " + endpoint.ToString());

            var task = Task.Run(() => AcceptWebSocketClientsAsync(server, cancellation.Token));

            Console.ReadKey(true);
            Log("Server stoping");
            cancellation.Cancel();
            task.Wait();
        }


        private static void Log(string v)
        {
            Console.WriteLine(v);
        }

        static async Task AcceptWebSocketClientsAsync(WebSocketListener server, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var ws = await server.AcceptWebSocketAsync(token).ConfigureAwait(false);
                    if (ws != null)
                        await Task.Run(() => HandleConnectionAsync(ws, token));
                }
                catch (Exception aex)
                {
                    Log("Error Accepting clients: " + aex.GetBaseException().Message);
                }
            }
            Log("Server Stop accepting clients");
        }



        static async Task HandleConnectionAsync(WebSocket ws, CancellationToken cancellation)
        {
            try
            {
                while (ws.IsConnected && !cancellation.IsCancellationRequested)
                {
                    String msg = await ws.ReadStringAsync(cancellation).ConfigureAwait(false);


                    if (msg != null)
                    {
                        ws.WriteString(handleMsg(msg));
                        
                    }
                }
            }
            catch (Exception aex)
            {
                Log("Error Handling connection: " + aex.GetBaseException().Message);
                try { ws.Close(); }
                catch { }
            }
            finally
            {
                ws.Dispose();
            }
        }






        static String handleMsg(string msg)
        {
            JsonValue json = JsonValue.Parse(msg);



            if (json.ContainsKey("do"))
            {

                PrinterInteface printerInterface = new PrinterInteface("receipt");


                if (json["do"] == "get_printers")
                {
                    printerInterface.printReceipt(json);
                    return printerInterface.getAllPrinters();
                }
                else
                if (json["do"] == "print_barcode")
                {
                   return printerInterface.printBarCode(json);
                }
                else
                if (json["do"] == "print_receipt")
                {
                    return printerInterface.printReceipt(json);
                }



            }
            return "";

        }



    }

}






