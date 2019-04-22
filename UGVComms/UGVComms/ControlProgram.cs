using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

using MessagePack;
using MessagePack.Resolvers;
using XBee;

namespace UGVComms
{
    class ControlProgram
    {
        private const string portName = "COM7";
        private const int baudRate = 57600;
        private const string destinationMAC = "0013A2004194754E";

        private static readonly XBeeController xbee = new XBeeController();
        private static XBeeNode toXbee;

        private static long offset = 0;
        private static int messageId = 0;
        private static string status = "disconnected"; // status types: disconnected, ready, waiting, running, paused, error
        private static readonly Dictionary<int, MsgClass> outboxMsg = new Dictionary<int, MsgClass>();

        private static readonly Timer sendTimer = new Timer();

        public static void Main()
        {
            InitializeConnection(portName, baudRate, destinationMAC);
            Console.ReadLine();
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
                Time = Time(),

                JobsAvailable = new string[] { "ugvRescue" },
            };

            AddToOutbox(connect);
        }

        /**
         * NEVER acknowledge an incorrect message. Send a bad message and exit! If the vehicle is in an incorrect state, the vehicle
         * must NOT acknowledge an incorrect message. If UGV is not "ready", it should not acknowledge "addMission"
         * messages. This also goes if GCS assigns an invalid job/task to the UGV, do NOT acknowledge.
         */
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
                    ProcessConnAckMsg(MessagePackSerializer.Deserialize<ConnAckMsg>(eventArgs.Data));
                    break;

                case "start":
                    ProcessStartMsg(MessagePackSerializer.Deserialize<StartMsg>(eventArgs.Data));
                    break;

                case "addMission":
                    ProcessAddMissionMsg(MessagePackSerializer.Deserialize<AddMissionMsg>(eventArgs.Data));
                    break;

                case "pause":
                    ProcessPauseMsg(MessagePackSerializer.Deserialize<PauseMsg>(eventArgs.Data));
                    break;

                case "resume":
                    ProcessResumeMsg(MessagePackSerializer.Deserialize<ResumeMsg>(eventArgs.Data));
                    break;

                case "stop":
                    ProcessStopMsg(MessagePackSerializer.Deserialize<StopMsg>(eventArgs.Data));
                    break;

                case "ack":
                    AckMsg ack = MessagePackSerializer.Deserialize<AckMsg>(eventArgs.Data);
                    outboxMsg.Remove(ack.AckId);

                    Console.WriteLine("Acknowledgement Received");
                    Console.WriteLine("AckId: " + ack.AckId);
                    break;

                default:
                    BadMessage(msg, "Message type is unrecognized by UGV");
                    break;
            }
        }

        /**
         * Add message to outbox to be sent.
         */
        private static void AddToOutbox(MsgClass msg)
        {
            outboxMsg.Add(messageId, msg);
            messageId += 1;
        }

        /**
         * Support function for the timer to run. Do not run this outside of the timer.
         */
        private static void SendMessages(Object source, ElapsedEventArgs e)
        {
            foreach (KeyValuePair<int, MsgClass> entry in outboxMsg)
            {
                SendMessage(entry.Value);
            }
        }
        
        /**
         * Run this if you are sending a single message.
         */
        private static async void SendMessage(MsgClass msg)
        {
            byte[] bytes = new byte[] { };
            string json = "";

            if (msg.Type == "connect")
            {
                bytes = MessagePackSerializer.Serialize((ConnectMsg)msg);
                json = MessagePackSerializer.ToJson((ConnectMsg)msg);
            }
            else if (msg.Type == "update")
            {
                bytes = MessagePackSerializer.Serialize((UpdateMsg)msg);
                json = MessagePackSerializer.ToJson((UpdateMsg)msg);
            }
            else if (msg.Type == "poi")
            {
                bytes = MessagePackSerializer.Serialize((POIMsg)msg);
                json = MessagePackSerializer.ToJson((POIMsg)msg);
            }
            else if (msg.Type == "complete")
            {
                bytes = MessagePackSerializer.Serialize((CompleteMsg)msg);
                json = MessagePackSerializer.ToJson((CompleteMsg)msg);
            }

            if (bytes.Length == 0)
            {
                throw new IndexOutOfRangeException("Message type is wrong, cannot send this message");
            }

            Console.WriteLine(json);
            await toXbee.TransmitDataAsync(bytes);
        }

        private static void Acknowledge(MsgClass msg)
        {
            AckMsg ack = new AckMsg()
            {
                Id = messageId,
                Tid = msg.Sid,
                Time = Time(),
                AckId = msg.Id,
            };
            SendMessage(ack);
        }

        private static void BadMessage(MsgClass msg, string error)
        {
            BadMsg bad = new BadMsg()
            {
                Id = messageId,
                Tid = msg.Sid,
                Time = Time(),
                Error = error,
            };
            SendMessage(bad);
        }

        /**
         * Gets current time with GCS offset.
         */
        private static long Time()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + offset;
        }

        private static void ProcessConnAckMsg(ConnAckMsg msg)
        {
            if (status != "disconnected")
            {
                Console.WriteLine("ERROR: Received connectionAck message while status is {0}", status);
                BadMessage(msg, "Received connectionAck message while status is " + status);
                return;
            }

            Acknowledge(msg);
            offset = msg.Time - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            status = "ready";

            // TODO: start sending update messages to GCS

            Console.WriteLine("Connecting");
            Console.WriteLine("Time: " + msg.Time);
            Console.WriteLine("");
            Console.WriteLine("Awaiting command...");
            Console.WriteLine("");
        }

        private static void ProcessStartMsg(StartMsg msg)
        {
            if (status != "ready")
            {
                Console.WriteLine("ERROR: Received start message while status is {0}", status);
                BadMessage(msg, "Received start message while status is " + status);
                return;
            }

            if (msg.jobType != "ugvRescue")
            {
                Console.WriteLine("ERROR: Received incorrect jobType {0} from start message", msg.jobType);
                BadMessage(msg, "Received incorrect jobType " + msg.jobType + " from start message");
                return;
            }


            // TODO: Wait for confirmation that UGV is running before changing state to "running"
            // Something like the following... software will need to implement startJob function, or something similar
            // bool success = startJob(msg.jobType);
            // if (success) {
            //    state = "waiting";
            // } else {
            //    send error message or something idk, put vehicle on error state probably so GCS can stop mission
            // }
            // Acknowledge afterwards

            Acknowledge(msg);

            Console.WriteLine("Starting Mission");
            Console.WriteLine(msg.jobType);
        }

        private static void ProcessAddMissionMsg(AddMissionMsg msg)
        {
            if (status != "waiting")
            {
                Console.WriteLine("ERROR: Received addMission message while status is {0}", status);
                BadMessage(msg, "Received addMission message while status is " + status);
                return;
            }

            if (msg.MissionInfo.TaskType != "retrieveTarget" && msg.MissionInfo.TaskType != "deliverTarget")
            {
                Console.WriteLine("ERROR: Received incorrect taskType {0} from addMission message", msg.MissionInfo.TaskType);
                BadMessage(msg, "Received incorrect taskType " + msg.MissionInfo.TaskType + " from addMission message");
                return;
            }

            // TODO: Wait for confirmation that UGV is running before changing state to "running"
            // Something like the following... software will need to implement startTask function, or something similar
            // if (success) {
            //    state = "running";
            // } else {
            //    send error message or something idk, put vehicle on error state probably so GCS can stop mission
            // }
            // Acknowledge afterwards

            Acknowledge(msg);

            Console.WriteLine("Adding Mission");
            Console.WriteLine("Latitude: " + msg.MissionInfo.Lat);
            Console.WriteLine("Longitude: " + msg.MissionInfo.Lng);
        }

        private static void ProcessPauseMsg(PauseMsg msg)
        {
            if (status != "running")
            {
                Console.WriteLine("ERROR: Received pause message while status is {0}", status);
                BadMessage(msg, "Received pause message while status is " + status);
                return;
            }

            // TODO: Wait for confirmation that UGV is paused before changing state to "paused"

            Acknowledge(msg);
        }

        private static void ProcessResumeMsg(ResumeMsg msg)
        {
            if (status != "paused")
            {
                Console.WriteLine("ERROR: Received resume message while status is {0}", status);
                BadMessage(msg, "Received resume message while status is " + status);
                return;
            }

            // TODO: Wait for confirmation that UGV has resumed before changing state to "running"
            Acknowledge(msg);
        }

        private static void ProcessStopMsg(StopMsg msg)
        {
            // GCS can send stop message even if vehicle is ready (usually its because other vehicles arent working right)
            if (status != "waiting" && status != "running" && status != "paused") return;

            // TODO: Wait for confirmation that UGV has stopped before changing state to "ready"
            Acknowledge(msg);
        }
    }
}
