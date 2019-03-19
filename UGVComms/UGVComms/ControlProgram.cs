using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XBee;
using Newtonsoft.Json;

namespace UGVComms
{
    class ControlProgram
    {
        private const string PortName = "COM3";
        private const int BaudRate = 57600;
        public const string DestinationMAC = "0013A20040A5430F";



        static void Main(string[] args)
        {
            initializeConnection(PortName, BaudRate, DestinationMAC);
        }

        static async void initializeConnection(string PortName, int BaudRate, string DestinationMAC)
        {
            Object message;
            //sending (xbee) and receiving (toXbee) xbees
            XBeeController xbee = new XBeeController();
            XBeeNode toXbee;

            //opens this xbee to connection
            await xbee.OpenAsync(PortName, BaudRate);
            //find the destination xbee and assign it to toXBee;
            toXbee = await xbee.GetNodeAsync(new NodeAddress(new LongAddress(UInt64.Parse(DestinationMAC))));

            xbee.DataReceived += (sender, eventArgs) =>
            {
                string jsonString = Encoding.UTF8.GetString(eventArgs.Data);
                //received data is stored in a json in this class for further use
                try
                {
                    message = JsonConvert.DeserializeObject(jsonString);
                    checkType(message);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Data received was not a json!");
                }

            };
        }


        //data processing
        static void checkType(Object msg)
        {
            string type = msg.type;
        }
    }
}
