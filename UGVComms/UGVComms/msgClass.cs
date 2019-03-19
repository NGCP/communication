using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UGVComms
{
    public class connectMsg
    {
        public const string type = "connect";
        public int time { get; set; }
        public string jobsAvailable { get; set; }
    }

    public class updateMsg
    {
        public const string type = "update";
        public float lat { get; set; }
        public float lon { get; set; }
        public float heading { get; set; }
        public string status { get; set; }
    }

    public class ackMsg
    {
        public const string type = "ack";
    }

    public class completeMsg
    {
        public const string type = "complete";
    }
}
