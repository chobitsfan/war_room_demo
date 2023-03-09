using System;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace WarRoomDemo
{
    public static class TestConstants
    {
        public const string SystemUid = "TestSys01";
        public const string SystemName = "Demo DGS";
    }

    public class MyState
    {
        public List<Vehicle>? vehicles;
        public string? missionUid;
        public string? missionState;
        public int missionProgress = 0;
    }

    public class MissionStatus
    {
        public string uid { get; set; } = string.Empty;
        public DateTime timestamp { get; set; } = DateTime.Now;
        public string type { get; set; } = "missionStatus";
        public class MissionStatusData
        {
            public string? missionUid { get; set; }
            public string? state { get; set; }
            public class MissionSwarm
            {
                public string swarmUid { get; set; } = string.Empty;
                public int ammunition { get; set; } = 0;
            }
            public List<MissionSwarm> swarms { get; set; } = new List<MissionSwarm>() { new MissionSwarm() };
            public int progress { get; set; } = 0;
        }
    }

    public class CmdReply
    {
        public string uid { get; set; } = string.Empty;
        public DateTime timestamp { get; set; } = DateTime.Now;
        public string type { get; set; } = "commandReply";
        public class CmdReplyData
        {
            public string? replyCommand { get; set; }
            public string? missionUid { get; set; }
            public string reply { get; set; } = "accepted";
        }
        public CmdReplyData data { get; set; } = new CmdReplyData();
    }

    public class Vehicle
    {
        public string vehicleUid { get; set; } = "";
        public double[]? position { get; set; }
        public string type { get; set; } = "copter";
        public string swarmUid { get; set; } = string.Empty;
        public bool isLeader { get; set; } = false;
        public class Payload
        {
            public string type { get; set; } = "gravityBomb";
            public int amount { get; set; }
        }
        public List<Payload> payloads { get; set; } = new List<Payload>();
    }

    public class DgsStatus
    {
        public string uid { get; set; } = "";
        public DateTime timestamp { get; set; } = DateTime.Now;
        public string type { get; set; } = "dgsStatus";
        public class DgsStatusData
        {
            public string systemUid { get; set; } = TestConstants.SystemUid;
            public int ttl { get; set; } = 1;
            public List<Vehicle>? vehicles { get; set; }
        };
        public DgsStatusData data { get; set; } = new DgsStatusData();
    }

    public class QueryReply
    {
        public string uid { get; set; } = "";
        public DateTime timestamp { get; set; } = DateTime.Now;
        public string type { get; set; } = "queryReply";
        public class QueryReplyData
        {
            public string replyContent { get; set; } = "system";
            public string replyTo { get; set; } = "";
            public string systemUid { get; set; } = TestConstants.SystemUid;
            public string name { get; set; } = TestConstants.SystemName;
            public List<Vehicle>? vehicles { get; set; }
        }
        public QueryReplyData data { get; set;} = new QueryReplyData();
    }

    public class WarRoomPkt
    {
        public string uid { get; set; } = string.Empty;
        public DateTime timestamp { get; set; }
        public string type { get; set; } = string.Empty;
        public JsonElement data { get; set; }
    }

    internal class Program
    {
        static async Task Send(ClientWebSocket webSocket, MyState myState)
        {
            var jsonOpt = new JsonSerializerOptions() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };
            while (webSocket.State == WebSocketState.Open)
            {
                DgsStatus dgsStatus = new DgsStatus();
                dgsStatus.data.vehicles = myState.vehicles;
                Console.WriteLine("sending dgs status...");
                await webSocket.SendAsync(new ReadOnlyMemory<byte>(JsonSerializer.SerializeToUtf8Bytes(dgsStatus, jsonOpt)), WebSocketMessageType.Text, true, CancellationToken.None);
                Console.WriteLine("dgs status sent");
                if (myState.missionUid != null)
                {
                    var missionStatus = new MissionStatus();
                    Console.WriteLine("sending mission status...");
                    await webSocket.SendAsync(new ReadOnlyMemory<byte>(JsonSerializer.SerializeToUtf8Bytes(missionStatus, jsonOpt)), WebSocketMessageType.Text, true, CancellationToken.None);
                    Console.WriteLine("dgs status sent");
                }
                await Task.Delay(1000);
            }
        }

        static async Task Receive(ClientWebSocket webSocket, MyState myState)
        {
            var jsonOpt = new JsonSerializerOptions() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };
            Memory<byte> buf = new Memory<byte>(new byte[4096]);
            while (webSocket.State == WebSocketState.Open)
            {
                var result =  await webSocket.ReceiveAsync(buf, CancellationToken.None);
                string str = System.Text.Encoding.UTF8.GetString(buf.ToArray(), 0, result.Count);
                Console.WriteLine("rcv msg:" + str);
                WarRoomPkt? warRoomPkt = null;
                try
                {
                    warRoomPkt = JsonSerializer.Deserialize<WarRoomPkt>(str);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
                if (warRoomPkt != null)
                {
                    Console.WriteLine("rcv " + warRoomPkt.type);
                    if (warRoomPkt.type == "query")
                    {
                        var queryContent = warRoomPkt.data.GetProperty("queryContent").GetString();
                        if (queryContent == "system")
                        {
                            Console.WriteLine("rcv system query");
                            var reply = new QueryReply();
                            reply.data.replyTo = warRoomPkt.uid;
                            reply.data.vehicles = myState.vehicles;
                            await webSocket.SendAsync(new ReadOnlyMemory<byte>(JsonSerializer.SerializeToUtf8Bytes(reply, jsonOpt)), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                    else if (warRoomPkt.type == "command")
                    {
                        var cmd = warRoomPkt.data.GetProperty("command").GetString();
                        var missionUid = warRoomPkt.data.GetProperty("missionUid").GetString();
                        myState.missionUid = missionUid;
                        if (cmd == "assign")
                        {
                            Console.WriteLine("rcv assign");
                            myState.missionState = "ready";
                            myState.missionProgress = 0;
                            var tgts = warRoomPkt.data.GetProperty("targets");
                            foreach (var tgt in tgts.EnumerateArray())
                            {
                                if (tgt.GetProperty("type").GetString() == "position")
                                {

                                }
                            }
                        }
                        else if (cmd == "engage")
                        {

                        }
                        var reply = new CmdReply();
                        reply.data.replyCommand = cmd;
                        reply.data.missionUid = missionUid;
                        await webSocket.SendAsync(new ReadOnlyMemory<byte>(JsonSerializer.SerializeToUtf8Bytes(reply, jsonOpt)), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            List<Vehicle> vehicles = new()
            {
                new Vehicle
                {
                    vehicleUid = "itri-1",
                    position = new double[] { 24.773252, 121.046107, 30 }
                }
            };
            var myState = new MyState();
            myState.vehicles = vehicles;

            ClientWebSocket webSocket = new ClientWebSocket();
            Console.WriteLine("connecting...");
            string uri = "ws://127.0.0.1:55688/Third";
            if (args.Length > 0)
            {
                uri = args[0];
            }
            webSocket.ConnectAsync(new Uri(uri), CancellationToken.None).Wait();
            Console.WriteLine("connected");

            Task.WaitAll(Receive(webSocket, myState), Send(webSocket, myState));
        }
    }
}