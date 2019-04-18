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
        public int Sid = 200;
        [Key("tid")]
        public int Tid;
        [Key("time")]
        public long Time;

        public MsgClass()
        {
            Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public MsgClass(int id, int tid, long offset)
        {
            Id = id;
            Tid = tid;
            Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + offset;
        }
    }

    //////////////messages to be sent/////////////

    [MessagePackObject]
    public class ConnectMsg : MsgClass
    {
        [Key("jobsAvailable")]
        public string[] JobsAvailable = { "ugvRescue" };

        public ConnectMsg()
        {
            Type = "connect";
        }

        public ConnectMsg(int id, int tid, long offset) : base(id, tid, offset)
        {
            Type = "connect";
        }
    }

    [MessagePackObject]
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
            Type = "update";
        }

        public UpdateMsg(int id, int tid, long offset) : base(id, tid, offset)
        {
            Type = "update";
        }
    }

    [MessagePackObject]
    public class CompleteMsg : MsgClass
    {
        public CompleteMsg()
        {
            Type = "complete";
        }

        public CompleteMsg(int id, int tid, long offset) : base(id, tid, offset)
        {
            Type = "complete";
        }
    }

    //////////////messages to be received/////////////

    [MessagePackObject]
    public class ConnAckMsg : MsgClass      
 	{
        public ConnAckMsg()
        {
            Type = "connectionAck";
        }

        public ConnAckMsg(int id, int tid, long offset) : base(id, tid, offset)
        {
            Type = "connectionAck";
        }
    }

    [MessagePackObject]
    public class StartMsg : MsgClass        
 	{
        [Key("jobType")]
        public string jobType;

        public StartMsg()
        {
            Type = "start";
        }

        public StartMsg(int id, int tid, long offset) : base(id, tid, offset)
        {
            Type = "start";
        }
    }

    [MessagePackObject]
    public class AddMissionMsg : MsgClass  
 	{
        [Key("missionInfo")]
        public MissionInfo MissionInfo; // either retrieveTarget or deliverTarget; same values required

        public AddMissionMsg()
        {
            Type = "addMission";
        }

        public AddMissionMsg(int id, int tid, long offset) : base(id, tid, offset)
        {
            Type = "addMission";
        }
    }

    [MessagePackObject]
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

    [MessagePackObject]
    public class AckMsg : MsgClass
    {
        [Key("ackId")]
        public int AckId;

        public AckMsg()
        {
            Type = "ack";
        }

        public AckMsg(int id, int tid, long offset) : base(id, tid, offset)
        {
            Type = "ack";
        }
    }

    [MessagePackObject]
    public class BadMsg : MsgClass
    {
        [Key("error")]
        public string Error;

        public BadMsg()
        {
            Type = "badMessage";
        }

        public BadMsg(int id, int tid, long offset) : base(id, tid, offset)
        {
            Type = "badMessage";
        }
    }
}





