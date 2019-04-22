using MessagePack;

namespace UGVComms
{
     // *** Name all float/long types to double

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
        public double Time;
    }

    //////////////messages to be sent/////////////
    public class ConnectMsg : MsgClass
    {
        [Key("jobsAvailable")]
        public string[] JobsAvailable;
        public ConnectMsg()
        {
            Type = "connect";
        }
    }

    public class UpdateMsg : MsgClass
    {
        [Key("lat")]
        public double Lat;
        [Key("lng")]
        public double Lng;
        [Key("status")]
        public string Status;
        [Key("heading")]
        public double Heading;
        [Key("battery")]
        public double Battery;
        public UpdateMsg()
        {
            Type = "update";
        }
    }

    public class POIMsg : MsgClass
    {
        [Key("lat")]
        public string Lat;
        [Key("lng")]
        public string Lng;
        public POIMsg()
        {
            Type = "poi";
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
        public double Lat;
        [Key("lng")]
        public double Lng;
    }

    public class PauseMsg : MsgClass
    {
        public PauseMsg()
        {
            Type = "pause";
        }
    }

    public class ResumeMsg : MsgClass
    {
        public ResumeMsg()
        {
            Type = "resume";
        }
    }

    public class StopMsg : MsgClass
    {
        public StopMsg()
        {
            Type = "stop";
        }
    }

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
        [Key("error")]
        public string Error;
        public BadMsg()
        {
            Type = "badMessage";
        }
    }
}
