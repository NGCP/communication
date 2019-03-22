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

    //////////////messages to be sent/////////////
    public class ConnectMsg : MsgClass
    {
        public new const string type = "connect";
        public int Time { get; set; }
        public string jobsAvailable = "searchAndRescue";
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
        public new string type;
        public int Time;
 	}
 	
 	public class RecAckMsg : MsgClass 
 	{
        public new string type;
 	}
 	
 	public class StartMsg : MsgClass 
 	{
        public new string type;
        public string jobType;
 	}
 	
 	public class AddMissionMsg : MsgClass 
 	{
        public new string type;
        public int lat;
        public int lon;
 	}
}
