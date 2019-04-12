﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class MsgClass
    {
        public string type;
        public int id;
        public int sid;
        public int tid;
        //public long Time = (long)DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    //////////////messages to be sent/////////////
    public class ConnectMsg : MsgClass
    {
        public new string type = "connect";
        public long Time = (long)DateTimeOffset.Now.ToUnixTimeMilliseconds();   
        public string jobsAvailable = "searchAndRescue";
    }

    public class UpdateMsg : MsgClass
    {
        public new string type = "update";
        public float lat { get; set; }
        public float lng { get; set; }
        public float heading { get; set; }
        public string status { get; set; }
    }

    public class AckMsg : MsgClass
    {
        public new string type = "ack";
    }

    public class CompleteMsg : MsgClass
    {
        public new string type = "complete";
    }
    
    //////////////messages to be received/////////////
    public class ConnAckMsg : MsgClass      
 	{
        public new string type;
        public long Time = (long)DateTimeOffset.Now.ToUnixTimeMilliseconds();
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
        public string type { get; set; }    // either retrieveTarget or deliverTarget; same values required
        public MissionInfo missionInfo { get; set; }


    }

    
}
public class MissionInfo
{
    public string taskType;
    public float lat;
    public float lng;
}



