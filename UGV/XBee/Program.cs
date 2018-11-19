using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using XBee;

namespace UGV.XBee
{
    public struct Config
    {
        public const string Port = "COM5";
        public const int BaudRate = 57600;
        // destinationMAC has no use as of now. This can be used in the coming future
        // see my comment above xbee.NodeDiscovered to see what a good idea for this will be
        // im thinking of a hashmap with key as team platform name(UAV, UUV, GCS) and
        // the value to be the xbee's MAC address
        public const string DestinationMAC = "0013A20040917A31";
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            // reference to xbee object, which will be instantiated in the XBeeCore function
            XBeeController xbee = new XBeeController();

            XBeeCore(xbee);

            // readline is important because the program just closes otherwise
            // try it out: uncomment the following line and see what happens
            Console.ReadLine();

            // always dispose the xbee object afterwards
            xbee.Dispose();
        }

        public static async void XBeeCore(XBeeController xbee)
        {
            Console.WriteLine("Press enter twice to exit");

            // this is the fake data we will send from UGV end
            XBeePacket packet = new XBeePacket(1, 2); ;

            // this callback runs when data is received from another xbee
            xbee.DataReceived += (sender, eventArgs) =>
            {
                string json = Encoding.UTF8.GetString(eventArgs.Data);
                Console.WriteLine(">> Received");
                Console.WriteLine(JsonConvert.DeserializeObject<XBeePacket>(json));
                Console.WriteLine();
            };

            // this callback is run when we discover an xbee somewhere
            // better idea in the future is to explicitly list the xbees to connect to for safety
            xbee.NodeDiscovered += async (sender, args) => {
                while (true)
                {
                    await Task.Delay(5000);
                    Console.WriteLine("<< Sent");
                    Console.WriteLine(packet);
                    Console.WriteLine();
                    await args.Node.TransmitDataAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(packet)));
                }
            };

            // try catch statement to do the following:
            // initialize xbee and let it find all the xbees around it
            try
            {
                await xbee.OpenAsync(Config.Port, Config.BaudRate);
                await xbee.DiscoverNetworkAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Failed to initialize XBee");
                return;
            }
        }
    }
}
