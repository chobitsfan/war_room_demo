﻿using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WarRoomDemo
{
    public static class DGSSetting
    {
        public const string BaseUid = "TestSys";
        public const string BaseName = "Demo DGS";


        public static string SystemUid => BaseUid + (string.IsNullOrEmpty(SystemPostfix) ? "" : "-" + SystemPostfix);
        public static string SystemName => BaseName + (string.IsNullOrEmpty(SystemPostfix) ? "" : " " + SystemPostfix);

        public static string SystemPostfix { get; set; } = "";

        public const double DefaultLatitude = 24.773252;
        public const double DefaultLongitude = 121.046107;
    }

    public class SwarmMission
    {
        public AttackTarget target = new();
        public int count;
        public double dLat = 0;
        public double dLon = 0;
        public int progress = 0;
    }

    public class MyState
    {
        public readonly List<Vehicle> vehicles = new List<Vehicle>();
        public string? missionUid;
        public string? missionState;
        public int missionProgress;
        public double[]? tgtPos;
        public AttackTarget[] targets = Array.Empty<AttackTarget>();

        public readonly List<string> swarms = new();
        public readonly List<List<Vehicle>> swarmVehicles = new();
        public readonly List<List<SwarmMission>> swarmMissions = new();
        public readonly List<int> swarmCurrentMission = new();
        public int totalSwarmMissions;

        public int missionFadeCounter = 3;
        public bool initialized;
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
            public string? snapshot { get; set; } = "data:image/jpeg;base64,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";

            public string? vehicleUid { get; set; }
            public string? targetUid { get; set; }
            public int? count { get; set; }
        }
        public MissionEventData data { get; set; }

        public MissionEvent(string mUid, double[]? pos)
        {
            data = new MissionEventData
            {
                missionUid = mUid,
                position = pos
            };
        }
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
                public string swarmUid { get; set; } = "";
                public int ammunition { get; set; }
                public ReportAttackTarget? target { get; set; }
            }
            public List<MissionSwarm> swarms { get; set; } = new List<MissionSwarm>();
            public int progress { get; set; } = 0;

            public MissionStatusData(MyState state)
            {
                missionUid = state.missionUid;
                this.state = state.missionState;
                int totalSubmissionProgress = 0;
                for (int i = 0; i < state.swarms.Count; i++)
                {
                    if (state.swarmCurrentMission[i] < state.swarmMissions[i].Count)
                    {
                        swarms.Add(new MissionSwarm
                        {
                            swarmUid = state.swarms[i],
                            ammunition = state.swarmVehicles[i].Sum(v => v.payloads.Sum(p => p.amount)),
                            target = new ReportAttackTarget(state.swarmMissions[i][state.swarmCurrentMission[i]].target)
                        });
                    }
                    totalSubmissionProgress += state.swarmMissions[i].Sum(m => m.progress);
                }
                progress = (int)((totalSubmissionProgress + 0.1) / state.totalSwarmMissions);
                if (progress > 100) progress = 100;
            }
        }
        public MissionStatusData data { get; set; }

        public MissionStatus(MyState state)
        {
            data = new MissionStatusData(state);
            state.missionProgress = data.progress;
        }
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
            public string? description { get; set; }
        }
        public CmdReplyData data { get; set; } = new CmdReplyData();
    }

    public class Vehicle
    {
        public string vehicleUid { get; set; } = "";
        public double[] position { get; set; } = new double[3];
        public string type { get; set; } = "";
        public string swarmUid { get; set; } = DGSSetting.SystemUid + "-swarm-01";
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
            public string systemUid { get; set; } = DGSSetting.SystemUid;
            public int ttl { get; set; } = 1;
            public List<Vehicle>? vehicles { get; set; }
        };
        public DgsStatusData data { get; set; } = new DgsStatusData();
    }

    public class QuerySystemReply
    {
        public string uid { get; set; } = Guid.NewGuid().ToString();
        public DateTime timestamp { get; set; } = DateTime.Now;
        public string type { get; set; } = "queryReply";
        public class QuerySystemReplyData
        {
            public string replyContent { get; set; } = "system";
            public string replyTo { get; set; } = "";
            public string systemUid { get; set; } = DGSSetting.SystemUid;
            public string name { get; set; } = DGSSetting.SystemName;
            public List<Vehicle>? vehicles { get; set; }
            public bool requestDGSStatus { get; set; } = Program.requestDGSStatus;
            public bool requestReconResults { get; set; } = Program.requestReconResults;
        }
        public QuerySystemReplyData data { get; set;} = new QuerySystemReplyData();
    }

    public class WarRoomPkt
    {
        public string uid { get; set; } = string.Empty;
        public DateTime timestamp { get; set; }
        public string type { get; set; } = string.Empty;
        public JsonElement data { get; set; }
    }

    public class AttackTarget
    {
        public string action { get; set; } = "";
        public int count { get; set; }
        public string type { get; set; } = "";
        public string? targetUid { get; set; }
        public double[]? position { get; set; }
        public double[][][]? areas { get; set; }
    }

    public class ReportAttackTarget
    {
        public string type { get; set; }
        public string? targetUid { get; set; }
        public double[]? position { get; set; }
        public double[][][]? areas { get; set; }

        public ReportAttackTarget(AttackTarget target)
        {
            type = target.type;
            targetUid = target.targetUid;
            position = target.position;
            areas = target.areas;
        }
    }

    public class ReconTarget
    {
        public string objectUid { get; set; } = "";
        public double[] position { get; set; } = new double[3];
        public string entityType { get; set; } = "ship";

        public double[] initialPosition = new double[3];
        public double[] moveStep = new double[3];
        public int movePeriod = 10;
        public int hidePeriod = 5;
        public int moveCounter;
        public int hideCounter;

        private void ResetPos()
        {
            for (int i = 0; i < 3; i++)
                position[i] = initialPosition[i];
            moveCounter = movePeriod;
        }

        public void Move()
        {
            if (hideCounter > 0)
            {
                if (--hideCounter == 0)
                {
                    ResetPos();
                }
            }
            else if (moveCounter > 0)
            {
                if (--moveCounter == 0)
                {
                    hideCounter = hidePeriod;
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                        position[i] += moveStep[i];
                }
            }
            else
            {
                ResetPos();
            }
        }
    }

    public class ReconResult
    {
        public string uid { get; set; } = Guid.NewGuid().ToString();
        public DateTime timestamp { get; set; } = DateTime.Now;
        public string type { get; set; } = "reconResult";
        public class ReconResultData
        {
            public string systemUid { get; set; } = DGSSetting.SystemUid;
            public int ttl { get; set; } = 1;
            public List<ReconTarget>? results { get; set; } = reconTargets?.Where(t => t.moveCounter > 0).ToList();
        };
        public ReconResultData data { get; set; } = new ReconResultData();

        public static List<ReconTarget>? reconTargets;

        public static bool reportRecon;
    }

    internal class Program
    {
        public class ProgressParameter
        {
            public int step { get; set; }
            public int lead { get; set; }
            public int tail => 100 - lead;
            public int segments => tail / step;
        }

        static ProgressParameter[] progresses = new[]
        {
            new ProgressParameter { step = 4, lead = 20},
            new ProgressParameter { step = 5, lead = 20},
            new ProgressParameter { step = 10, lead = 20}
        };

        static JsonSerializerOptions DefaultJsonOpt = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

        static ClientWebSocket? websocket;
        static async Task SendInJson<T>(T obj, JsonSerializerOptions? opt = null)
        {
            if (websocket != null)
            {
                await websocket.SendAsync(new ReadOnlyMemory<byte>(JsonSerializer.SerializeToUtf8Bytes(obj, opt ?? DefaultJsonOpt)), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        static async Task Send(MyState myState)
        {
            while (websocket?.State == WebSocketState.Open)
            {
                DgsStatus dgsStatus = new DgsStatus();
                dgsStatus.data.vehicles = myState.vehicles;
                await SendInJson(dgsStatus);
                if (myState.missionUid != null)
                {
                    var missionStatus = new MissionStatus(myState);
                    await SendInJson(missionStatus);
                    Console.WriteLine("mission status sent, progress " + missionStatus.data.progress);
                    if (myState.missionState == "engaging")
                    {
                        if (myState.missionProgress >= 100)
                        {
                            myState.missionState = "completed";
                        }
                        else
                        {
                            for (int i = 0; i < myState.swarms.Count; ++i)
                            {
                                if (myState.swarmCurrentMission[i] < myState.swarmMissions[i].Count)
                                {
                                    var m = myState.swarmMissions[i][myState.swarmCurrentMission[i]];
                                    if (m.progress < 100)
                                    {
                                        var pp = progresses[i % 3];
                                        int prog = (m.progress += pp.step);
                                        foreach (var v in myState.swarmVehicles[i])
                                        {
                                            if (v.isLeader && prog <= pp.tail
                                                || !v.isLeader && prog >= pp.lead)
                                            {
                                                v.position[0] += m.dLat / pp.segments;
                                                v.position[1] += m.dLon / pp.segments;
                                            }
                                        }
                                        if (m.progress >= 100)
                                        {
                                            async Task SendAttackEvent(Vehicle v, int count)
                                            {
                                                var evt = new MissionEvent(myState.missionUid, m.target.position);
                                                evt.data.vehicleUid = v.vehicleUid;
                                                evt.data.targetUid = m.target.targetUid;
                                                if (m.count > 1) evt.data.count = m.count;
                                                await SendInJson(evt);
                                                Console.WriteLine("mission event sent");
                                            }

                                            _ = myState.swarmVehicles[i].Any(v => v.payloads.Where(p =>
                                                p.type == "guidedBomb" && m.target.action == "snipe" ||
                                                p.type == "gravityBomb" && m.target.action == "bombard").Any(p =>
                                                {
                                                    int count = p.amount > m.count ? m.count : p.amount;
                                                    Task.Run(() => SendAttackEvent(v, count));
                                                    m.count -= count;
                                                    p.amount -= count;
                                                    return m.count == 0;
                                                }));
                                            myState.swarmCurrentMission[i]++;
                                            Console.WriteLine("\t\t swarm " + i + " attacks");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (myState.missionState == "finished"
                        || myState.missionState == "canceled"
                        || myState.missionState == "completed")
                    {
                        if (--myState.missionFadeCounter < 0)
                        {
                            myState.missionFadeCounter = 3;
                            myState.missionUid = null;
                        }
                    }
                }
                if (ReconResult.reportRecon)
                {
                    void resetPos(ReconTarget t)
                    {
                        for (int i = 0; i < 3; i++)
                            t.position[i] = t.initialPosition[i];
                        t.moveCounter = t.movePeriod;
                    }

                    void movePos(ReconTarget t)
                    {
                        for (int i = 0; i < 3; i++)
                            t.position[i] += t.moveStep[i];
                    }

                    ReconResult.reconTargets!.ForEach(t =>
                    {
                        if (t.hideCounter > 0)
                        {
                            if (--t.hideCounter == 0)
                            {
                                resetPos(t);
                            }
                        }
                        else if (t.moveCounter > 0)
                        {
                            if (--t.moveCounter == 0)
                            {
                                if (t.hidePeriod > 0)
                                {
                                    t.hideCounter = t.hidePeriod;
                                }
                                else
                                {
                                    resetPos(t);
                                }
                            }
                            else
                            {
                                movePos(t);
                            }
                        }
                        else
                        {
                            resetPos(t);
                        }
                    });

                    await SendInJson(new ReconResult());
                }
                await Task.Delay(1000);
            }
        }

        static async Task Receive(MyState myState)
        {
            Memory<byte> buf = new Memory<byte>(new byte[4096]);
            while (websocket?.State == WebSocketState.Open)
            {
                var result =  await websocket.ReceiveAsync(buf, CancellationToken.None);
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
                            var reply = new QuerySystemReply();
                            reply.data.replyTo = warRoomPkt.uid;
                            reply.data.vehicles = myState.vehicles;
                            await SendInJson(reply);
                            Console.WriteLine("reply query");
                            if (!myState.initialized &&
                                initLat == DGSSetting.DefaultLatitude && initLng == DGSSetting.DefaultLongitude
                                && warRoomPkt.data.TryGetProperty("location", out var json))
                            {
                                var loc = json.Deserialize<double[]>();
                                if (loc != null)
                                {
                                    double latoff = loc[0] - initLat;
                                    double lngoff = loc[1] - initLng;
                                    myState.vehicles.ForEach(v =>
                                    {
                                        v.position[0] += latoff;
                                        v.position[1] += lngoff;
                                    });
                                }
                            }
                            ReconResult.reconTargets?.ForEach(t =>
                            {
                                var ip = t.initialPosition;
                                if (ip[0] == 0 && ip[1] == 0)
                                {
                                    ip[0] = initLat;
                                    ip[1] = initLng;
                                }
                                ReconResult.reportRecon = true;
                            });
                            myState.initialized = true;
                        }
                        else if (queryContent == "fence")
                        {
                            //
                        }
                    }
                    else if (warRoomPkt.type == "command")
                    {
                        string? rejectReason = "";
                        var cmd = warRoomPkt.data.GetProperty("command").GetString();
                        var missionUid = warRoomPkt.data.GetProperty("missionUid").GetString();
                        if (cmd == "assign")
                        {
                            if (myState.missionUid == null && missionUid != null)
                                rejectReason = VerifyMissionAssignment(myState, warRoomPkt, missionUid);
                            else if (myState.missionUid == missionUid)
                            {
                                // already assigned, reply with ack
                                rejectReason = null;
                            }
                            else //if (myState.missionUid != missionUid)
                            {
                                rejectReason = "System is busy.";
                            }
                        }
                        else if (myState.missionUid == missionUid)
                        {
                            if (cmd == "engage")
                            {
                                Console.WriteLine("mission engage");
                                myState.missionState = "engaging";
                                rejectReason = null;
                            }
                            else if (cmd == "terminate")
                            {
                                Console.WriteLine("mission terminate");
                                if (myState.missionState == "completed") myState.missionState = "finished"; else myState.missionState = "canceled";
                                rejectReason = null;
                            }

                        }

                        if (rejectReason?.Length != 0)
                        {
                            // null: accept, nonnull & nonempty: reject
                            var reply = new CmdReply
                            {
                                data = new CmdReply.CmdReplyData
                                {
                                    replyCommand = cmd,
                                    missionUid = missionUid,
                                    reply = rejectReason == null ? "accepted" : "rejected",
                                    description = rejectReason
                                }
                            };
                            await SendInJson(reply);
                        }
                    }
                }
            }
        }

        private static string? VerifyMissionAssignment(MyState myState, WarRoomPkt warRoomPkt, string missionUid)
        {
            var targets = warRoomPkt.data.GetProperty("targets").Deserialize<AttackTarget[]>() ?? Array.Empty<AttackTarget>();

            int guided = 0, gravity = 0, others = 0;
            var swarms = myState.swarms;
            int nswarms = swarms.Count;
            int[] sguided = new int[nswarms], sgravity = new int[nswarms];

            myState.swarmCurrentMission.Clear();
            for (int i = 0; i < nswarms; i++)
            {
                var s = swarms[i];
                var vehicles = myState.vehicles.Where(v => v.swarmUid == s);
                myState.swarmMissions[i].Clear();
                myState.swarmCurrentMission.Add(0);
                sguided[i] = vehicles.Sum(v => v.payloads.Where(p => p.type.ToLower() == "guidedbomb").Sum(p => p.amount));
                sgravity[i] = vehicles.Sum(v => v.payloads.Where(p => p.type.ToLower() == "gravitybomb").Sum(p => p.amount));

            }
            int myguided = sguided.Sum(), mygravity = sgravity.Sum();

            foreach (var t in targets)
            {
                if (t.position == null) return "Target not supported";

                if (t.action == "snipe") guided += t.count;
                else if (t.action == "bombard") gravity += t.count;
                else others += t.count;
            }

            if (guided == 0 && gravity == 0 || gravity < 0 || guided < 0 || others < 0)
            {
                return "Attack count invalid.";
            }

            if (guided > myguided || gravity > mygravity || others > 0)
            {
                return "Attack count exceed capacity.";
            }

            Console.WriteLine("mission assign");
            myState.missionProgress = 0;
            myState.targets = targets;

            int sptr = 0;
            foreach (var t in targets)
            {
                bool fillQuota(ref int quota, ref int stock, int swarmno, AttackTarget target)
                {
                    if (stock == 0) return false;
                    var missions = myState.swarmMissions[swarmno];
                    double[] lastpos = missions.Count > 0 ? missions.Last().target.position! :
                                                        myState.swarmVehicles[swarmno].First().position;
                    var m = new SwarmMission
                    {
                        target = target,
                        count = quota > stock ? stock : quota,
                        dLat = target.position![0] - lastpos[0],
                        dLon = target.position![1] - lastpos[1],
                    };
                    missions.Add(m);
                    quota -= m.count;
                    stock -= m.count;
                    return true;
                }

                int amount = t.count;
                while (amount > 0)
                {
                    if (t.action == "snipe")
                    {
                        fillQuota(ref amount, ref sguided[sptr], sptr, t);
                    }
                    else if (t.action == "bombard")
                    {
                        fillQuota(ref amount, ref sgravity[sptr], sptr, t);
                    }
                    else
                    {
                        return "Unsuported attack type: " + t.action;
                    }
                    if (++sptr >= nswarms) sptr -= nswarms;
                }
            }
            myState.missionUid = missionUid;
            myState.missionState = "ready";
            myState.totalSwarmMissions = myState.swarmMissions.Sum(s => s.Count);

            return null;
        }

        static double initLat = DGSSetting.DefaultLatitude;
        static double initLng = DGSSetting.DefaultLongitude;

        static int swarmSN = 0;
        static int vehicleSN = 0;
        static int objectSN = 0;

        public static bool requestDGSStatus;
        public static bool requestReconResults;

        static void AddSwarm(MyState myState, int size, string[] vtypes, string[] ptypes, int[] pamounts, double latoff, double lngoff)
        {
            double lat = initLat + latoff * swarmSN;
            string swarmUid = $"{DGSSetting.SystemUid}-swarm-{++swarmSN:00}";
            myState.swarms.Add(swarmUid);
            string vtype = vtypes[0], ptype = ptypes[0];
            int pamount = pamounts[0];

            var leader = new Vehicle
            {
                vehicleUid = $"{DGSSetting.SystemUid}-drone-{++vehicleSN:00}",
                swarmUid = swarmUid,
                type = vtype,
                position = new double[] { lat, initLng + vehicleSN * lngoff, 30 },
                isLeader = true,
                payloads = new List<Vehicle.Payload>
                {
                    new Vehicle.Payload
                    {
                        type = ptype,
                        amount = pamount,
                    }
                },
            };
            myState.vehicles.Add(leader);

            var swarmvehicles = new List<Vehicle> { leader };

            for (int i = 0; ++i < size; )
            {
                if (i < vtypes.Length) vtype = vtypes[i];
                if (i < ptypes.Length) ptype = ptypes[i];
                if (i < pamounts.Length) pamount = pamounts[i];
                var wingman = new Vehicle
                {
                    vehicleUid = $"{DGSSetting.SystemUid}-drone-{++vehicleSN:00}",
                    type = vtype,
                    swarmUid = swarmUid,
                    position = new double[] { lat, initLng + vehicleSN * lngoff, 35 },
                    payloads = new List<Vehicle.Payload> { new Vehicle.Payload { type = ptype, amount = pamount } }
                };
                myState.vehicles.Add(wingman);
                swarmvehicles.Add(wingman);
            }

            myState.swarmVehicles.Add(swarmvehicles);
            myState.swarmMissions.Add(new List<SwarmMission>());
        }

        static void Main(string[] args)
        {
            int argptr = 0, arglen = args.Length, swarms = 1, swarmsize = 2;
            double latoff = 0.001, lngoff = 0.0005;
            string uri;
            string[] vehicletypes = { "copter" }, payloadtypes = { "guidedBomb", "gravityBomb" };
            List<string> entityTypes = new();
            int[] payloadamounts = { 1 };
            if (arglen > 0)
            {
                uri = args[0];
            }
            else
            {
                Console.WriteLine("Usage: WarRoomDemo <server uri> [dgs postfix] [[-switch arg] ...]");
                return;
            }
            if (arglen > 1 && !args[1].StartsWith('-'))
            {
                DGSSetting.SystemPostfix = args[1];
                argptr++;
            }
            while (++argptr < arglen)
            {
                string opt = args[argptr];
                if (opt[0] == '-' && ++argptr < arglen)
                {
                    string arg = args[argptr];
                    switch (opt.ToLower())
                    {
                        case "-dronetype":
                        case "-dronetypes":
                        case "-vtype":
                        case "-vtypes":
                        case "-vehicletype":
                        case "-vehicletypes":
                            vehicletypes = arg.ToLower().Split(',');
                            break;
                        case "-ptype":
                        case "-ptypes":
                        case "-payloadtype":
                        case "-payloadtypes":
                            payloadtypes = arg.ToLower().Split(',');
                            break;
                        case "-pamount":
                        case "-pamounts":
                        case "-payloadamount":
                        case "-payloadamounts":
                            try
                            {
                                var amounts = arg.ToLower().Split(',').Select(
                                    s => int.TryParse(s, out int a) ? a : 0);
                                payloadamounts = amounts.ToArray();
                            }
                            catch { }
                            break;
                        case "-swarms":
                            int.TryParse(arg, out swarms);
                            break;
                        case "-swarmsize":
                            int.TryParse(arg, out swarmsize);
                            break;
                        case "-latoff":
                        case "-latoffset":
                        case "-latittudeoffset":
                            double.TryParse(arg, out latoff);
                            break;
                        case "-lngoff":
                        case "-lngoffset":
                        case "-longitudeoffset":
                            double.TryParse(arg, out lngoff);
                            break;
                        case "-loc":
                        case "-location":
                        case "-startlocation":
                        case "-startloc":
                            try
                            {
                                var loc = arg.Split(',');
                                var lat = double.Parse(loc[0]);
                                var lng = double.Parse(loc[1]);
                                initLat = lat;
                                initLng = lng;
                            }
                            catch { }
                            break;
                        case "-reqdgs":
                        case "-requestdgs":
                        case "-requestdgsstatus":
                            requestDGSStatus = true;
                            break;
                        case "-reqrecon":
                        case "-requestrecon":
                        case "-requestreconresults":
                            requestReconResults = true;
                            break;
                        case "-recon":
                            try
                            {
                                string[] argsplit = arg.Split(',');
                                if (argsplit.Length < 8) break;
                                int mp = int.Parse(argsplit[0]);
                                int hp = int.Parse(argsplit[1]);
                                double[] ip = new double[] { double.Parse(argsplit[2]), double.Parse(argsplit[3]), double.Parse(argsplit[4]) };
                                double[] step = new double[] { double.Parse(argsplit[5]), double.Parse(argsplit[6]), double.Parse(argsplit[7]) };
                                ReconResult.reconTargets ??= new List<ReconTarget>();
                                ReconResult.reconTargets.Add(new ReconTarget
                                {
                                    objectUid = $"{DGSSetting.SystemUid}-Object-{++objectSN}",
                                    movePeriod = mp,
                                    hidePeriod = hp,
                                    initialPosition = ip,
                                    moveStep = step
                                }); ;
                            }
                            catch
                            {
                                Console.WriteLine("Invalid recon parameter: expected steps,hidesteps,initlat,initlng,initalt,steplat,steplng,stepalt got: " + arg);
                            }
                            break;
                        case "-entitytype":
                        case "-targettype":
                        case "-ttype":
                        case "-etype":
                        case "-entitytypes":
                        case "-targettypes":
                        case "-ttypes":
                        case "-etypes":
                            entityTypes = arg.Split(',').ToList();
                            break;
                        default:
                            --argptr;
                            break;
                    }
                }
            }

            if (ReconResult.reconTargets != null)
            {
                string etype = "boat";
                for (int i = 0; i < ReconResult.reconTargets.Count; i++)
                {
                    if (i < entityTypes.Count) etype = entityTypes[i];
                    ReconResult.reconTargets[i].entityType = etype;
                }
            }

            var myState = new MyState();
            while (swarms-- > 0)
            {
                AddSwarm(myState, swarmsize, vehicletypes, payloadtypes, payloadamounts, latoff, lngoff);
            }

            websocket = new ClientWebSocket();
            //webSocket.Options.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2("D:\\戰情中心\\certs\\ccclient.pfx", "ccclient"));
            Console.WriteLine("connecting...");

            try
            {
                websocket.ConnectAsync(new Uri(uri), CancellationToken.None).Wait();
                Console.WriteLine("connected");
                Task.WaitAll(Receive(myState), Send(myState));
            }
            catch { }
        }
    }
}