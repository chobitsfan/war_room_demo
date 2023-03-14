using System;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        public List<Vehicle> vehicles = new List<Vehicle>();
        public string? missionUid;
        public string? missionState;
        public int missionProgress = 0;
        public double[]? tgtPos;
    }

    public class MissionEvent
    {
        public string uid { get; set; } = Guid.NewGuid().ToString();
        public DateTime timestamp { get; set; } = DateTime.Now;
        public string type { get; set; } = "missionEvent";
        public class MissionEventData
        {
            public string? missionUid { get; set; }
            [JsonPropertyName("event")]
            public string evt { get; set; } = "missionAttack";
            public double[]? position { get; set; }
            public string? snapshot { get; set; }
        }
        public MissionEventData data { get; set; } = new MissionEventData();
    }

    public class MissionStatus
    {
        public string uid { get; set; } = Guid.NewGuid().ToString();
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
        public MissionStatusData data { get; set; } = new MissionStatusData();
    }

    public class CmdReply
    {
        public string uid { get; set; } = Guid.NewGuid().ToString();
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
        public double[] position { get; set; } = new double[3];
        public string type { get; set; } = "copter";
        public string swarmUid { get; set; } = string.Empty;
        public bool isLeader { get; set; } = false;
        public class Payload
        {
            public string type { get; set; } = "gravityBomb";
            public int amount { get; set; }
        }
        public List<Payload> payloads { get; set; } = new List<Payload>();
        public double[] moveStep = new double[3];
    }

    public class DgsStatus
    {
        public string uid { get; set; } = Guid.NewGuid().ToString();
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
        public string uid { get; set; } = Guid.NewGuid().ToString();
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
                await webSocket.SendAsync(new ReadOnlyMemory<byte>(JsonSerializer.SerializeToUtf8Bytes(dgsStatus, jsonOpt)), WebSocketMessageType.Text, true, CancellationToken.None);
                if (myState.missionUid != null)
                {
                    var missionStatus = new MissionStatus();
                    missionStatus.data.missionUid = myState.missionUid;
                    missionStatus.data.progress = myState.missionProgress;
                    missionStatus.data.state = myState.missionState;
                    await webSocket.SendAsync(new ReadOnlyMemory<byte>(JsonSerializer.SerializeToUtf8Bytes(missionStatus, jsonOpt)), WebSocketMessageType.Text, true, CancellationToken.None);
                    Console.WriteLine("mission status sent, progress " + missionStatus.data.progress);
                    if (myState.missionState == "engaging")
                    {
                        myState.missionProgress += 5;
                        if (myState.missionProgress >= 100)
                        {
                            myState.missionProgress = 100;
                            myState.missionState = "completed";
                            var missionEvt = new MissionEvent();
                            missionEvt.data.missionUid = myState.missionUid;
                            missionEvt.data.position = myState.tgtPos;
                            missionEvt.data.snapshot = "data:image/jpeg;base64,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";
                            await webSocket.SendAsync(new ReadOnlyMemory<byte>(JsonSerializer.SerializeToUtf8Bytes(missionEvt, jsonOpt)), WebSocketMessageType.Text, true, CancellationToken.None);
                            Console.WriteLine("mission event sent");
                        }
                        else
                        {
                            foreach (var vehicle in myState.vehicles)
                            {
                                vehicle.position[0] += vehicle.moveStep[0];
                                vehicle.position[1] += vehicle.moveStep[1];
                            }
                        }
                    }
                    else if (myState.missionState == "finished" || myState.missionState == "canceled")
                    {
                        myState.missionUid = null;
                    }
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
                //string str = System.Text.Encoding.UTF8.GetString(buf.ToArray(), 0, result.Count);
                //Console.WriteLine("rcv msg:" + str);
                WarRoomPkt? warRoomPkt = null;
                try
                {
                    warRoomPkt = JsonSerializer.Deserialize<WarRoomPkt>(buf.Span[..result.Count]);
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
                            var reply = new QueryReply();
                            reply.data.replyTo = warRoomPkt.uid;
                            reply.data.vehicles = myState.vehicles;
                            await webSocket.SendAsync(new ReadOnlyMemory<byte>(JsonSerializer.SerializeToUtf8Bytes(reply, jsonOpt)), WebSocketMessageType.Text, true, CancellationToken.None);
                            Console.WriteLine("reply query");
                        }
                    }
                    else if (warRoomPkt.type == "command")
                    {
                        var cmd = warRoomPkt.data.GetProperty("command").GetString();
                        var missionUid = warRoomPkt.data.GetProperty("missionUid").GetString();
                        myState.missionUid = missionUid;
                        if (cmd == "assign")
                        {
                            Console.WriteLine("mission assign");
                            myState.missionState = "ready";
                            myState.missionProgress = 0;
                            var tgts = warRoomPkt.data.GetProperty("targets");
                            foreach (var tgt in tgts.EnumerateArray())
                            {
                                if (tgt.GetProperty("type").GetString() == "position")
                                {
                                    var tgtPos = tgt.GetProperty("position").Deserialize<double[]>();
                                    if (tgtPos != null)
                                    {
                                        Console.WriteLine("tgt pos " + string.Join(",", tgtPos));
                                        myState.tgtPos = tgtPos;
                                        foreach (var vehicle in myState.vehicles)
                                        {
                                            vehicle.moveStep[0] = (tgtPos[0] - vehicle.position[0]) / 20.0;
                                            vehicle.moveStep[1] = (tgtPos[1] - vehicle.position[1]) / 20.0;
                                        }
                                    }
                                }
                            }
                        }
                        else if (cmd == "engage")
                        {
                            Console.WriteLine("mission engage");
                            myState.missionState = "engaging";
                            myState.missionProgress = 0;
                        }
                        else if (cmd == "terminate")
                        {
                            Console.WriteLine("mission terminate");
                            if (myState.missionState == "completed") myState.missionState = "finished"; else myState.missionState = "canceled";
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
                },
                new Vehicle
                {
                    vehicleUid = "itri-2",
                    position = new double[] { 24.773292, 121.046157, 35 }
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