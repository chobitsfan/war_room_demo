using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Tasks;

namespace WarRoomDemo
{
    public class Vehicle
    {
        public string vehicleUid { get; set; } = "";
        public double[]? position { get; set; }
        public string type { get; set; } = "copter";
        public string? swarmUid { get; set; }
        public bool isLeader { get; set; } = false;
    }

    public class Status
    {
        public string uid { get; set; } = "";
        public DateTime timestamp { get; set; }
        public string type { get; set; } = "dgsStatus";
        public class StatusData
        {
            public string systemUid { get; set; } = "";
            public int ttl { get; set; } = 1;
            public List<Vehicle> vehicles { get; set; } = new List<Vehicle>();
        };
        public StatusData data { get; set; } = new StatusData();
    }

    internal class Program
    {
        static async Task Send(ClientWebSocket webSocket, Status status)
        {
            var jsonOpt = new JsonSerializerOptions() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };
            while (webSocket.State == WebSocketState.Open)
            {
                status.timestamp = DateTime.Now;
                Console.WriteLine("sendin status...");
                await webSocket.SendAsync(new ArraySegment<byte>(JsonSerializer.SerializeToUtf8Bytes(status, jsonOpt)), WebSocketMessageType.Text, true, CancellationToken.None);
                Console.WriteLine("status sent");
                await Task.Delay(1000);
            }
        }

        static async Task Receive(ClientWebSocket webSocket)
        {
            byte[] buffer = new byte[1024];
            while (webSocket.State == WebSocketState.Open)
            {
                await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
        }

        static void Main(string[] args)
        {
            var status = new Status();
            status.data.vehicles.Add(new Vehicle
            {
                vehicleUid = "itri-1",
                position = new double[] { 24.773252, 121.046107, 30 }
            });

            ClientWebSocket webSocket = new ClientWebSocket();
            Console.WriteLine("connecting...");
            string uri = "ws://127.0.0.1:55688/Third";
            if (args.Length > 0)
            {
                uri = args[0];
            }
            webSocket.ConnectAsync(new Uri(uri), CancellationToken.None).Wait();
            Console.WriteLine("connected");

            Task.WaitAll(Receive(webSocket), Send(webSocket, status));
        }
    }
}