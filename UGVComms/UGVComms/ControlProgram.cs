using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XBee;
using Newtonsoft.Json;
using System.Threading;

namespace UGVComms
{
    class ControlProgram
    {
        private const string PortName = "COM3";
        private const int BaudRate = 57600;
        public const string DestinationMAC = "0013A20040A5430F";
        static ManualResetEvent _quitEvent = new ManualResetEvent(false);
        // Sending (xbee) and receiving (toXbee) xbees
        static XBeeController xbee = new XBeeController();
        static XBeeNode toXbee;

        static void Main(string[] args)
        {
            
            // Important: Ctrl+C to exit program
            // Allows the program to continue to run until it is exited
            Console.CancelKeyPress += (sender, eArgs) => {
                _quitEvent.Set();
                eArgs.Cancel = true;
            };

            initializeConnection(PortName, BaudRate, DestinationMAC);
            Console.WriteLine("Press enter to send connection request");
            Console.ReadLine();
            sendConnect();

            _quitEvent.WaitOne();
        }

        static async void initializeConnection(string PortName, int BaudRate, string DestinationMAC)
        {
            MsgClass message;
            

            // Opens this xbee to connection
            await xbee.OpenAsync(PortName, BaudRate);
            // Find the destination xbee and assign it to toXBee;
            // "AllowHexSpecifer" is needed to accept hex values A-F
            toXbee = await xbee.GetNodeAsync(new NodeAddress(new LongAddress(UInt64.Parse(DestinationMAC, System.Globalization.NumberStyles.AllowHexSpecifier))));

            xbee.DataReceived += (sender, eventArgs) =>
            {
                // Received data is stored in a string in this class for further use
                string jsonString = Encoding.UTF8.GetString(eventArgs.Data);
                
                try
                {
                    // Converts the received data into usable json
                    message = JsonConvert.DeserializeObject<MsgClass>(jsonString);
                    checkType(message, jsonString);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Data received was not a json!");
                }
            };
        }

        static async void sendConnect()
        {
            ConnectMsg conn = new ConnectMsg();
            conn.Time = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            await toXbee.TransmitDataAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(conn)));
        }

        // Data processing

        // Checks "type" field of received data to determine what to do next
        static void checkType(MsgClass msg, string json)
        {
            string type = msg.type;
            switch(type)
            {
                case "connectionAck":
                    ConnAckMsg connAck = JsonConvert.DeserializeObject<ConnAckMsg>(json);
                    Console.WriteLine("Connecting");
                    Console.WriteLine("Time: " + connAck.Time);
                    Console.WriteLine("");
                    Console.WriteLine("Awaiting command...");
                    Console.WriteLine("");
                    break;
                case "start":
                    StartMsg start = JsonConvert.DeserializeObject<StartMsg>(json);
                    Console.WriteLine("Starting Mission");
                    Console.WriteLine(start.jobType);
                    break;
                case "addMission":
                    AddMissionMsg addMsg = JsonConvert.DeserializeObject<AddMissionMsg>(json);
                    Console.WriteLine("Adding Mission");
                    Console.WriteLine("Latitude: " + addMsg.missionInfo.lat);
                    Console.WriteLine("Longitude: " + addMsg.missionInfo.lng);
                    break;
                case "ack":
                    RecAckMsg recAck = JsonConvert.DeserializeObject<RecAckMsg>(json);
                    Console.WriteLine("Acknowledgement Received");
                    break;
            }
        }
    }
}
