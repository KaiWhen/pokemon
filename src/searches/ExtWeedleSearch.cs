using System.Linq;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

using static SearchCommon;
using static RbyIGTChecker<Red>;
using System.Text.Json;
using System.Net.Http.Headers;

class ExtWeedleSearch
{
    const string NidoPath = "LLLULLUAULALDLDLLDADDADLALLALUUAU";
    const string BasePath = "DRRUUURRRRRRRRRRRRRRRRRRRRRUR";
    const string BasePathToGirl = BasePath + "UUUUUUR";
    const string BasePathToSignL = BasePathToGirl + "UUUULUUUUUU";
    const string BasePathToSignR = BasePathToGirl + "UUUUUUUUUUL";
    static string[] Pidgey = { "",
        BasePath + "UUUUUURUUUULUUUUUUAUUUUUUUUUUUUULLLUUUUUUUUUUURRR",       // 1
        // BasePath + "UUUUUURUUUULUUUUUUUUUUUUUUUUUUUUULLLUUUUURRRAURAUUU",        // 1 alt
        BasePath + "UUUUUURUUAUULUUUAUUUUUUUUUUUUUUAUULLLUUUUUUURRRRUAUUU",      // 2
        // BasePath + "UUUUUURUUAUULUUUAUUUUUUUUUUUUUAUUULLLUUUUUUURRRRUAUUU",   // 2 early a press
        BasePath + "UUUUUURAUUUUUUUUUUUUUUUUUUUULUAUULLLUUUUUUUUUURRRARU",       // 3
        BasePath + "UUUUUURUUUUUUUUUULAUUUUAUUUUAUUUAUULLLUUUUUUUUAURRRRU",      // 4
        BasePath + "UUUUUURUUUULUUUUUUAUUUUUUUUAUUUAUULLLUUUUUUAUURRUUAURR",     // 5
    //  BasePath + "UUUUUURUUUUUUUUUULAUUUUUUUUUUUUUULLLUUAUUUAURRRRAUU",        // 6
        BasePath + "UUUUUURUUUUUUUUUULAUUUUUUUUUUUUULLLAUUUUUUUARRRRAUU",        // 6 "normal turn"
    };
    static string[] Forest = { "",
        "RUULLLLLUUU" + "RUUUUUUU" + "UUUURRRRRURRRUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUALLLLLLLLDDDDDDDLLLLUUUUUUUUUUUUULLLLLLDDDDDDDDDDDDDDDDDDLDLLLLUUU",       // 1
        // "UUUULLLLLU" + "UUUUURUU" + "UUAUURUARRRRRRRUUUUUUUUUUUAUUAUUUUUUUUUUUUUUUUUUUULLLLLLLLDDDDDDDLLLLUUUUUUUUUUUUULLLLLLDDDDDDDDDDDDDDDDDDLDLLLLUUU",     // 2
        "UAULALLLLAUUU" + "UUUUUURU" + "UUUURRRRUURRRRUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUULLLLLLLLDDDDDDDLLLLUUUUUUUUUUUUULLLLLLDDDDDDDDDDDDDDDDDLDDLLLLUUU", // 2 quint
        "UUUAULLLLLU" + "RUUUUUUU" + "UUURURRURRRRRUAUUUUUUUUUUUUUUUUUUAUUUUUUUUUUUUUULLLLLLLLDDDDDDDLLLLUUUUUUUUUUUUULLLLLLDDDDDDDDDDDDDDDDDDDLLLLLUAUU",     // 3
        "UUUUULLLLLU" + "UUUUUURU" + "UUUURURRRRRRRAUUUAUUUAUUUUUUUUUUUUUUUAUUUUUUUUUUUULLLLLLLLDDDDDDDLLLLUUUUUUUUUUUUULLLLLLDDDDDADDADDADDDDDDDDDDLLLLLAUUU",// 4
        "UUUULLLLLU" + "UUUURUUU" + "UUUURRRRRRRURAUUUUUAUUUUUUAUUUUUUUUUUUUUUUUUAUUUUULLLLLLLLDDDDDDDLLLLUUUUUUUUUUUUULLLLLLDDDDDDDDDDDDDDDDDDDLLLLLAUUU",    // 5
        "UUUULALLLLUUU" + "RUUUUUUU" + "UUURURRRRRRRUUUUUUUAUUUUAUUUUUUUUUUUUUAUUUAUUUUUUULLLLLLLLDDDDDDDLLLLUUUUUUUUUUUUULLLLLLDDDDDDDDDDDDDDDDDDLDLLLLUUU",  // 6
        null, null, null, null
    };

    const string BaseP2Quint = "UAULALLLLAUUU" + "UUUUUURU" + "UUUURRRRUURRRRUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUULLLLLLLLDDDDDDDLLLLUUUUUUUUUUUUULLLLLL";

    public static Dictionary<(int hp, int maxhp, int atk, int def), Paths> SingleSearchWeedle(int framesToWait, int igtf, string pidgeypath, string forestpath, int numThreads, int maxcost, int _hp, int _maxhp, bool antidote, string seenFile="", int apress=0)
    {
        int p = Extended.FramePath(framesToWait);
        StartWatch();

        Red[] gbs = MultiThread.MakeThreads<Red>(numThreads);
        Red gb = gbs[0];
        if (numThreads == 1)
            gb.Show(); // gb.Record("test");
        Elapsed("threads");

        RbyIntroSequence intro = new RbyIntroSequence(RbyStrat.NoPal);
        gb.LoadState("basesaves/red/manip/nido.gqs");
        gb.HardReset();
        intro.ExecuteUntilIGT(gb);
        gb.CpuWrite("wPlayTimeMinutes", 5);
        gb.CpuWrite("wPlayTimeSeconds", (byte)30);
        gb.CpuWrite("wPlayTimeFrames", (byte)igtf);
        intro.ExecuteAfterIGT(gb);
        gb.CpuWriteBE("wPartyMon1HP", (ushort)_hp);
        gb.CpuWriteBE("wPartyMon1MaxHP", (ushort)_maxhp);
        gb.Execute(SpacePath(NidoPath));
        gb.Yoloball(0, Joypad.B);
        gb.ClearText(Joypad.B);
        gb.Press(Joypad.A);
        gb.RunUntil("_Joypad");
        gb.AdvanceFrame();
        gb.AdvanceFrames(Extended.PathFrame(framesToWait));
        gb.Press(Joypad.A);
        gb.Press(Joypad.Start);
        int ret = gb.Execute(SpacePath(pidgeypath));
        Extended.CheckEncounter(ret, gb, "PIDGEY", new IGTResult());
        gb.ClearText(Joypad.A);
        gb.Press(Joypad.B);
        if (forestpath != null)
            ret = gb.Execute(SpacePath(forestpath), (gb.Maps[51][25, 12], gb.PickupItem));
        IGTState state = new IGTState(gb, false, 2);
        Elapsed("states");

        RbyMap route2 = gb.Maps[13];
        RbyMap gate = gb.Maps[50];
        RbyMap forest = gb.Maps[51];
        forest.Sprites.Remove(25, 11);
        Action actions = Action.Right | Action.Left | Action.Up | Action.Down | Action.A | Action.StartB;
        RbyTile startTile = gb.Tile;
        RbyTile[] endTiles = { forest[2, 19] };

        RbyTile[] blockedTiles;
        if (antidote) blockedTiles = new RbyTile[]{
            forest[26, 12], // antidote pickup
            forest[16, 10], forest[18, 10],
            forest[16, 15], forest[18, 15],
            forest[11, 15], forest[12, 15],
            forest[11, 4], forest[12, 4],
            forest[6, 4], forest[8, 4],
            forest[6, 15], forest[8, 15],
            forest[1, 22]
        };
        else blockedTiles = new RbyTile[]{
            forest[25, 11], forest[26, 8], forest[27, 9], // antidote skip
            forest[16, 10], forest[18, 10],
            forest[16, 15], forest[18, 15],
            forest[11, 15], forest[12, 15],
            forest[11, 4], forest[12, 4],
            forest[6, 4], forest[8, 4],
            forest[6, 15], forest[8, 15],
            forest[1, 22]
        };
        Pathfinding.GenerateEdges<RbyMap, RbyTile>(gb, 0, endTiles[0], actions, blockedTiles);
        Pathfinding.GenerateEdges<RbyMap, RbyTile>(gb, 0, gate[5, 1], actions, blockedTiles);
        Pathfinding.GenerateEdges<RbyMap, RbyTile>(gb, 0, route2[3, 44], actions, blockedTiles);
        for (int x = 4; x <= 7; ++x) for (int y = 45; y <= 46; ++y) route2[x, y].RemoveEdge(0, Action.Up);
        route2[3, 44].AddEdge(0, new Edge<RbyMap, RbyTile>() { Action = Action.Up, NextTile = gate[4, 7], NextEdgeset = 0, Cost = 0 });
        gate[4, 7].GetEdge(0, Action.Right).Cost = 0;
        gate[4, 7].RemoveEdge(0, Action.A);
        gate[4, 7].RemoveEdge(0, Action.StartB);
        gate[5, 1].AddEdge(0, new Edge<RbyMap, RbyTile>() { Action = Action.Up, NextTile = forest[17, 47], NextEdgeset = 0, Cost = 0 });
        forest[25, 12].RemoveEdge(0, Action.A);
        forest[25, 13].RemoveEdge(0, Action.A);
        forest[2, 19].RemoveEdge(0, Action.A);
        forest[2, 20].RemoveEdge(0, Action.A);

        const int weedleframes = 3;
        // var results = new List<SFState<RbyMap, RbyTile>>();

        // (int, int)[][] hplists = {
        //     new (int, int)[] { (20, 23), (21, 23) } // p1?
        // };
        (int, int)[][] hplists = {
            new (int, int)[] {(12, 21), (13, 21), (14, 22), (14, 23)}, // p2 g
            new (int, int)[] {(14, 21), (15, 21), (16, 21), (17, 21), (19, 21), (21, 21), (13, 22), (15, 22), (16, 22), (17, 22), (18, 22), (22, 22), (16, 23), (17, 23), (18, 23), (19, 23), (23, 23)}, // p2 y
            new (int, int)[] {(18, 21), (19, 22), (20, 22), (15, 23), (20, 23), (21, 23)}, // p2 r
        };

        // (int, int)[][] hplists = {
        //     new (int, int)[] {(15, 21), (16, 21), (16, 22), (16, 23)}, // p3 y
        //     new (int, int)[] {(18, 21), (19, 21), (18, 22), (19, 22), (14, 21), (15, 22), (19, 23)}, // p3 r
        //     new (int, int)[] {(17, 21), (21, 21), (17, 22), (22, 22), (17, 23), (18, 23), (23, 23)}, // p3 g
        //     new (int, int)[] {(20, 22), (13, 22), (20, 23), (21, 23)}, // p3 b
        //     new (int, int)[] {(12, 21), (13, 21), (14, 22), (14, 23)}, // p3 x
        //     new (int, int)[] {(15, 23) } // p3 w
        // };

        // (int, int)[][] hplists = {
        //     new (int, int)[] {(12, 21), (13, 21), (15, 21), (16, 21), (17, 21), (21, 21), (14, 22), (16, 22), (17, 22), (22, 22), (14, 23), (16, 23), (17, 23), (18, 23), (23, 23)}, // p4 y
        //     new (int, int)[] {(14, 21), (18, 21), (19, 21), (13, 22), (15, 22), (18, 22), (19, 22), (20, 22), (15, 23), (19, 23), (20, 23), (21, 23)}, // p4 r
        // };

        // (int, int)[][] hplists = {
        //     new (int, int)[] {(20, 22), (20, 23), (21, 23)}, // p4 test
        // };

        (int, int)[] hplist = null;
        foreach (var set in hplists)
            foreach ((int h, int m) in set)
                if (_hp == h && _maxhp == m)
                    hplist = set;

        var wpaths = ExtendedWeedle.ReadWeedleStats();
        var stats = new List<(int hp, int maxhp, int atk, int def)>();
        var paths = new Dictionary<(int hp, int maxhp, int atk, int def), Paths>();

        List<(int hp, int maxhp, int atk, int def)> seenStats = new List<(int hp, int maxhp, int atk, int def)>();
        if (File.Exists(seenFile))
        {
            string json = File.ReadAllText(seenFile);
            var pathData = JsonSerializer.Deserialize<Dictionary<string, ExtendedWeedle.PathInfo>>(json);
            foreach (var stat in pathData)
            {
                var values = stat.Key.Trim('(', ')').Split(',');
                int hp = int.Parse(values[0]);
                int maxhp = int.Parse(values[1]);
                int atk = int.Parse(values[2]);
                int def = int.Parse(values[3]);
                var statKey = (hp, maxhp, atk, def);
                seenStats.Add(statKey);
            }
        }

        foreach ((int hp, int maxhp) in hplist)
        {
            foreach (var stat in wpaths)
                if (stat.hp == hp && stat.maxhp == maxhp)
                {
                    var statKey = (hp, maxhp, stat.atk, stat.def);
                    if (seenStats.Contains(statKey)) continue;
                    stats.Add(statKey);
                    paths[statKey] = new Paths();
                }
        }

        string link = "https://gunnermaniac.com/pokeworld?local=51#" + (startTile.X + 13) + "/" + (startTile.Y + 11) + "/";
        var parameters = new SFParameters<Red, RbyMap, RbyTile>()
        {
            MaxCost = maxcost,
            EndTiles = endTiles,
            FoundCallback = (state, gb) =>
            {
                // if(state.Log == "UUUULLLLLUUURUUUUUUUUUURRURRURRRRUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUULLLLLLLLDDDDDDDLLLLUUUUUUUUUUUUULLLLLLDDDDDDDDDDDDDDLDDDDDLLLLUUU")
                // {
                //     Trace.WriteLine($"{gb.CpuRead("wPlayTimeMinutes"):d2}:{gb.CpuRead("wPlayTimeSeconds"):d2}.{gb.CpuRead("wPlayTimeFrames"):d2} {gb.CpuRead("hRandomAdd"):x2}{gb.CpuRead("hRandomSub"):x2}");
                // }
                gb.LoadState(state.IGT.State);
                gb.Press(Joypad.A);
                gb.ClearText(1);
                gb.Inject(Joypad.None);
                byte[] textboxstate = gb.SaveState();
                byte[][] battlestates = new byte[3][];
                for (int weedleframe = 0; weedleframe < weedleframes; ++weedleframe)
                {
                    gb.LoadState(textboxstate);
                    gb.AdvanceFrames(weedleframe);
                    gb.Inject(Joypad.B);
                    gb.AdvanceFrame(Joypad.B);
                    gb.ClearText(Joypad.B, 1);
                    battlestates[weedleframe] = gb.SaveState();
                }
                for (int i = 0; i < stats.Count; ++i)
                {
                    int hp = stats[i].hp;
                    int maxhp = stats[i].maxhp;
                    int atk = stats[i].atk;
                    int def = stats[i].def;
                    int weedleframe;
                    for (weedleframe = 0; weedleframe < weedleframes; ++weedleframe)
                    {
                        gb.LoadState(battlestates[weedleframe]);
                        gb.CpuWriteBE("wPartyMon1HP", (ushort)hp);
                        gb.CpuWriteBE("wPartyMon1MaxHP", (ushort)maxhp);
                        gb.CpuWriteBE("wPartyMon1Attack", (ushort)atk);
                        gb.CpuWriteBE("wPartyMon1Defense", (ushort)def);
                        gb.ClearText(Joypad.B);
                        int lasthp = hp;
                        gb.Press(Joypad.A, Joypad.Down, Joypad.A); // t1
                        DoTurn(gb);
                        gb.ClearText(Joypad.A);
                        if (gb.BattleMon.HP < hp) break;
                        gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t2
                        DoTurn(gb);
                        gb.ClearText(Joypad.A);
                        if (gb.BattleMon.HP < hp - 3 || gb.BattleMon.Poisoned) break;
                        lasthp = gb.BattleMon.HP;
                        gb.Press(Joypad.A, Joypad.Up, Joypad.A); // t3
                        DoTurn(gb);
                        gb.ClearText(Joypad.A);
                        if (gb.BattleMon.HP <= lasthp - 5) break;
                        if (gb.BattleMon.HP < hp - 6 || gb.BattleMon.Poisoned || gb.EnemyMon.HP > 21) break;
                        lasthp = gb.BattleMon.HP;
                        gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t4
                        DoTurn(gb);
                        gb.ClearText(Joypad.A);
                        if (gb.BattleMon.HP <= lasthp - 5) break;
                        if (gb.BattleMon.HP < hp - 6 || gb.BattleMon.Poisoned || gb.EnemyMon.HP > 15) break;
                        lasthp = gb.BattleMon.HP;
                        gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t5
                        DoTurn(gb);
                        gb.ClearText(Joypad.A);
                        if (gb.BattleMon.HP <= lasthp - 5) break;
                        if (gb.BattleMon.HP < hp - 6 || gb.BattleMon.Poisoned || gb.EnemyMon.HP > 8) break;
                        lasthp = gb.BattleMon.HP;
                        gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t6
                        DoTurn(gb);
                        if (gb.BattleMon.HP <= lasthp - 5) break;
                        if (gb.BattleMon.HP < hp - 6 || gb.BattleMon.Poisoned || gb.EnemyMon.HP != 0) break;
                    }
                    if (weedleframe == weedleframes)
                    {
                        SearchCommon.Path p = new SearchCommon.Path(state.Log);
                        Trace.WriteLine(atk + " " + def + " " + hp + "/" + maxhp);
                        Trace.WriteLine(link + state.Log);
                        paths[stats[i]].Add(p);
                    }
                }
            }
        };

        if (antidote) parameters.TileCallback = (forest[25, 12], gb => gb.PickupItem());

        SingleFrameSearch.StartSearch(gbs, parameters, startTile, 0, state, apress);
        Elapsed("search");

        // p2f0 all hps 159181.48s
        // p2f6g3 40307.168s
        // p2f6g2 120430.18s

        // for(int i = 0; i < stats.Count; ++i)
        // {
        //     foreach(Path path in paths[i])
        //     {
        //         var r = Weedle(p, path.P, 30, false, stats[i].atk, stats[i].def, stats[i].hp, stats[i].maxhp);
        //         path.I = r.info;
        //         path.SS = r.s9 * 100 + r.s15 * 10 + r.s1 + r.s2;
        //     }
        // }
        // Elapsed("check");

        // for(int i = 0; i < stats.Count; ++i)
        // {
        //     Trace.WriteLine(stats[i].atk + " " + stats[i].def + " " + stats[i].hp + "/" + stats[i].maxhp + " p" + p + " (" + maxcost + ")");
        //     paths[i].RemoveAll(p => p.SS < 770);
        //     paths[i].PrintAll(link);
        // }
        return paths;
    }

    public static string SearchPostWeedle(int numThreads, int maxcost, List<byte[]> saveStates)
    {
        StartWatch();
        Red[] gbs = MultiThread.MakeThreads<Red>(numThreads);
        Red gb = gbs[0];
        // if (numThreads == 1)
        //     gb.Record("test"); // gb.Show();
        Elapsed("threads");

        IGTResults states = new IGTResults(saveStates.Count);
        MultiThread.For(states.Length, gbs, (gb, i) =>
        {
            gb.LoadState(saveStates[i]);
            gb.ClearTextUntil(Joypad.B, gb.SYM["EnterMap"]);
            gb.AdvanceFrames(35);
            states[i] = new IGTState(gb, false, i);
        });

        Action actions = Action.Left | Action.Up | Action.A | Action.StartB;
        RbyTile[] endTiles = { gb.Maps[51][1, 6] };
        RbyTile[] blockedTiles = { gb.Maps[51][2, 20], gb.Maps[51][2, 17],
                                   gb.Maps[51][2, 15], gb.Maps[51][2, 13],
                                   gb.Maps[51][2, 11], gb.Maps[51][2, 6] };
        gb.AdvanceFrames(5);
        Pathfinding.GenerateEdges<RbyMap, RbyTile>(gb, 0, endTiles[0], actions, blockedTiles);

        Paths paths = new Paths();
        string postFight = "";
        var parameters = new DFParameters<Red, RbyMap, RbyTile>()
        {
            MaxCost = maxcost,
            SuccessSS = states.Length,
            EndTiles = endTiles,
        };

        parameters.FoundCallback = state =>
        {
            SearchCommon.Path p = new SearchCommon.Path(state.Log, state.IGT.TotalRunning);
            paths.Add(p);
            Console.WriteLine(p);
            if (state.WastedFrames < parameters.MaxCost) parameters.MaxCost = state.WastedFrames;
        };

        DepthFirstSearch.StartSearch(gbs, parameters, gb.Tile, 0, states);
        Elapsed("search");

        if (paths.Count > 0)
            postFight = paths.OrderByDescending(p => p.SS).ThenBy(p => p.C).ThenBy(p => p.S).ThenBy(p => p.A).ThenBy(p => p.T).First().P;

        // gb.Dispose();
        // foreach (var g in gbs) g.Dispose();
        return postFight;
    }

    static List<DFState<RbyMap, RbyTile>> SearchPidgeyAlt(int framesToWait, string path, int f1, int f2, int numThreads = 8, int success = -1, int maxcost = 6)
    {
        Extended.BuildStates();
        StartWatch();

        Red[] gbs = MultiThread.MakeThreads<Red>(numThreads);
        Red gb = gbs[0];
        if(numThreads == 1)
            gb.Record("test");
        Elapsed("threads");

        int numFrames = ExtendedWeedle.FrameCount(f1, f2);
        int[] frames = new int[numFrames];
        for (int k = 0; k < numFrames; k++) frames[k] = (k + f1) % 60;
        int numigt = (numFrames * 60) - (f2+1+2);
        IGTResults states = new IGTResults(numigt);
        // List<(int s, int f)> igts = new List<(int s, int f)>();
        MultiThread.For(states.Length, gbs, (gb, i) =>
        {
            int f = i;
            for (int s = 0; s < 60; ++s)
                foreach (int skip in Extended.IgnoredFrames)
                {
                    int skipf = skip + numFrames * s;
                    int skipInWindow = frames[skipf % numFrames];
                    if (f >= skipf && skipInWindow == skip)
                        ++f;
                }

            int sec = f / numFrames, frame = frames[f % numFrames];
            if (sec == 59 && (frame == 58 || frame == 59)) return;
            gb.LoadState("basesaves/red/manip/ext/nido_" + sec + "_" + frame + ".gqs");
            // igts.Add((sec, frame));
            gb.AdvanceFrames(framesToWait);
            gb.Press(Joypad.A);
            gb.Press(Joypad.Start);

            int ret = gb.Execute(SpacePath(path));
            states[i] = new IGTState(gb, false, f);
        });
        Elapsed("states");

        // igts.Sort();
        // foreach (var igt in igts) Console.WriteLine(igt);

        string link = "https://gunnermaniac.com/pokeworld?map=1#33/181/" + BasePathToGirl;

        RbyMap viridian = gb.Maps[1];
        RbyMap route2 = gb.Maps[13];
        viridian.Sprites.Remove(18, 9);
        viridian.Sprites.Remove(17, 5);
        RbyTile startTile = gb.Tile;
        RbyTile[] endTiles = { route2[8, 48] };
        RbyTile[] encounterTiles = { route2[6, 48], route2[7, 48], route2[8, 48], route2[7, 49], route2[8, 49], route2[8, 50] };
        Pathfinding.GenerateEdges<RbyMap, RbyTile>(gb, 0, endTiles[0], Action.Right | Action.Left | Action.Up | Action.Down | Action.A | Action.StartB);
        // Pathfinding.DebugDrawEdges(gb, viridian, 0);

        var results = new List<DFState<RbyMap, RbyTile>>();
        var parameters = new DFParameters<Red, RbyMap, RbyTile>()
        {
            MaxCost = maxcost,
            SuccessSS = success >= 0 ? success : Math.Max(1, states.Length - 3),// amount of yoloball success for found
            EndTiles = endTiles,
            EncounterCallback = gb => gb.EnemyMon.Species.Name == "PIDGEY" && gb.Yoloball() && encounterTiles.Any(t => t.X == gb.Tile.X && t.Y == gb.Tile.Y),
            FoundCallback = state =>
            {
                results.Add(state);
                Trace.WriteLine(link + state.Log + " Captured: " + state.IGT.TotalSuccesses + " Failed: " + (state.IGT.TotalFailures - state.IGT.TotalRunning) + " NoEnc: " + state.IGT.TotalRunning + " Cost: " + state.WastedFrames);
            }
        };

        DepthFirstSearch.StartSearch(gbs, parameters, startTile, 0, states, 0);
        Elapsed("search");

        return results;
    }

    public ExtWeedleSearch()
    {
        // var wpaths = ExtendedWeedle.ReadWeedlePaths();
        // string link = "https://gunnermaniac.com/pokeworld?local=51#21/59/";
        int igtf = 5;
        int numThreads = 6;
        // int hp = 20, maxhp = 23; // p2 g3
        // int hp = 16, maxhp = 22; // p2 g2
        // int hp = 12, maxhp = 21; // p2 g1

        // Trace.WriteLine("-----G1-----");
        // int hp1 = 14, maxhp1 = 23; // p2f0 g1
        // SingleSearchWeedle(2, igtf, Pidgey[2], null, numThreads, 6, hp1, maxhp1, "p2a_f58-0_f4-7.json", 1);

        // Trace.WriteLine("-----G3-----");
        // int hp3 = 21, maxhp3 = 23; // p2bf5 g3 4a
        // SingleSearchWeedle(2, igtf, Pidgey[2], null, numThreads, 8, hp3, maxhp3, false, "p2b_f57-1_f3-8.json", 4);

        // int hp2 = 12, maxhp2 = 21; // p2bf5 g1 4a 20912.195s
        int hp2 = 16, maxhp2 = 22; // p2bf5 g3 4a 21 stats 49121.594s
        SingleSearchWeedle(2, igtf, Pidgey[2], null, numThreads, 8, hp2, maxhp2, false, "p2b_f57-1_f3-8.json", 4);

        // p3 test search
        // int hp = 20, maxhp = 22; // p3 g4
        // SingleSearchWeedle(4, igtf, Pidgey[3], null, numThreads, 6, hp, maxhp);

        // p1 test search
        // int hp = 20, maxhp = 23; // p1 g?
        // SingleSearchWeedle(1, igtf, Pidgey[1], null, numThreads, 6, hp, maxhp, false, apress: 1);
        // Trace.WriteLine("----- FRAME 59 -----");
        // SingleSearchWeedle(1, 59, Pidgey[1], null, numThreads, 6, hp, maxhp);


        // p4
        // int hp = 20, maxhp = 22; // p4
        // SingleSearchWeedle(4, igtf, Pidgey[4], null, numThreads, 6, hp, maxhp);
        // Trace.WriteLine("----- PATH 5 -----");
        // SingleSearchWeedle(5, igtf, Pidgey[5], null, numThreads, 6, hp, maxhp);
        // foreach (var res in results)
        // {
        //     Trace.WriteLine(res.Key);
        //     foreach (var path in res.Value)
        //     {
        //         Trace.WriteLine(link + path.P);
        //     }
        // }

        // alt p1 search
        // SearchPidgeyAlt(1, BasePathToGirl, 56, 2, numThreads, 6, 6);

        // alt p2 search
        // SearchPidgeyAlt(2, BasePathToGirl, 56, 8, numThreads, -1, 8);
    }
}
