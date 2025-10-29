using System.Linq;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using static SearchCommon;
using static RbyIGTChecker<Red>;
using System.Runtime.InteropServices;
using System.Net.Security;
using OpenGL;
using System.Reflection.Metadata;
using System.Threading;

class ExtWeedleSearch
{
    const string NidoPath = "LLLULLUAULALDLDLLDADDADLALLALUUAU";
    const string BasePath = "DRRUUURRRRRRRRRRRRRRRRRRRRRUR";
    const string BasePathToGirl = BasePath + "UUUUUUR";
    const string BasePathToSignL = BasePathToGirl + "UUUULUUUUUU";
    const string BasePathToSignR = BasePathToGirl + "UUUUUUUUUUL";
    static string[] Pidgey = { "",
        BasePath + "UUUUUURUUUULUUUUUUAUUUUUUUUUUUUULLLUUUUUUUUUUURRR",          // 1
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

    static Dictionary<(int hp, int maxhp, int atk, int def), Paths> SingleSearchWeedle(int framesToWait, int igtf, string pidgeypath, string forestpath, int numThreads, int maxcost, int _hp, int _maxhp)
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
        gb.AdvanceFrames(framesToWait);
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
        Action gateActions = Action.Right | Action.Left | Action.Up | Action.Down;
        RbyTile startTile = gb.Tile;
        RbyTile[] endTiles = { forest[2, 19] };
        RbyTile[] blockedTiles = {
            forest[25, 11], forest[26, 8], forest[27, 9],
            forest[16, 10], forest[18, 10],
            forest[16, 15], forest[18, 15],
            forest[11, 15], forest[12, 15],
            forest[11, 4], forest[12, 4],
            forest[6, 4], forest[8, 4],
            forest[6, 15], forest[8, 15],
            forest[1, 22]
        };
        Pathfinding.GenerateEdges<RbyMap, RbyTile>(gb, 0, endTiles[0], actions, blockedTiles);
        Pathfinding.GenerateEdges<RbyMap, RbyTile>(gb, 0, gate[5, 1], gateActions, blockedTiles);
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
        (int, int)[][] hplists = {
            new (int, int)[] {(12, 21), (13, 21), (14, 22), (14, 23)}, // p2 g
            new (int, int)[] {(14, 21), (15, 21), (16, 21), (17, 21), (19, 21), (21, 21), (13, 22), (15, 22), (16, 22), (17, 22), (18, 22), (22, 22), (16, 23), (17, 23), (18, 23), (19, 23), (23, 23)}, // p2 y
            new (int, int)[] {(18, 21), (19, 22), (20, 22), (15, 23), (20, 23), (21, 23)}, // p2 r
        };

        (int, int)[] hplist = null;
        foreach (var set in hplists)
            foreach ((int h, int m) in set)
                if (_hp == h && _maxhp == m)
                    hplist = set;

        var wpaths = ExtendedWeedle.ReadWeedleStats();
        var stats = new List<(int hp, int maxhp, int atk, int def)>();
        var paths = new Dictionary<(int hp, int maxhp, int atk, int def), Paths>();
        foreach ((int hp, int maxhp) in hplist)
        {
            foreach (var stat in wpaths)
                if (stat.hp == hp && stat.maxhp == maxhp)
                {
                    stats.Add((hp, maxhp, stat.atk, stat.def));
                    paths[(stat.hp, stat.maxhp, stat.atk, stat.def)] = new Paths();
                }
        }

        // missing from p2f6g3
        stats.Add((21, 23, 12, 13));
        stats.Add((18, 21, 11, 12));
        stats.Add((21, 23, 11, 13));
        stats.Add((18, 21, 11, 13));
        stats.Add((19, 22, 11, 13));
        stats.Add((21, 23, 11, 12));
        stats.Add((15, 23, 11, 14));
        stats.Add((21, 23, 11, 14));
        stats.Add((20, 22, 12, 13));
        paths[(21, 23, 12, 13)] = new Paths();
        paths[(18, 21, 11, 12)] = new Paths();
        paths[(21, 23, 11, 13)] = new Paths();
        paths[(18, 21, 11, 13)] = new Paths();
        paths[(19, 22, 11, 13)] = new Paths();
        paths[(21, 23, 11, 12)] = new Paths();
        paths[(15, 23, 11, 14)] = new Paths();
        paths[(21, 23, 11, 14)] = new Paths();
        paths[(20, 22, 12, 13)] = new Paths();

        // foreach (var wp in wpaths)
        // {
        //     if (wp.S9 < 9 || wp.S15 < 12)
        //     {
        //         stats.Add((wp.HP, wp.MaxHP, wp.Atk, wp.Def));
        //         paths[(wp.HP, wp.MaxHP, wp.Atk, wp.Def)] = new Paths();
        //     }
        // }

        // int found = 0;
        // Paths[] paths = new Paths[stats.Count];
        // for (int i = 0; i < stats.Count; ++i) paths[i] = new Paths();
        string link = "https://gunnermaniac.com/pokeworld?local=51#" + (startTile.X + 13) + "/" + (startTile.Y + 11) + "/";
        var parameters = new SFParameters<Red, RbyMap, RbyTile>()
        {
            MaxCost = maxcost,
            EndTiles = endTiles,
            // TileCallback = (forest[25, 12], gb => gb.PickupItem()),
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
                        // ++found;
                    }
                }
            }
        };

        SingleFrameSearch.StartSearch(gbs, parameters, startTile, 0, state, 0);
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

        return postFight;
    }

    public ExtWeedleSearch()
    {
        // var wpaths = ExtendedWeedle.ReadWeedlePaths();
        string link = "https://gunnermaniac.com/pokeworld?local=51#21/59/";
        int igtf = 6;
        int numThreads = 6;
        // int hp = 20, maxhp = 23; // p2 g3
        int hp = 16, maxhp = 22; // p2 g2
        // int atk = wp.Atk, def = wp.Def, hp = wp.HP, maxhp = wp.MaxHP;
        // Trace.WriteLine("----- " + atk + " " + def + " " + hp + "/" + maxhp + " -----");
        var results = SingleSearchWeedle(2, igtf, Pidgey[2], null, numThreads, 6, hp, maxhp);
        foreach (var res in results)
        {
            Trace.WriteLine(res.Key);
            foreach (var path in res.Value)
            {
                Trace.WriteLine(link + path.P);
            }
        }
    }
}
