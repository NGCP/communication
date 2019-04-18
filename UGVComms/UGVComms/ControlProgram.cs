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
            ConnectMsg connect = new ConnectMsg()
            {
                Id = messageId,
                Tid = 0,
                JobsAvailable = new string[] { "ugvRescue" },
                Time = Time(),
            };
            AddToOutbox(connect);
        }

        private static void ReceiveMessage(object sender, SourcedDataReceivedEventArgs eventArgs)
        {
            MsgClass msg;

            try
            {
                msg = MessagePackSerializer.Deserialize<MsgClass>(eventArgs.Data);
            }
            catch (Exception)
            {
                Console.WriteLine("Received invalid message {0}", eventArgs.Data.ToString());
                return;
            }

            switch (msg.Type)
            {
                case "connectionAck":
                    ConnAckMsg connAck = MessagePackSerializer.Deserialize<ConnAckMsg>(eventArgs.Data);
                    Acknowledge(connAck);

                    offset = connAck.Time - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    // TODO: start sending update messages to GCS

                    Console.WriteLine("Connecting");
                    Console.WriteLine("Time: " + connAck.Time);
                    Console.WriteLine("");
                    Console.WriteLine("Awaiting command...");
                    Console.WriteLine("");
                    break;
                case "start":
                    StartMsg start = MessagePackSerializer.Deserialize<StartMsg>(eventArgs.Data);
                    Acknowledge(start);
                    // TODO: add check to ensure UGV can perform the job (is in "ready" state, jobType is "ugvRescue")

                    Console.WriteLine("Starting Mission");
                    Console.WriteLine(start.jobType);
                    break;
                case "addMission":
                    AddMissionMsg addMission = MessagePackSerializer.Deserialize<AddMissionMsg>(eventArgs.Data);
                    Acknowledge(addMission);
                    // TODO: add check to ensure UGV can perform the task (is in "waiting" state, and valid task for jobType)

                    Console.WriteLine("Adding Mission");
                    Console.WriteLine("Latitude: " + addMission.MissionInfo.Lat);
                    Console.WriteLine("Longitude: " + addMission.MissionInfo.Lng);
                    break;
                case "pause":
                    PauseMsg pause = MessagePackSerializer.Deserialize<PauseMsg>(eventArgs.Data);
                    Acknowledge(pause);
                    // TODO: add check to ensure UGV is in a "running" state before pausing.
                    break;
                case "resume":
                    ResumeMsg resume = MessagePackSerializer.Deserialize<ResumeMsg>(eventArgs.Data);
                    Acknowledge(resume);
                    // TODO: add check to ensure UGV is in a "paused" state before resuming.
                    break;
                case "stop":
                    StopMsg stop = MessagePackSerializer.Deserialize<StopMsg>(eventArgs.Data);
                    Acknowledge(stop);
                    // TODO: ensure UGV will not break if this message is sent when it is in "ready" state
                    // UGV must go back to "ready" state if it is in "waiting" or "running" or "paused" state.
                    break;
                case "ack":
                    AckMsg ack = MessagePackSerializer.Deserialize<AckMsg>(eventArgs.Data);
                    outboxMsg.Remove(ack.AckId);

                    Console.WriteLine("Acknowledgement Received");
                    Console.WriteLine("AckId: " + ack.AckId);
                    break;
                default:
                    BadMsg bad = new BadMsg()
                    {
                        Id = messageId,
                        Tid = msg.Sid,
                        Time = Time(),
                        Error = "Message type is unrecognized by UGV",
                    };
                    SendMessage(bad);
                    break;
            }
        }

        private static void AddToOutbox(MsgClass msg)
        {
            outboxMsg.Add(messageId, msg);
            messageId += 1;
        }

        private static void Acknowledge(MsgClass msg)
        {
            AckMsg ack = new AckMsg()
            {
                Id = messageId,
                Tid = msg.Sid,
                AckId = msg.Id,
                Time = Time(),
            };
            SendMessage(ack);
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

        private static async void SendMessage(MsgClass msg)
        {
            await toXbee.TransmitDataAsync(MessagePackSerializer.Serialize(msg));
        }

        private static long Time()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + offset;
        }
    }
}
