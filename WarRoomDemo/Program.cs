using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Tasks;

namespace WarRoomDemo
{
    public static class TestConstants
    {
        public const string SystemUid = "TestSys01";
        public const string SystemName = "Demo DGS";
    }

    public class Vehicle
    {
        public string vehicleUid { get; set; } = "";
        public double[]? position { get; set; }
        public string type { get; set; } = "copter";
        public string? swarmUid { get; set; }
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
        static async Task Send(ClientWebSocket webSocket, List<Vehicle> vehicles)
        {
            var jsonOpt = new JsonSerializerOptions() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };
            while (webSocket.State == WebSocketState.Open)
            {
                DgsStatus dgsStatus = new DgsStatus();
                dgsStatus.data.vehicles = vehicles;
                Console.WriteLine("sendin status...");
                await webSocket.SendAsync(new ReadOnlyMemory<byte>(JsonSerializer.SerializeToUtf8Bytes(dgsStatus, jsonOpt)), WebSocketMessageType.Text, true, CancellationToken.None);
                Console.WriteLine("status sent");
                await Task.Delay(1000);
            }
        }

        static async Task Receive(ClientWebSocket webSocket, List<Vehicle> vehicles)
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
                        var queryContent = warRoomPkt.data.GetProperty("queryContent");
                        if (queryContent.GetString() == "system")
                        {
                            Console.WriteLine("rcv system query");
                            QueryReply queryReply = new QueryReply();
                            queryReply.data.replyTo = warRoomPkt.uid;
                            queryReply.data.vehicles = vehicles;
                            await webSocket.SendAsync(new ReadOnlyMemory<byte>(JsonSerializer.SerializeToUtf8Bytes(queryReply, jsonOpt)), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
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

            ClientWebSocket webSocket = new ClientWebSocket();
            Console.WriteLine("connecting...");
            string uri = "ws://127.0.0.1:55688/Third";
            if (args.Length > 0)
            {
                uri = args[0];
            }
            webSocket.ConnectAsync(new Uri(uri), CancellationToken.None).Wait();
            Console.WriteLine("connected");

            Task.WaitAll(Receive(webSocket, vehicles), Send(webSocket, vehicles));
        }
    }
}