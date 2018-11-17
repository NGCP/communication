using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using UGV.Core.IO;
using Newtonsoft.Json;

namespace UGV.XBee
{
    public class XBeeDevice
    {
        // XBee object
        private Serial xbee;
        // Checks if xbee started or not
        private bool started;
        // Map of MAC addresses xbee can send data to
        private Dictionary<string, string> destinations;
        public XBeeDevice(string port, int baudRate, Dictionary<string, string> destinationMACs)
        {
            SetupXBee(port, baudRate);
            destinations = destinationMACs;
        }

        public XBeeDevice(string port, int baudRate, string destinationMAC)
        {
            SetupXBee(port, baudRate);
            destinations = new Dictionary<string, string>
            {
                { "default", destinationMAC }
            };
        }
        
        public XBeeDevice(string port, int baudRate)
        {
            SetupXBee(port, baudRate);
            destinations = new Dictionary<string, string>();
        }

        private void SetupXBee(string port, int baudRate)
        {
            xbee = new Serial(port, baudRate)
            {
                EscapeToken = new byte[] { 253, 254, 255 },
                PackageReceived = bytes =>
                {
                    Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                    Int32 number = BitConverter.ToInt32(bytes, 0);
                    Console.WriteLine("Received: {0}", number);
                    Console.WriteLine();
                    /*
                    string json = Encoding.UTF8.GetString(bytes);
                    Console.WriteLine("Received: {0}", json);
                    Console.WriteLine();
                    */
                },
            };
            started = false;
        }

        public bool Start()
        {
            if (started) return false;

            try
            {
                xbee.Start();
                started = true;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool Stop()
        {
            if (!started) return false;

            xbee.Stop();
            started = false;
            return true;
        }

        public void Send(XBeePacket packet)
        {
            string json = JsonConvert.SerializeObject(packet);
            List<byte> bytes = Encoding.UTF8.GetBytes(json).ToList();
            bytes.AddRange(xbee.EscapeToken);

            xbee.Send(bytes.ToArray());
            Console.WriteLine("Sent: {0}", json);
            Console.WriteLine();
        }

        public void Send(Int32 number)
        {
            List<byte> bytes = BitConverter.GetBytes(number).ToList();
            bytes.AddRange(xbee.EscapeToken);

            xbee.Send(bytes.ToArray());
            Console.WriteLine("Sent: {0}", number);
            Console.WriteLine();
        }
    }
}
