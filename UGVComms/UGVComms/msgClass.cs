using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UGVComms
{
    public class msgClass
    {
        public string type;
    }
    public class connectMsg : msgClass
    {
        public new const string type = "connect";
        public int time { get; set; }
        public string jobsAvailable { get; set; }
    }

    public class updateMsg : msgClass
    {
        public new const string type = "update";
        public float lat { get; set; }
        public float lon { get; set; }
        public float heading { get; set; }
        public string status { get; set; }
    }

    public class ackMsg : msgClass
    {
        public new const string type = "ack";
    }

    public class completeMsg : msgClass
    {
        public new const string type = "complete";
    }
    
    //////////////messages to be received/////////////
    public class connAckMsg : msgClass
 	{
 		public new const string type = "connectionAck";
 		public int time { get; set; }
 	}
 	
 	public class recAckMsg : msgClass 
 	{
 		public new const string type = "ack";
 	}
 	
 	public class startMsg : msgClass 
 	{
 		public new const string type = "start";
 		public new const string jobType = "SearchAndRescue";
 	}
 	
 	public class addMissionMsg : msgClass 
 	{
 		public new const string type = "addMission";
 		public int lat { get; set; }
 		public int lon { get; set; }
 	}
}
