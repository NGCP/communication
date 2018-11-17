using System;

namespace UGV.XBee
{
    public struct Config
    {
        public const string Port = "COM5";
        public const int BaudRate = 57600;
        public const string DestinationMAC = "0013A20040917A31";
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            XBeeDevice xbee = new XBeeDevice(Config.Port, Config.BaudRate, Config.DestinationMAC);
            XBeeDevice xbee_r = new XBeeDevice("COM6", 57600);

            bool success1 = xbee.Start();
            bool success2 = xbee_r.Start();
            if (!success1 || !success2)
            {
                Console.WriteLine("Failed to initialize XBees");
                Console.ReadLine();
                return;
            }

            string input;
            XBeePacket packet;
            Random rnd = new Random();
            while (true)
            {
                Console.Write("Type anything to send a packet (-1 to exit): ");
                input = Console.ReadLine();
                if (input.Equals("-1")) break;

                xbee.Send(rnd.Next(1, 50));
                /*
                packet = new XBeePacket(rnd.Next(1,50), rnd.Next(1, 50));
                xbee.Send(packet);
                */
            }

            xbee.Stop();
            xbee_r.Start();
        }
    }
}
