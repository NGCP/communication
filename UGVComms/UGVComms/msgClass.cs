using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UGVComms
{
    public class MsgClass
    {
        public string type;
    }
    public class ConnectMsg : MsgClass
    {
        public new const string type = "connect";
        public int time { get; set; }
        public string jobsAvailable { get; set; }
    }

    public class UpdateMsg : MsgClass
    {
        public new const string type = "update";
        public float lat { get; set; }
        public float lon { get; set; }
        public float heading { get; set; }
        public string status { get; set; }
    }

    public class AckMsg : MsgClass
    {
        public new const string type = "ack";
    }

    public class CompleteMsg : MsgClass
    {
        public new const string type = "complete";
    }
    
    //////////////messages to be received/////////////
    public class ConnAckMsg : MsgClass
 	{
 		public new const string type = "connectionAck";
 		public int time { get; set; }
 	}
 	
 	public class RecAckMsg : MsgClass 
 	{
 		public new const string type = "ack";
 	}
 	
 	public class StartMsg : MsgClass 
 	{
 		public new const string type = "start";
 		public const string jobType = "SearchAndRescue";
 	}
 	
 	public class AddMissionMsg : MsgClass 
 	{
 		public new const string type = "addMission";
 		public int lat { get; set; }
 		public int lon { get; set; }
 	}
}
