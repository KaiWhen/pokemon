using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

using static System.Threading.Interlocked;
using static SearchCommon;
using static RbyIGTChecker<Red>;
using System.Data;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;

public class MoonRocket
{
    static string rt3Moon = "RRRRRRRRURRUUUUUARRRRRRRRRRRRDDDDDRRRRRRRARUURRUUUUUUUUUURRRRUUUUUUUUUURRRRRU"
    + "UUUUUULLLLLALLLLDD"
    + "RRRRUURRRARRUUUUUUURRRRRRRAUUUUUUURRRDRDDDDDDDADDDDDDDDADRRRRRURRRR"
    + "UUUUUUUUR"
    + "ULUUUUUAUUUUUULLLUUUUUUUULLLLLLDDLALLLLLLLDDDDDD"
    + "LALLALLALLALDD"
    + "RRRUUULAUR"
    + "DDADLALLAD"
    + "RARRARRARRARUU"
    + "DDLDDDDLLLLLLLULUUUUULUUUUUUUULLLUL"
    + "DADDRAR"
    + "DRRDDDDDDDDDDRRRARRRRRRRRRRDR"
    + "RRUUURARRRDDRRRRRUARURARRDDDDDDDDALLLLDDDDDDDADDLLLALLLLLLLLLLLLALLLLLLUUUUAUUALUUUUUUUU";

    static string rt3MoonPartial1 = "RRRRRRRRURRUUUUUARRRRRRRRRRRRDDDDDRRRRRRRARUURRUUUUUUUUUURRRRUUUUUUUUUURRRRRU"
    + "UUUUUULLLLLALLLLDD"
    + "RRRRUURRRARRUUUUUUURRRRRRRAUUUUUUURRRDRDDDDDDDADDDDDDDDADRRRRRURRRR"
    + "UUUUUUUUR"
    + "ULUUUUUAUUUUUULLLUUUUUUUULLLLLLDDLALLLLLLLDDDDDD"
    + "LALLALLALLALDD"
    + "RRRUUULAUR"
    + "DDADLALLAD"
    + "RARRARRARRARUU"
    + "DDLDDDDLLLLLLLULUUUUULUUUUUUUULLLUL"
    + "DADDRAR"
    + "DRRDDDDDDDDDDRRRARRRRRRRRRRDR"
    + "RRUUURARRRDDRRRRRUARURARRDDDDDDDDALLLLDDDDDDDADDLLLALLLLLLLLLLLLALLLLL";

    static string rt3MoonPartial2 = "RRRRRRRRURRUUUUUARRRRRRRRRRRRDDDDDRRRRRRRARUURRUUUUUUUUUURRRRUUUUUUUUUURRRRRU"
    + "UUUUUULLLLLALLLLDD"
    + "RRRRUURRRARRUUUUUUURRRRRRRAUUUUUUURRRDRDDDDDDDADDDDDDDDADRRRRRURRRR"
    + "UUUUUUUUR"
    + "ULUUUUUAUUUUUULLLUUUUUUUULLLLLLDDLALLLLLLLDDDDDD"
    + "LALLALLALLALDD"
    + "RRRUUULAUR"
    + "DDADLALLAD"
    + "RARRARRARRARUU"
    + "DDLDDDDLLLLLLLULUUUUULUUUUUUUULLLUL"
    + "DADDRAR"
    + "DRRDDDDDDDDDDRRRARRRRRRRRRRDR"
    + "RRUUURARRRDDRRRRRUARURARRDDDDDDDDALLLLDDDDDDDADD";

    static string rt3MoonPartial3 = "RRRRRRRRURRUUUUUARRRRRRRRRRRRDDDDDRRRRRRRARUURRUUUUUUUUUURRRRUUUUUUUUUURRRRRU"
    + "UUUUUULLLLLALLLLDD"
    + "RRRRUURRRARRUUUUUUURRRRRRRAUUUUUUURRRDRDDDDDDDADDDDDDDDADRRRRRURRRR"
    + "UUUUUUUUR"
    + "ULUUUUUAUUUUUULLLUUUUUUUULLLLLLDDLALLLLLLLDDDDDD"
    + "LALLALLALLALDD"
    + "RRRUUULAUR"
    + "DDADLALLAD"
    + "RARRARRARRARUU"
    + "DDLDDDDLLLLLLLULUUUUULUUUUUUUULLLUL"
    + "DADDRAR"
    + "DRRDDDDDDDDDDRRRARRRRRRRRRRDR"
    + "RRUUURARRRDDRRRRRUARURARR";

    string moon3Link = "https://gunnermaniac.com/pokeworld?map=61#21/17/";

    static bool LoadPartialMoonState(Red gb, string path, int hp, int igtf, int igts=50)
    {
        RbyMap moon1 = gb.Maps[59];
        RbyMap moon2 = gb.Maps[60];
        RbyMap moon3 = gb.Maps[61];
        // gb.Record("rt3moontest");
        RbyIntroSequence intro = new RbyIntroSequence(RbyStrat.PalHold);
        gb.LoadState("basesaves/red/manip/rt3.gqs");
        gb.HardReset();
        intro.ExecuteUntilIGT(gb);
        gb.CpuWrite("wPlayTimeMinutes", 17);
        gb.CpuWrite("wPlayTimeSeconds", (byte)igts);
        gb.CpuWrite("wPlayTimeFrames", (byte)igtf);
        intro.ExecuteAfterIGT(gb);
        gb.CpuWriteBE("wPartyMon1HP", (ushort)hp);
        int adr = gb.Execute(SpacePath(path),
            (moon1[ 5, 31], gb.PickupItem),
            (moon1[34, 31], gb.PickupItem),
            (moon1[35, 23], gb.PickupItem),
            (moon3[28,  5], gb.PickupItem),
            (moon1[ 2,  3], gb.PickupItem),
            (moon1[ 3,  2], gb.PickupItem)
        );
        if (adr == gb.WildEncounterAddress) return false;
        else return true;
    }

    void SearchParas(string path, int hp, int f1, int f2, int numThreads = 6, int success = -1, int maxcost = 24)
    {
        StartWatch();
        int numigt = ExtendedWeedle.FrameCount(f1, f2);
        Red[] gbs = MultiThread.MakeThreads<Red>(numThreads);
        Red gb = gbs[0];
        IGTResults states = new IGTResults(numigt);
        MultiThread.For(numigt, gbs, (gb, i) =>
        {
            int igtf = (f1 + i) % 60;
            LoadPartialMoonState(gb, path, hp, igtf);
            states[i] = new IGTState(gb, false, igtf);
        });
        Elapsed("states");

        RbyMap moon1 = gb.Maps[59];
        RbyMap moon2 = gb.Maps[60];
        RbyMap moon3 = gb.Maps[61];
        Action actions = Action.Right | Action.Down | Action.Up | Action.Left | Action.A;
        RbyTile startTile = gb.Tile;
        RbyTile[] endTiles = { moon3[10, 17] };
        RbyTile[] encounterTiles = { moon3[10, 17], moon3[10, 18] };
        RbyTile[] blockedTiles =
        {
            moon3[33, 23], moon3[34, 23],
            moon3[35, 23], moon3[36, 23],
            moon3[37, 14], moon3[11, 19]
        };
        Pathfinding.GenerateEdges<RbyMap, RbyTile>(gb, 0, endTiles[0], actions, blockedTiles);
        moon3[26, 31].RemoveEdge(0, Action.A);
        moon3[25, 31].RemoveEdge(0, Action.A);
        moon3[24, 31].RemoveEdge(0, Action.A);
        moon3[23, 31].RemoveEdge(0, Action.A);
        moon3[22, 31].RemoveEdge(0, Action.A);
        moon3[21, 31].RemoveEdge(0, Action.A);
        moon3[20, 31].RemoveEdge(0, Action.A);
        moon3[19, 31].RemoveEdge(0, Action.A);
        // moon3[28, 6].GetEdge(0, Action.Left).Cost = 0;
        // moon3[28, 6].RemoveEdge(0, Action.Up);
        // moon3[27, 6].RemoveEdge(0, Action.Right);
        // moon3[27, 5].RemoveEdge(0, Action.A);

        var parameters = new DFParameters<Red, RbyMap, RbyTile>()
        {
            MaxCost = maxcost,
            SuccessSS = success,
            EndTiles = endTiles,
            EncounterCallback = gb => gb.EnemyMon.Species.Name == "PARAS" && gb.Yoloball() && encounterTiles.Any(t => t.X == gb.Tile.X && t.Y == gb.Tile.Y),
            LogStart = startTile.PokeworldLink + "/",
            FoundCallback = state =>
            {
                if (state.IGT.TotalSuccesses == numigt)
                    Trace.WriteLine(state.Log);
            }
        };

        DepthFirstSearch.StartSearch(gbs, parameters, startTile, 0, states, 0);
        Elapsed("search");
    }

    static (List<int> successfulFrames, List<int> twMissFrames) CheckMoonRocket(string partialPath, string parasPath, int hp, int igtf1, int igtf2, int numThreads=6)
    {
        int numigt = ExtendedWeedle.FrameCount(igtf1, igtf2);
        List<int> successfulFrames = new List<int>();
        List<int> twMissFrames = new List<int>();
        int rocketFrames = 3;
        string twMiss = "tail whip miss";
        string haCrit = "horn attack crit";

        RedCb[] gbs = MultiThread.MakeThreads<RedCb>(numThreads);
        // RedCb gb = new RedCb();
        MultiThread.For(numigt, gbs, (gb, it) =>
        // for (int it = 0; it < numigt; ++it)
        {
            int igtf = (igtf1 + it) % 60;
            int rocketSuccess = 0;
            int t1twMiss = 0;
            if (!LoadPartialMoonState(gb, partialPath, hp, igtf)) return;
            if (numThreads == 1) gbs[0].Record("rocketTest");
            int adr = gb.Execute(SpacePath(parasPath));
            if (adr != gb.WildEncounterAddress) return;
            if (gb.EnemyMon.Species.Name != "PARAS") return;
            if (!gb.Yoloball(0, Joypad.B)) return;
            gb.ClearText(Joypad.A);
            gb.Press(Joypad.B);
            gb.Execute("R");
            gb.AdvanceFrames(63);
            gb.ClearText(Joypad.A, 3);
            gb.Inject(Joypad.None);
            byte[] rocketState = gb.SaveState();
            for (int rocketFrame = 0; rocketFrame < rocketFrames; ++rocketFrame)
            {
                string log = "";
                string turn = "";
                bool fail = false;
                gb.LoadState(rocketState);
                gb.AdvanceFrames(rocketFrame);
                gb.Inject(Joypad.B);
                gb.AdvanceFrame(Joypad.B);
                gb.ClearText(Joypad.B);
                gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t1
                turn = LogTurn(gb);
                log += turn;
                if (!turn.ToLower().Contains(twMiss)) fail = true;
                if (turn.ToLower().Contains(twMiss)) Increment(ref t1twMiss);
                // if (
                //     gb.BattleMon.HP < hp || 
                //     gb.EnemyMon.Poisoned) fail = true;
                // gb.ClearText(Joypad.A);
                // gb.Press(Joypad.A, Joypad.Up, Joypad.A); // t2
                // turn = LogTurn(gb);
                // log += turn;
                // if (gb.BattleMon.HP < hp) fail = true;
                // if (!turn.ToLower().Contains(haCrit)
                // // || !turn.ToLower().Contains(twMiss)
                // ) fail = true;
                // if (gb.EnemyMon.HP != 0) 
                // {
                //     fail = true;
                //     gb.ClearText(Joypad.A);
                // }
                // else gb.ClearText(Joypad.B);
                // gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t3
                // turn = LogTurn(gb);
                // log += turn;
                // if (turn.ToLower().Contains(haCrit)) fail = true;
                // if (
                //     // gb.BattleMon.HP < hp || 
                //     gb.BattleMon.Confused) fail = true;
                // if (gb.EnemyMon.HP == 0) gb.ClearText(Joypad.B);
                // else gb.ClearText(Joypad.A);
                // gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t4
                // turn = LogTurn(gb);
                // // if (!turn.ToLower().Contains(haCrit)) fail = true;
                // log += turn;
                // // Console.WriteLine(igtf + " (" + rocketFrame + ")" + log);
                
                // if (gb.EnemyMon.HP == 0 && !fail) {
                //     Increment(ref rocketSuccess);
                //     Console.WriteLine(igtf + " " + rocketFrame + " success");
                // }
                // continue;
            }
            // if (rocketSuccess == rocketFrames) lock (successfulFrames) successfulFrames.Add(igtf);
            if (t1twMiss == rocketFrames) lock (twMissFrames) twMissFrames.Add(igtf);
        }
        );

        return (successfulFrames, twMissFrames);
    }

    static List<string> ReadParasPaths(string inputFile)
    {
        List<string> paths = new List<string>();
        foreach (string line in File.ReadAllLines(inputFile))
        {
            var r1 = Regex.Match(line, "([LRUDA]+)");
            if (r1.Success) paths.Add(r1.Groups[1].Value);
        }
        return paths;
    }

    void CheckRocketPaths(string inputFile, string partialPath, int hp, int f1, int f2, int numThreads=6)
    {
        var paths = ReadParasPaths(inputFile);
        foreach (var path in paths)
        {
            Trace.WriteLine(path);
            var res = CheckMoonRocket(partialPath, path, hp, f1, f2, numThreads);
            var frames = res.successfulFrames;
            var twFrames = res.twMissFrames;
            Trace.Write(String.Join(",", frames));
            Trace.WriteLine("\n");
            Trace.Write(String.Join(",", twFrames));
            Trace.WriteLine("\n");
        }
    }

    public MoonRocket()
    {
        // SearchParas(rt3MoonPartial3, 41, 0, 15, maxcost: 30);

        // List<int> frames = CheckMoonRocket("LUUAUUUUUAUUULUUUU", 41, 0, 15, 6);
        // foreach (int frame in frames) Console.WriteLine(frame); 

        CheckRocketPaths("moonrocket/paras/partial3.txt", rt3MoonPartial3, 41, 0, 15, 6);
    }
}