using System.Net.WebSockets;
using System.Text.Json;

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
        static void Main(string[] args)
        {
            ClientWebSocket ws = new ClientWebSocket();
            Console.WriteLine("connecting...");
            string uri = "ws://127.0.0.1:55688/Third";
            if (args.Length > 0)
            {
                uri = args[0];
            }
            Task task = ws.ConnectAsync(new Uri(uri), CancellationToken.None);
            task.Wait();
            Console.WriteLine("connected");

            var status = new Status();
            status.data.vehicles.Add(new Vehicle
            {
                vehicleUid = "itri-1",
                position = new double[] { 24.773252, 121.046107, 30 }
            });
            while (true)
            {
                status.timestamp = DateTime.Now;
                Console.WriteLine("sending...");
                task=  ws.SendAsync(new ArraySegment<byte>(JsonSerializer.SerializeToUtf8Bytes(status)), WebSocketMessageType.Text, true , CancellationToken.None);
                task.Wait();
                Console.WriteLine("sent");
                //Console.WriteLine(JsonSerializer.Serialize(status));
                Thread.Sleep(1000);
            }
        }
    }
}