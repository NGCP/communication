using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MessagePack;

namespace UGVComms
{
    /* Classes are being used instead of structs because structs are considered value types
     * which means they are copied when they are passed around. 
     * 
     * If you change a copy, changes are only made to that specific copy, and not to the 
     * original nor to any other copies around in use that you may have wanted to change as well.
     * 
     * Manipulating a struct's properties as you would a class would require you pass the struct
     * by reference, which such an implementation is significantly more prone to hard-to-catch 
     * mistakes, as well as reduced code maintainability. 
     * 
     * Using structs for this implementation would prove to be less efficient and not worth
     * the effort as performance increases are negligible when comparing classes and structs.
     */

    [MessagePackObject]
    public class MsgClass
    {
        [Key("type")]
        public string Type;
        [Key("id")]
        public int Id;
        [Key("sid")]
        public int Sid;
        [Key("tid")]
        public int Tid;
        [Key("time")]
        public long Time = (long) DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    //////////////messages to be sent/////////////
    public class ConnectMsg : MsgClass
    {
        [Key("jobsAvailable")]
        public string[] JobsAvailable = { "ugvRescue" };

        public ConnectMsg()
        {
            Type = "connect";
        }
    }

    public class UpdateMsg : MsgClass
    {
        [Key("lat")]
        public float Lat;
        [Key("lng")]
        public float Lng;
        [Key("status")]
        public string Status;
        [Key("heading")]
        public float Heading;
        [Key("battery")]
        public float Battery;

        public UpdateMsg()
        {
            Type = "connect";
        }
    }

    public class CompleteMsg : MsgClass
    {
        public CompleteMsg()
        {
            Type = "complete";
        }
    }
    
    //////////////messages to be received/////////////
    public class ConnAckMsg : MsgClass      
 	{
        public ConnAckMsg()
        {
            Type = "connectionAck";
        }
 	}
 	
 	public class StartMsg : MsgClass        
 	{
        [Key("jobType")]
        public string jobType;

        public StartMsg()
        {
            Type = "start";
        }
 	}
 	
 	public class AddMissionMsg : MsgClass  
 	{
        [Key("missionInfo")]
        public MissionInfo MissionInfo; // either retrieveTarget or deliverTarget; same values required

        public AddMissionMsg()
        {
            Type = "addMission";
        }


    }
    public class MissionInfo
    {
        [Key("taskType")]
        public string TaskType;
        [Key("lat")]
        public float Lat;
        [Key("lng")]
        public float Lng;
    }

    //////////////other messages//////////////
    public class AckMsg : MsgClass
    {
        [Key("ackId")]
        public int AckId;

        public AckMsg()
        {
            Type = "ack";
        }
    }

    public class BadMsg : MsgClass
    {
        public string Error;

        public BadMsg()
        {
            Type = "badMessage";
        }
    }
}





