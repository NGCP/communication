using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

using MessagePack;
using XBee;

namespace UGVComms
{
    class ControlProgram
    {
        private const string portName = "COM3";
        private const int baudRate = 57600;
        private const string destinationMAC = "0013A20040A5430F";

        private static readonly XBeeController xbee = new XBeeController();
        private static XBeeNode toXbee;

        private static long offset = 0;
        private static int messageId = 0;
        private static readonly Dictionary<int, MsgClass> outboxMsg = new Dictionary<int, MsgClass>();

        private static readonly Timer sendTimer = new Timer();

        public static void Main()
        {
            InitializeConnection(portName, baudRate, destinationMAC);
        }

        private static async void InitializeConnection(string portName, int baudRate, string destinationMAC)
        {
            // Opens this xbee to connection
            await xbee.OpenAsync(portName, baudRate);

            // Find the destination xbee and assign it to toXBee;
            // "AllowHexSpecifer" is needed to accept hex values A-F
            toXbee = await xbee.GetNodeAsync(new NodeAddress(new LongAddress(UInt64.Parse(destinationMAC, System.Globalization.NumberStyles.AllowHexSpecifier))));

            xbee.DataReceived += ReceiveMessage;

            sendTimer.Elapsed += SendMessages;
            sendTimer.Interval = 1000;
            sendTimer.Enabled = true;

            SendConnect();
        }

        private static void SendConnect()
        {
            CreateMessage(new ConnectMsg(messageId, 0, offset));
        }

        private static void ReceiveMessage(object sender, SourcedDataReceivedEventArgs eventArgs)
        {
            MsgClass msg = MessagePackSerializer.Deserialize<MsgClass>(eventArgs.Data);

            switch (msg.Type)
            {
                case "connectionAck":
                    ConnAckMsg connAck = MessagePackSerializer.Deserialize<ConnAckMsg>(eventArgs.Data);
                    offset = connAck.Time - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    Console.WriteLine("Connecting");
                    Console.WriteLine("Time: " + connAck.Time);
                    Console.WriteLine("");
                    Console.WriteLine("Awaiting command...");
                    Console.WriteLine("");
                    break;
                case "start":
                    StartMsg start = MessagePackSerializer.Deserialize<StartMsg>(eventArgs.Data);
                    // TODO: add check to ensure UGV can perform the job (is in "ready" state, jobType is "ugvRescue")

                    Console.WriteLine("Starting Mission");
                    Console.WriteLine(start.jobType);
                    break;
                case "addMission":
                    AddMissionMsg addMission = MessagePackSerializer.Deserialize<AddMissionMsg>(eventArgs.Data);
                    // TODO: add check to ensure UGV can perform the task (is in "waiting" state, and valid task for jobType)

                    Console.WriteLine("Adding Mission");
                    Console.WriteLine("Latitude: " + addMission.MissionInfo.Lat);
                    Console.WriteLine("Longitude: " + addMission.MissionInfo.Lng);
                    break;
                case "pause":
                    // TODO: add check to ensure UGV is in a "running" state before pausing.
                    break;
                case "resume":
                    // TODO: add check to ensure UGV is in a "paused" state before resuming.
                    break;
                case "ack":
                    AckMsg ack = MessagePackSerializer.Deserialize<AckMsg>(eventArgs.Data);
                    outboxMsg.Remove(ack.AckId);

                    Console.WriteLine("Acknowledgement Received");
                    Console.WriteLine("AckId: " + ack.AckId);
                    break;
                default:
                    SendMessage(new BadMsg(messageId, 0, offset));
                    break;
            }
        }

        private static void CreateMessage(MsgClass msg)
        {
            outboxMsg.Add(messageId, msg);
            messageId += 1;
        }

        /**
         * Only run this function for messages that are not going to be sent once.
         * Messages that should only be sent once (ack + badMessage) should just be sent once.
         */
        private static void SendMessages(Object source, ElapsedEventArgs e)
        {
            foreach(KeyValuePair<int, MsgClass> entry in outboxMsg)
            {
                SendMessage(entry.Value);
            }
        }

        static async void SendMessage(MsgClass msg)
        {
            await toXbee.TransmitDataAsync(MessagePackSerializer.Serialize(msg));
        }
    }
}
