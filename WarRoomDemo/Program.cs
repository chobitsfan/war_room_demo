using System.Net.WebSockets;
using System.Text.Json;

namespace WarRoomDemo
{
    public class Vehicle
    {
        public string? vehicleUid { get; set; }
        public double[]? position { get; set; }
        public string? type { get; set; }
        public string? swarmUid { get; set; }
        public bool isLeader { get; set; } = false;
    }

    public class Status
    {
        public string? uid { get; set; }
        public DateTime timestamp { get; set; }
        public string type { get; set; } = "dgsStatus";
        public class StatusData
        {
            public string? systemUid { get; set; }
            public int ttl { get; set; }
            public List<Vehicle> vehicles { get; set; } = new List<Vehicle>();
        };
        public StatusData data { get; set; } = new StatusData();
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            /*ClientWebSocket ws = new ClientWebSocket();
            Console.WriteLine("connecting...");
            Task task = ws.ConnectAsync(new Uri("ws://120.0.0.1:55688"), CancellationToken.None);
            task.Wait();
            Console.WriteLine("connected");*/

            var status = new Status();
            status.data.vehicles.Add(new Vehicle
            {
                vehicleUid = "haha",
                position = new double[2] { 5.0, 8.0 }
            });
            Console.WriteLine(JsonSerializer.Serialize(status));
        }
    }
}