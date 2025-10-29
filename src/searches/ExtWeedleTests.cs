using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

using static System.Threading.Interlocked;
using static SearchCommon;
using static RbyIGTChecker<Red>;
using System.Dynamic;
using System.Data;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Threading;
using OpenGL;
using System.Net.Http.Headers;
using System.Text.Json;
using System.IO;
using System.Xml;
using System.Runtime.ExceptionServices;

public class ExtendedWeedle
{
    class IGTStateResult : IGTResult
    {
        public byte[] State;
    }

    public class PathInfo
    {
        public string route2 { get; set; }
        public string gate { get; set; }
        public string forest { get; set; }
        public string postFight { get; set; }
        public string link { get; set; }
        public string npcs { get; set; }
        public string path { get; set; }
        public List<int> frames { get; set; }
        public List<int> igtSecs { get; set; }
        public int score { get; set; }
        public Dictionary<int, List<string>> frameLogs { get; set; }
    }

    static Dictionary<string, byte[]> WeedleStates = new Dictionary<string, byte[]>();

    static int PathFrame(int path)
    {
        return path > 2 ? path + 1 : path;
    }

    static RbyTile EndTile(string path)
    {
        return SearchCommon.EndTile(new Red().Maps[33][33, 11], path);
    }

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

    static SortedSet<int> IgnoredFramesP2 = new SortedSet<int> { 10, 11, 12, 13, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38 };

    static void SortFrames(List<int> frames)
    {
        if (frames.Count <= 1) return;

        frames.Sort();

        if (!frames.Contains(0) || !frames.Contains(59))
            return;

        int maxGap = 0;
        int splitIndex = 0;

        for (int i = 0; i < frames.Count - 1; i++)
        {
            int gap = frames[i + 1] - frames[i];
            if (gap > maxGap)
            {
                maxGap = gap;
                splitIndex = i + 1;
            }
        }

        var temp = frames.GetRange(0, splitIndex);
        frames.RemoveRange(0, splitIndex);
        frames.AddRange(temp);
    }
    
    static List<List<int>> GroupContiguousFrames(IEnumerable<int> frames)
    {
        var frameSet = new HashSet<int>(frames.Select(f => f % 60));
        if (frameSet.Count == 0) return new List<List<int>>();
        var sorted = frameSet.OrderBy(f => f).ToList();
        var result = new List<List<int>>();
        var current = new List<int> { sorted[0] };
        bool IsNext(int a, int b) => (a + 1) % 60 == b;
        
        for (int i = 1; i < sorted.Count; i++)
        {
            if (IsNext(sorted[i - 1], sorted[i]))
                current.Add(sorted[i]);
            else
            {
                result.Add(new List<int>(current));
                current.Clear();
                current.Add(sorted[i]);
            }
        }
        result.Add(new List<int>(current));

        // merge if >=2 groups and wrap
        if (result.Count > 1 && IsNext(result.Last().Last(), result.First().First()))
        {
            var merged = result.Last().Concat(result.First()).ToList();
            result.RemoveAt(result.Count - 1);
            result.RemoveAt(0);
            result.Add(merged);
        }
        
        // accept only 3-6 frame window
        result = result.Where(g => g.Count >= 3 && g.Count < 7).ToList();
        result = result.OrderByDescending(g => g.First()).ToList();
        return result;
    }

    static int FrameCount(int start, int end, int maxFrames = 60)
    {
        return start <= end 
            ? end - start + 1 
            : maxFrames - start + end + 1;
    }

    public static bool LoadForestState(Red gb, int hp, int maxhp, int path, int igts, int igtf)
    {
        string state = hp + "/" + maxhp + " p" + path + " " + igts + "," + igtf;
        if (!WeedleStates.ContainsKey(state))
        {
            RbyIntroSequence intro = new RbyIntroSequence(RbyStrat.NoPal);
            gb.LoadState("basesaves/red/manip/nido.gqs");
            gb.HardReset();
            intro.ExecuteUntilIGT(gb);
            gb.CpuWrite("wPlayTimeMinutes", 5);
            gb.CpuWrite("wPlayTimeSeconds", (byte)igts);
            gb.CpuWrite("wPlayTimeFrames", (byte)igtf);
            intro.ExecuteAfterIGT(gb);
            gb.CpuWriteBE("wPartyMon1HP", (ushort)hp);
            gb.CpuWriteBE("wPartyMon1MaxHP", (ushort)maxhp);
            gb.Execute(SpacePath(NidoPath));
            gb.Yoloball(0, Joypad.B);
            gb.ClearText(Joypad.B);
            gb.Press(Joypad.A);
            gb.RunUntil("_Joypad");
            gb.AdvanceFrame();
            gb.AdvanceFrames(PathFrame(path));
            gb.Press(Joypad.A);
            // gb.AdvanceFrame();
            gb.Press(Joypad.Start);
            if (gb.Execute(SpacePath(Pidgey[path])) != gb.WildEncounterAddress) return false;
            if (gb.EnemyMon.Species.Name != "PIDGEY") return false;
            if (!gb.Yoloball(0, Joypad.B)) return false;
            gb.ClearText(Joypad.A);
            gb.Press(Joypad.B);
            lock (WeedleStates) { WeedleStates[state] = gb.SaveState(); }
        }
        else
        {
            gb.LoadState(WeedleStates[state]);
        }
        return true;
    }

    static (string info, int s1, int s2, int s15, int s9, string npc, List<int> successfulFrames, List<string> frameLogs) Weedle(int p, string forest, int igtf1, int igtf2, int igts, bool print, int atk, int def, int hp, int maxhp, bool antidote=true, List<byte[]> successes = null, int numThreads=6)
    {
        // const int igtf1 = 0, igtf2 = 4;
        int numigt = igtf2 - igtf1 + 1;
        const int weedleframes = 3;
        string[] result = new string[(numigt - IgnoredFramesP2.Count) * weedleframes];
        List<int> successfulFrames = new List<int>();
        int score1 = 0;
        int score2 = 0;
        int score15 = 0;
        int score9 = 0;
        string postFightPath = "LUUUUUUUUUUUUU";
        
        List<string> frameLogs = new List<string>();
        // if(WeedleGbs == null) WeedleGbs = MultiThread.MakeThreads<RedCb>(1); WeedleGbs[0].Record("test");
        // if(WeedleGbs == null) WeedleGbs = MultiThread.MakeThreads<RedCb>(numigt);
        RedCb[] gbs = MultiThread.MakeThreads<RedCb>(numThreads);
        // RedCb gb = new RedCb();
        SortedSet<string> goodnpcs = new SortedSet<string>();
        MultiThread.For(numigt, gbs, (gb, it) =>
        // Trace.WriteLine("----- Squirtle: " + atk + " " + def + " " + hp + "/" + maxhp + " -----");
        // for (int it = 0; it < numigt; ++it)
        {
            int igtf = igtf1 + it;
            if (IgnoredFramesP2.Contains(igtf)) return;
            int weedleSuccess = 0;
            if (!LoadForestState(gb, hp, maxhp, p, igts, igtf))
                return;

            var npcTracker = new NpcTracker<RedCb>(gb.CallbackHandler);
            int adr;
            if (antidote)
            {
                adr = gb.Execute(SpacePath(forest), (gb.Maps[51][25, 12], gb.PickupItem));
            }
            else
            {
                adr = gb.Execute(SpacePath(forest));
            }
            string npcs = npcTracker.GetMovement((50, 2), (51, 1), (51, 8));
            string log2 = "";
            bool enc = false;
            if (adr == gb.WildEncounterAddress)
            {
                if (print) result[it * weedleframes] = igtf + " " + hp + "/" + maxhp + " " + npcs + " " + gb.EnemyMon.ToString();
                // Trace.WriteLine("F" + igtf + " PATHFAIL");
                // return;
                enc = true;
            }
            if (!enc)
            {
                gb.CpuWriteBE("wPartyMon1Attack", (ushort)atk);
                gb.CpuWriteBE("wPartyMon1Defense", (ushort)def);
                gb.Press(Joypad.A);
                gb.ClearText(1);
                gb.Inject(Joypad.None);
                byte[] weedlestate = gb.SaveState();
                for (int weedleframe = 0; weedleframe < weedleframes; ++weedleframe)
                {
                    gb.LoadState(weedlestate);
                    string log = "";
                    log2 = "F" + igtf + " " + npcs + " (" + weedleframe + ")";
                    // if (print) log = igtf + " " + hp + "/" + maxhp + " " + npcs + " (" + weedleframe + ")";
                    gb.AdvanceFrames(weedleframe);
                    gb.Inject(Joypad.B);
                    gb.AdvanceFrame(Joypad.B);
                    gb.ClearText(Joypad.B);
                    int lasthp = hp;
                    bool crit = false;
                    gb.Press(Joypad.A, Joypad.Down, Joypad.A); // t1
                    log += LogTurn(gb);
                    gb.ClearText(Joypad.A);
                    if (gb.BattleMon.HP == hp) Increment(ref score1);
                    if (gb.BattleMon.HP <= lasthp - 5) crit = true;
                    lasthp = gb.BattleMon.HP;
                    gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t2
                    log += LogTurn(gb);
                    gb.ClearText(Joypad.A);
                    if (gb.BattleMon.HP <= lasthp - 5) crit = true;
                    lasthp = gb.BattleMon.HP;
                    if (gb.BattleMon.HP == hp) Increment(ref score2);
                    gb.Press(Joypad.A, Joypad.Up, Joypad.A); // t3
                    log += LogTurn(gb);
                    gb.ClearText(Joypad.A);
                    if (gb.BattleMon.HP <= lasthp - 5) crit = true;
                    lasthp = gb.BattleMon.HP;
                    gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t4
                    log += LogTurn(gb);
                    gb.ClearText(Joypad.A);
                    if (gb.BattleMon.HP <= lasthp - 5) crit = true;
                    lasthp = gb.BattleMon.HP;
                    gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t5
                    log += LogTurn(gb);
                    gb.ClearText(Joypad.A);
                    if (gb.BattleMon.HP <= lasthp - 5) crit = true;
                    gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t6
                    log += LogTurn(gb);
                    if (gb.EnemyMon.HP == 0 && !gb.BattleMon.Poisoned && gb.BattleMon.HP >= hp - 6 && !crit)
                    {
                        // Trace.WriteLine("success");
                        Increment(ref score15);
                        log2 += " SUCCESS";
                        weedleSuccess++;
                        if (igtf > igtf1 && igtf < igtf2) Increment(ref score9);
                        lock (goodnpcs) { goodnpcs.Add(npcs); }
                        if (successes != null) successes.Add(gb.SaveState());
                        gb.ClearTextUntil(Joypad.B, gb.SYM["EnterMap"]);
                        int adr2 = gb.Execute(SpacePath(postFightPath));
                        if (adr2 == gb.WildEncounterAddress)
                        {
                            log2 += " ENC";
                        }
                        else
                        {
                            log2 += " NOENC";
                        }
                    }
                    else
                    {
                        log2 += " FAIL";
                    }
                    if (print) result[it * weedleframes + weedleframe] = log;
                    // Trace.WriteLine(log2);
                }
            }
            if (weedleSuccess == weedleframes)
            {
                lock (successfulFrames) successfulFrames.Add(igtf);
            }
            if (log2 == "") log2 += "F" + igtf + " " + gb.EnemyMon.ToString();
            lock (frameLogs) frameLogs.Add(log2);
        });
        if (print) foreach (string s in result) Trace.WriteLine(s);
        string npc = "";
        foreach (string s in goodnpcs) if (npc == "") npc = s; else npc += " or " + s;
        string info = "1:" + score1 + " 2:" + score2 + " 6:" + score15 + "-" + score9;
        if (print) Trace.WriteLine(info);
        SortFrames(successfulFrames);
        frameLogs.Sort();
        foreach (var gb in gbs) gb.Dispose();
        return (info, score1, score2, score15, score9, npc, successfulFrames, frameLogs);
    }

    static (int score, string npc, Dictionary<int, List<string>> frameLogs) EvalWeedle(int p, string forest, int igtf1, int igtf2, int igts, int atk, int def, int hp, int maxhp, bool antidote=true, List<byte[]> successes = null, int numThreads = 8)
    {
        // check neighbouring frames
        int frameWindow = FrameCount(igtf1, igtf2);
        if (igtf1 == 0) igtf1 = 59;
        else igtf1--;
        if (igtf2 == 59) igtf2 = 0;
        else igtf2++;

        int numigt = FrameCount(igtf1, igtf2);
        const int weedleframes = 3;
        int score = 0;
        // string postFightPath = "LUUUUUUUUUUUUU";
        int APresses = forest.Count(c => c == 'A');
        score += Math.Abs(APresses - 3);
        string ssMiss = "STRING SHOT Miss";
        string ps = "POISON STING";

        Dictionary<int, List<string>> frameLogs = new Dictionary<int, List<string>>();
        // if(WeedleGbs == null) WeedleGbs = MultiThread.MakeThreads<RedCb>(1); WeedleGbs[0].Record("test");
        // if(WeedleGbs == null) WeedleGbs = MultiThread.MakeThreads<RedCb>(numigt);
        RedCb[] gbs = MultiThread.MakeThreads<RedCb>(numThreads);
        // RedCb gb = new RedCb();
        SortedSet<string> goodnpcs = new SortedSet<string>();
        MultiThread.For(numigt, gbs, (gb, it) =>
        // Trace.WriteLine("----- Squirtle: " + atk + " " + def + " " + hp + "/" + maxhp + " -----");
        // for (int it = 0; it < numigt; ++it)
        {
            int igtf = (igtf1 + it) % 60;
            if (IgnoredFramesP2.Contains(igtf)) return;
            if (!LoadForestState(gb, hp, maxhp, p, igts, igtf))
                return;

            var npcTracker = new NpcTracker<RedCb>(gb.CallbackHandler);
            int adr;
            if (antidote)
            {
                adr = gb.Execute(SpacePath(forest), (gb.Maps[51][25, 12], gb.PickupItem));
            }
            else
            {
                adr = gb.Execute(SpacePath(forest));
            }
            string npcs = npcTracker.GetMovement((50, 2), (51, 1), (51, 8));
            bool enc = false;
            if (adr == gb.WildEncounterAddress)
            {
                enc = true;
            }
            if (!enc)
            {
                score+=3;
                if (igtf == igtf1 || igtf == igtf2) return;
                gb.CpuWriteBE("wPartyMon1Attack", (ushort)atk);
                gb.CpuWriteBE("wPartyMon1Defense", (ushort)def);
                gb.Press(Joypad.A);
                gb.ClearText(1);
                gb.Inject(Joypad.None);
                byte[] weedlestate = gb.SaveState();
                for (int weedleframe = 0; weedleframe < weedleframes; ++weedleframe)
                {
                    gb.LoadState(weedlestate);
                    string log = "";
                    string turn = "";
                    // if (print) log = igtf + " " + hp + "/" + maxhp + " " + npcs + " (" + weedleframe + ")";
                    gb.AdvanceFrames(weedleframe);
                    gb.Inject(Joypad.B);
                    gb.AdvanceFrame(Joypad.B);
                    gb.ClearText(Joypad.B);
                    int lasthp = hp;
                    bool crit = false;
                    gb.Press(Joypad.A, Joypad.Down, Joypad.A); // t1
                    turn = LogTurn(gb);
                    log += turn;
                    if (turn.Contains(ssMiss)) score++;
                    else if (turn.Contains(ps)) score--;
                    gb.ClearText(Joypad.A);
                    if (gb.BattleMon.HP <= lasthp - 5) crit = true;
                    lasthp = gb.BattleMon.HP;
                    gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t2
                    turn = LogTurn(gb);
                    log += turn;
                    if (turn.Contains(ssMiss)) score++;
                    else if (turn.Contains(ps)) score--;
                    gb.ClearText(Joypad.A);
                    if (gb.BattleMon.HP <= lasthp - 5) crit = true;
                    lasthp = gb.BattleMon.HP;
                    gb.Press(Joypad.A, Joypad.Up, Joypad.A); // t3
                    turn = LogTurn(gb);
                    log += turn;
                    if (turn.Contains(ssMiss)) score++;
                    else if (turn.Contains(ps)) score--;
                    gb.ClearText(Joypad.A);
                    if (gb.BattleMon.HP <= lasthp - 5) crit = true;
                    lasthp = gb.BattleMon.HP;
                    gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t4
                    turn = LogTurn(gb);
                    log += turn;
                    if (turn.Contains(ssMiss)) score++;
                    else if (turn.Contains(ps)) score--;
                    gb.ClearText(Joypad.A);
                    if (gb.BattleMon.HP <= lasthp - 5) crit = true;
                    lasthp = gb.BattleMon.HP;
                    gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t5
                    turn = LogTurn(gb);
                    log += turn;
                    if (turn.Contains(ssMiss)) score++;
                    else if (turn.Contains(ps)) score--;
                    gb.ClearText(Joypad.A);
                    if (gb.BattleMon.HP <= lasthp - 5) crit = true;
                    gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t6
                    turn = LogTurn(gb);
                    log += turn;
                    if (turn.Contains(ssMiss)) score++;
                    else if (turn.Contains(ps)) score--;
                    if (gb.EnemyMon.HP == 0 && !gb.BattleMon.Poisoned && gb.BattleMon.HP >= hp - 6 && !crit)
                    {
                        if (gb.BattleMon.HP == hp) score += 3;
                        else if (gb.BattleMon.HP == hp - 3) score += 2;
                        else score++;
                        // Trace.WriteLine("success");
                        lock (goodnpcs) { goodnpcs.Add(npcs); }
                        if (successes != null) successes.Add(gb.SaveState());
                        lock (frameLogs)
                        {
                            if (!frameLogs.ContainsKey(igtf)) frameLogs[igtf] = new List<string>();
                            frameLogs[igtf].Add(log);
                        }
                        // int adr2 = gb.Execute(SpacePath(postFightPath));
                        // if (adr2 == gb.WildEncounterAddress)
                        // {
                        //     log2 += " ENC";
                        // }
                        // else
                        // {
                        //     log2 += " NOENC";
                        // }
                    }
                    // Trace.WriteLine(log2);
                }
            }
            else
            {
                if (igtf == igtf1 || igtf == igtf2)
                {
                    if (frameWindow == 3) score -= 5;
                    else if (frameWindow == 4) score -= 2;
                }
            }
        });
        string npc = "";
        foreach (string s in goodnpcs) if (npc == "") npc = s; else npc += " or " + s;
        if (successes.Count < 9) score = -9999;
        foreach (var gb in gbs) gb.Dispose();
        return (score, npc, frameLogs);
    }

    static (int success, string npc, Dictionary<int, List<string>> frameLogs) EvalWeedle2(int p, string forest, string postFight, int igtf1, int igtf2, int igts, int atk, int def, int hp, int maxhp, bool antidote=true, List<byte[]> successes = null, int numThreads=7)
    {
        // check neighbouring frames
        int numigt = FrameCount(igtf1, igtf2);
        const int weedleframes = 3;
        int success = 0;

        Dictionary<int, List<string>> frameLogs = new Dictionary<int, List<string>>();
        // if(WeedleGbs == null) WeedleGbs = MultiThread.MakeThreads<RedCb>(1); WeedleGbs[0].Record("test");
        // if(WeedleGbs == null) WeedleGbs = MultiThread.MakeThreads<RedCb>(numigt);
        // RedCb[] gbs = MultiThread.MakeThreads<RedCb>(numThreads);
        RedCb gb = new RedCb();
        SortedSet<string> goodnpcs = new SortedSet<string>();
        // MultiThread.For(numigt, gbs, (gb, it) =>
        // Trace.WriteLine("----- Squirtle: " + atk + " " + def + " " + hp + "/" + maxhp + " -----");
        for (int it = 0; it < numigt; ++it)
        {
            int igtf = (igtf1 + it) % 60;
            if (IgnoredFramesP2.Contains(igtf)) continue;
            if (!LoadForestState(gb, hp, maxhp, p, igts, igtf))
                continue;

            var npcTracker = new NpcTracker<RedCb>(gb.CallbackHandler);
            int adr;
            if (antidote)
            {
                adr = gb.Execute(SpacePath(forest), (gb.Maps[51][25, 12], gb.PickupItem));
            }
            else
            {
                adr = gb.Execute(SpacePath(forest));
            }
            string npcs = npcTracker.GetMovement((50, 2), (51, 1), (51, 8));
            bool enc = false;
            if (adr == gb.WildEncounterAddress)
            {
                enc = true;
            }
            if (!enc)
            {
                // score += 3;
                gb.CpuWriteBE("wPartyMon1Attack", (ushort)atk);
                gb.CpuWriteBE("wPartyMon1Defense", (ushort)def);
                gb.Press(Joypad.A);
                gb.ClearText(1);
                gb.Inject(Joypad.None);
                byte[] weedlestate = gb.SaveState();
                for (int weedleframe = 0; weedleframe < weedleframes; ++weedleframe)
                {
                    gb.LoadState(weedlestate);
                    string log = "";
                    string turn = "";
                    // if (print) log = igtf + " " + hp + "/" + maxhp + " " + npcs + " (" + weedleframe + ")";
                    gb.AdvanceFrames(weedleframe);
                    gb.Inject(Joypad.B);
                    gb.AdvanceFrame(Joypad.B);
                    gb.ClearText(Joypad.B);
                    int lasthp = hp;
                    bool crit = false;
                    gb.Press(Joypad.A, Joypad.Down, Joypad.A); // t1
                    turn = LogTurn(gb);
                    log += turn;
                    gb.ClearText(Joypad.A);
                    if (gb.BattleMon.HP <= lasthp - 5) crit = true;
                    lasthp = gb.BattleMon.HP;
                    gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t2
                    turn = LogTurn(gb);
                    log += turn;
                    gb.ClearText(Joypad.A);
                    if (gb.BattleMon.HP <= lasthp - 5) crit = true;
                    lasthp = gb.BattleMon.HP;
                    gb.Press(Joypad.A, Joypad.Up, Joypad.A); // t3
                    turn = LogTurn(gb);
                    log += turn;
                    gb.ClearText(Joypad.A);
                    if (gb.BattleMon.HP <= lasthp - 5) crit = true;
                    lasthp = gb.BattleMon.HP;
                    gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t4
                    turn = LogTurn(gb);
                    log += turn;
                    gb.ClearText(Joypad.A);
                    if (gb.BattleMon.HP <= lasthp - 5) crit = true;
                    lasthp = gb.BattleMon.HP;
                    gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t5
                    turn = LogTurn(gb);
                    log += turn;
                    gb.ClearText(Joypad.A);
                    if (gb.BattleMon.HP <= lasthp - 5) crit = true;
                    gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t6
                    turn = LogTurn(gb);
                    log += turn;
                    if (gb.EnemyMon.HP == 0 && !gb.BattleMon.Poisoned && gb.BattleMon.HP >= hp - 6 && !crit)
                    {
                        if (postFight != "")
                        {
                            gb.ClearTextUntil(Joypad.B, gb.SYM["EnterMap"]);
                            adr = gb.Execute(SpacePath(postFight));
                            if (adr != gb.WildEncounterAddress) Increment(ref success);
                        }
                        else
                        {
                            Increment(ref success);
                            lock (goodnpcs) { goodnpcs.Add(npcs); }
                            if (successes != null) successes.Add(gb.SaveState());
                            lock (frameLogs)
                            {
                                if (!frameLogs.ContainsKey(igtf)) frameLogs[igtf] = new List<string>();
                                frameLogs[igtf].Add(log);
                            }
                        }
                        // int adr2 = gb.Execute(SpacePath(postFightPath));
                        // if (adr2 == gb.WildEncounterAddress)
                        // {
                        //     log2 += " ENC";
                        // }
                        // else
                        // {
                        //     log2 += " NOENC";
                        // }
                    }
                    // Trace.WriteLine(log2);
                }
            }
        }
        // );
        string npc = "";
        foreach (string s in goodnpcs) if (npc == "") npc = s; else npc += " or " + s;
        gb.Dispose();
        return (success, npc, frameLogs);
    }

    public static List<(int hp, int maxhp, int atk, int def)> ReadWeedleStats()
    {
        var paths = new List<(int hp, int maxhp, int atk, int def)>();
        foreach (string line in File.ReadAllLines("weedle/wpaths.txt"))
        {
            var r1 = Regex.Match(line, @"([0-9]+) ([0-9]+) ([0-9]+)/([0-9]+) p([0-9]+)");
            if (r1.Success)
                paths.Add((int.Parse(r1.Groups[3].Value), int.Parse(r1.Groups[4].Value), int.Parse(r1.Groups[1].Value), int.Parse(r1.Groups[2].Value)));
        }
        return paths;
    }

    void WeedleRecordTest()
    {
        RedCb gb = new RedCb();
        int hp = 18;
        int maxhp = 23;
        int atk = 11;
        int def = 12;
        string postFightPath = "LUUUUUUUUUUUUU";
        LoadForestState(gb, hp, maxhp, 2, 49, 0);
        var npcTracker = new NpcTracker<RedCb>(gb.CallbackHandler);
        int adr = gb.Execute(SpacePath(Forest[2]), (gb.Maps[51][25, 12], gb.PickupItem));
        string npcs = npcTracker.GetMovement((50, 2), (51, 1), (51, 8));
        if (adr == gb.WildEncounterAddress)
        {
            return;
        }
        gb.CpuWriteBE("wPartyMon1Attack", (ushort)atk);
        gb.CpuWriteBE("wPartyMon1Defense", (ushort)def);
        gb.Record("weedletest");
        gb.Press(Joypad.A);
        gb.ClearText(1);
        gb.Inject(Joypad.None);
        gb.AdvanceFrames(1);
        gb.Inject(Joypad.B);
        gb.AdvanceFrame(Joypad.B);
        gb.ClearText(Joypad.B);
        int lasthp = hp;
        int lastEnemyHp = gb.EnemyMon.HP;
        bool crit = false;
        bool tackleCrit = false;
        string log = "";
        gb.Press(Joypad.A, Joypad.Down, Joypad.A); // t1
        log += LogTurn(gb);
        gb.ClearText(Joypad.A);
        if (gb.BattleMon.HP <= lasthp - 5) crit = true;
        lasthp = gb.BattleMon.HP;
        gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t2
        log += LogTurn(gb);
        gb.ClearText(Joypad.A);
        if (gb.BattleMon.HP <= lasthp - 5) crit = true;
        lasthp = gb.BattleMon.HP;
        gb.Press(Joypad.A, Joypad.Up, Joypad.A); // t3
        log += LogTurn(gb);
        gb.ClearText(Joypad.A);
        if (gb.BattleMon.HP <= lasthp - 5) crit = true;
        if (gb.EnemyMon.HP >= lastEnemyHp - 5) tackleCrit = true;
        lastEnemyHp = gb.EnemyMon.HP;
        lasthp = gb.BattleMon.HP;
        gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t4
        log += LogTurn(gb);
        gb.ClearText(Joypad.A);
        if (gb.BattleMon.HP <= lasthp - 5) crit = true;
        if (gb.EnemyMon.HP >= lastEnemyHp - 5) tackleCrit = true;
        lastEnemyHp = gb.EnemyMon.HP;
        lasthp = gb.BattleMon.HP;
        gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t5
        log += LogTurn(gb);
        gb.ClearText(Joypad.A);
        if (gb.BattleMon.HP <= lasthp - 5) crit = true;
        if (gb.EnemyMon.HP >= lastEnemyHp - 5) tackleCrit = true;
        lastEnemyHp = gb.EnemyMon.HP;
        gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t6
        log += LogTurn(gb);
        if (gb.EnemyMon.HP == 0 && !gb.BattleMon.Poisoned && gb.BattleMon.HP >= hp - 6 && !crit && !tackleCrit)
        {
            Trace.WriteLine("Success");
            gb.ClearTextUntil(Joypad.B, gb.SYM["EnterMap"]);
            int adr2 = gb.Execute(SpacePath(postFightPath));
            if (adr2 == gb.WildEncounterAddress)
            {
                Trace.WriteLine(gb.EnemyMon.ToString());
                gb.Dispose();
                Trace.WriteLine(log);
                Trace.WriteLine(npcs);
                return;
            }
        }
        Trace.WriteLine(log);
        Trace.WriteLine(npcs);
        gb.Dispose();
    }

    static Dictionary<(int hp, int maxhp, int atk, int def), List<string>> ParseLogs(string file)
    {
        var paths = new Dictionary<(int hp, int maxhp, int atk, int def), List<string>>();
        var currStats = (0, 0, 0, 0);
        foreach (string line in System.IO.File.ReadAllLines(file))
        {
            var r1 = Regex.Match(line, @"([0-9]+) ([0-9]+) ([0-9]+)/([0-9]+)");
            var r2 = Regex.Match(line, "([LRUDA]*)");
            if (r1.Success) currStats = (int.Parse(r1.Groups[3].Value), int.Parse(r1.Groups[4].Value), int.Parse(r1.Groups[1].Value), int.Parse(r1.Groups[2].Value));
            else
            {
                if (!paths.ContainsKey(currStats)) paths[currStats] = new List<string>();
                paths[currStats].Add(r2.Groups[1].Value);
            }
        }
        return paths;
    }
    
    void CheckWeedlePaths(string inputFile, string outputFile, bool antidote)
    {
        var paths = ParseLogs(inputFile);
        string link = "https://gunnermaniac.com/pokeworld?local=51#21/59/";
        var listener = new TextWriterTraceListener(File.CreateText(outputFile));
        Trace.Listeners.Add(listener);
        Trace.WriteLine("Stats with Paths: " + paths.Count);
        int igtf1 = 0;
        int igtf2 = 59;
        // Dictionary<(int hp, int maxhp, int atk, int def), List<string>> frameRes = new Dictionary<(int hp, int maxhp, int atk, int def), List<string>>();
        // var paths2 = ParseLogs2();
        foreach (var stat in paths)
        {
            int atk = stat.Key.atk, def = stat.Key.def, hp = stat.Key.hp, maxhp = stat.Key.maxhp;
            // if (hp >= 15) continue;
            // if (paths2.Contains(stat.Key)) continue;
            Trace.WriteLine(atk + " " + def + " " + hp + "/" + maxhp);
            foreach (var path in stat.Value)
            {
                Trace.WriteLine(link + path);
                var res = Weedle(2, path, igtf1, igtf2, 30, false, atk, def, hp, maxhp, antidote);
                var successfulFrames = res.successfulFrames;
                Trace.Write(String.Join(",", successfulFrames));
                Trace.WriteLine("\n");
                // if (!frameRes.ContainsKey(stat.Key)) frameRes[stat.Key] = new List<string>();
                // frameRes[stat.Key] = res.frameLogs;
            }
        }
        // var outputLines = new List<string>();
        // foreach (var frameLog in frameRes)
        // {
        //     outputLines.Add(frameLog.Key.atk + " " + frameLog.Key.def + " " + frameLog.Key.hp + "/" + frameLog.Key.maxhp);
        //     foreach (var frame in frameLog.Value) outputLines.Add(frame);
        //     outputLines.Add("");
        // }
        // System.IO.File.WriteAllLines("p2f2_framelogs.txt", outputLines);
        Trace.Listeners.Remove(listener);
        listener.Close();
        paths.Clear();
    }

    static List<(int hp, int maxhp, int atk, int def)> ParseLogs2()
    {
        var stats = new List<(int hp, int maxhp, int atk, int def)>();
        foreach (string line in System.IO.File.ReadAllLines("weedle/p2f0/p2f0_frames_lt15hp_unfinished.txt"))
        {
            var r1 = Regex.Match(line, @"([0-9]+) ([0-9]+) ([0-9]+)/([0-9]+)");
            if (r1.Success) stats.Add((int.Parse(r1.Groups[3].Value), int.Parse(r1.Groups[4].Value), int.Parse(r1.Groups[1].Value), int.Parse(r1.Groups[2].Value)));
        }
        return stats;
    }

    static Dictionary<(int hp, int maxhp, int atk, int def), Dictionary<string, List<List<int>>>> ParseSuccessfulFrames(string file)
    {
        var paths = new Dictionary<(int hp, int maxhp, int atk, int def), Dictionary<string, List<List<int>>>>();
        var currStats = (0, 0, 0, 0);
        var currPath = "";
        foreach (string line in File.ReadAllLines(file))
        {
            var r1 = Regex.Match(line, @"([0-9]+) ([0-9]+) ([0-9]+)/([0-9]+)");
            var r2 = Regex.Match(line, "([LRUDA]+)");
            var r3 = Regex.Match(line, @"^\d{1,2}(?:,\d{1,2})*$");
            if (r1.Success) currStats = (int.Parse(r1.Groups[3].Value), int.Parse(r1.Groups[4].Value), int.Parse(r1.Groups[1].Value), int.Parse(r1.Groups[2].Value));
            else if (r2.Success)
            {
                if (!paths.ContainsKey(currStats)) paths[currStats] = new Dictionary<string, List<List<int>>>();
                currPath = r2.Groups[1].Value;
                if (!paths[currStats].ContainsKey(currPath)) paths[currStats][currPath] = new List<List<int>>();
            }
            else if (r3.Success)
            {
                var numbers = r3.Value.Split(',').Select(int.Parse);
                var contigousFrames = GroupContiguousFrames(numbers);
                paths[currStats][currPath].AddRange(contigousFrames);
            }
        }
        foreach (var stat in paths)
        {
            foreach (var path in stat.Value)
            {
                if (path.Value.Count == 0) paths[stat.Key].Remove(path.Key);

            }
        }
        return paths;
    }

    static List<List<int>> LargestFrameClusters(List<List<int>> lists)
    {
        if (lists.Count == 0) return new List<List<int>>();

        // accept 3-6 windows
        int maxLength = lists.Max(l => l.Count);
        if (maxLength == 4) return lists.Where(l => l.Count >= maxLength - 1).ToList();
        else if (maxLength == 5) return lists.Where(l => l.Count >= maxLength - 2).ToList();
        else if(maxLength == 6) return lists.Where(l => l.Count >= maxLength - 3).ToList();
        else return lists.Where(l => l.Count == maxLength).ToList();
    }

    static List<string> SeparateForestPath(string path)
    {
        List<string> separated = new List<string>();
        int x = 0;
        int index1 = 0;
        int index2 = 0;
        int APressGate = 0;
        for (int i = 0; i < path.Length; i++)
        {
            if (x >= 10 && x < 18 && path[i] == 'A')
            {
                APressGate++;
                continue;
            }
            if (x < 10 && path[i] == 'A') continue;
            x++;
            if (x == 10) index1 = i + 1;
            if (x == 18) index2 = i + 1;
        }
        string route2 = path.Substring(0, index1);
        string gate = path.Substring(index1, 8 + APressGate);
        string forest = path.Substring(index2);
        separated.Add(route2);
        separated.Add(gate);
        separated.Add(forest);
        return separated;
    }

    static void FindBestWeedlePaths(string inputFile, string outputFile, int pidgeypath, bool antidote)
    {
        string link = "https://gunnermaniac.com/pokeworld?local=51#21/59/";
        var paths = ParseSuccessfulFrames(inputFile);
        var finalPaths = new Dictionary<string, PathInfo>();
        int igtSecDeduction = 4;

        List<(int hp, int maxhp, int atk, int def)> seenStats = new List<(int hp, int maxhp, int atk, int def)>();
        if (File.Exists(outputFile))
        {
            string json = File.ReadAllText(outputFile);
            var pathData = JsonSerializer.Deserialize<Dictionary<string, PathInfo>>(json);
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
            finalPaths = pathData;
        }

        foreach (var stat in paths)
        {
            if (seenStats.Count > 0 && seenStats.Contains(stat.Key)) continue;
            Trace.WriteLine(stat.Key);
            // if (stat.Key != (15, 22, 11, 13)) continue;
            int atk = stat.Key.atk, def = stat.Key.def, hp = stat.Key.hp, maxhp = stat.Key.maxhp;
            // if (stat.Key == (15, 22, 11, 13)) Debugger.Break();
            string bestPath = "";
            int bestScore = -9999;
            int currLargestWindow = 0;
            Dictionary<int, List<string>> bestFrameLogs = new Dictionary<int, List<string>>();
            string bestNpcs = "";
            List<int> bestFrames = new List<int>();
            List<byte[]> bestStates = new List<byte[]>();
            SortedSet<int> bestIgtSecs = new SortedSet<int>();
            foreach (var path in stat.Value)
            {
                var clusters = LargestFrameClusters(path.Value);
                if (clusters.Count == 1)
                {
                    int igtf1 = clusters[0][0], igtf2 = clusters[0][clusters[0].Count - 1];
                    int frameWindow = FrameCount(igtf1, igtf2);
                    if (currLargestWindow >= frameWindow && bestIgtSecs.Count <= 7) continue;
                    currLargestWindow = Math.Max(currLargestWindow, frameWindow);
                    List<byte[]> states = new List<byte[]>();
                    var res = EvalWeedle(pidgeypath, path.Key, igtf1, igtf2, 30, atk, def, hp, maxhp, antidote, successes: states);
                    int successes = states.Count;
                    SortedSet<int> igtSecs;
                    if (successes >= 9)
                    {
                        igtSecs = CheckWeedleIgtSecond(pidgeypath, path.Key, igtf1, igtf2, atk, def, hp, maxhp, successes, antidote);
                        int score = res.score - (igtSecs.Count * igtSecDeduction);
                        // int score = res.score;
                        if (score > bestScore)
                        {
                            bestPath = path.Key;
                            bestScore = score;
                            bestFrameLogs = res.frameLogs;
                            bestNpcs = res.npc;
                            bestFrames = clusters[0];
                            bestStates = states;
                            bestIgtSecs = igtSecs;
                        }
                    }
                }
                else
                {
                    List<int> bestCluster = new List<int>();
                    int bestClusterScore = -9999;
                    Dictionary<int, List<string>> bestClusterFrameLogs = new Dictionary<int, List<string>>();
                    string bestClusterNpcs = "";
                    List<byte[]> bestClusterStates = new List<byte[]>();
                    SortedSet<int> bestClusterIgtSecs = new SortedSet<int>();
                    foreach (var cluster in clusters)
                    {
                        int igtf1 = cluster[0], igtf2 = cluster[cluster.Count - 1];
                        int frameWindow = FrameCount(igtf1, igtf2);
                        if (currLargestWindow >= frameWindow && bestIgtSecs.Count <= 7) continue;
                        currLargestWindow = Math.Max(currLargestWindow, frameWindow);
                        List<byte[]> states = new List<byte[]>();
                        var res = EvalWeedle(pidgeypath, path.Key, igtf1, igtf2, 30, atk, def, hp, maxhp, successes: states);
                        int successes = states.Count;
                        SortedSet<int> igtSecs;
                        if (successes >= 9)
                        {
                            igtSecs = CheckWeedleIgtSecond(pidgeypath, path.Key, igtf1, igtf2, atk, def, hp, maxhp, successes, antidote);
                            int score = res.score - (igtSecs.Count * igtSecDeduction);
                            // int score = res.score;
                            if (score > bestClusterScore)
                            {
                                bestClusterScore = score;
                                bestCluster = cluster;
                                bestClusterFrameLogs = res.frameLogs;
                                bestClusterNpcs = res.npc;
                                bestClusterStates = states;
                                bestClusterIgtSecs = igtSecs;
                            }
                        }
                    }
                    if (bestClusterScore > bestScore)
                    {
                        bestPath = path.Key;
                        bestScore = bestClusterScore;
                        bestFrameLogs = bestClusterFrameLogs;
                        bestNpcs = bestClusterNpcs;
                        bestFrames = bestCluster;
                        bestStates = bestClusterStates;
                        bestIgtSecs = bestClusterIgtSecs;
                    }
                }
            }

            if (bestPath != "" && bestStates.Count >= 9)
            {
                string postFight = ExtWeedleSearch.SearchPostWeedle(8, 42, bestStates);
                List<string> separatedPaths = SeparateForestPath(bestPath);
                string keyString = "(" + hp + "," + maxhp + "," + atk + "," + def + ")";
                string pathString = "";
                if (antidote) pathString = pidgeypath + "a";
                else pathString = pidgeypath + "b";
                if (!finalPaths.ContainsKey(keyString))
                {
                    finalPaths[keyString] = new PathInfo
                    {
                        route2 = separatedPaths[0],
                        gate = separatedPaths[1],
                        forest = separatedPaths[2],
                        postFight = postFight,
                        link = link + bestPath + postFight,
                        npcs = bestNpcs,
                        path = pathString,
                        frames = bestFrames,
                        igtSecs = bestIgtSecs.ToList(),
                        score = bestScore,
                        frameLogs = bestFrameLogs
                    };

                    using var stream = File.Create(outputFile);
                    JsonSerializer.Serialize(stream, finalPaths, new JsonSerializerOptions { WriteIndented = true });
                }
            }
        }
        finalPaths.Clear();
        paths.Clear();
    }

    static void RecheckWeedleIgtSecond(string inputFile, string outputFile, bool antidote)
    {
        string json = File.ReadAllText(inputFile);
        var pathData = JsonSerializer.Deserialize<Dictionary<string, PathInfo>>(json);
        foreach (var stat in pathData)
        {
            var pathInfo = stat.Value;
            string path = pathInfo.route2 + pathInfo.gate + pathInfo.forest;
            string postFight = pathInfo.postFight;
            if (postFight == "") continue;
            var values = stat.Key.Trim('(', ')').Split(',');
            int hp = int.Parse(values[0]);
            int maxhp = int.Parse(values[1]);
            int atk = int.Parse(values[2]);
            int def = int.Parse(values[3]);
            var statKey = (hp, maxhp, atk, def);
            Console.WriteLine(statKey);
            SortedSet<int> set = new SortedSet<int>();
            var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 2 };
            Parallel.For(0, 60, options, s =>
            {
                var r = EvalWeedle2(2, path, postFight, pathInfo.frames[0], pathInfo.frames[pathInfo.frames.Count - 1], s, atk, def, hp, maxhp, antidote);
                int successes = pathInfo.frames.Count * 3;
                Console.WriteLine("IGT Second: " + s);
                Console.WriteLine("Original success: " + successes);
                Console.WriteLine("New success: " + r.success);
                Console.WriteLine(" ");
                if (r.success < successes)
                    lock (set) { set.Add(s); }
            });
            int score = pathData[stat.Key].score;
            var oldIgtSecs = pathData[stat.Key].igtSecs;
            // Console.WriteLine(set.ToList().Count - oldIgtSecs.ToList().Count);
            pathData[stat.Key].score = score - ((set.ToList().Count - oldIgtSecs.ToList().Count) * 4);
            pathData[stat.Key].igtSecs = set.ToList();
        }
        using var stream = File.Create(outputFile);
        JsonSerializer.Serialize(stream, pathData, new JsonSerializerOptions { WriteIndented = true });
        pathData.Clear();
    }

    static SortedSet<int> CheckWeedleIgtSecond(int p, string path, int igtf1, int igtf2, int atk, int def, int hp, int maxhp, int successes, bool antidote)
    {
        SortedSet<int> set = new SortedSet<int>();
        Parallel.For(0, 60, s =>
        {
            var r = EvalWeedle2(p, path, "", igtf1, igtf2, s, atk, def, hp, maxhp, antidote);
            if (r.success < successes)
                lock (set) { set.Add(s); }
        });
        return set;
    }

    void ComparePathScores(string file1, string file2, string outputFile)
    {
        string json1 = File.ReadAllText(file1);
        var pathData1 = JsonSerializer.Deserialize<Dictionary<string, PathInfo>>(json1);
        string json2 = File.ReadAllText(file2);
        var pathData2 = JsonSerializer.Deserialize<Dictionary<string, PathInfo>>(json2);
        var mergedPaths = new Dictionary<string, PathInfo>();
    
        foreach (var stat in pathData1)
        {
            mergedPaths[stat.Key] = stat.Value;
        }
        
        foreach (var stat in pathData2)
        {
            string key = stat.Key;
            PathInfo path2 = stat.Value;
            
            if (mergedPaths.ContainsKey(key))
            {
                if (path2.score > mergedPaths[key].score)
                {
                    mergedPaths[key] = path2;
                }
            }
            else
            {
                mergedPaths[key] = path2;
            }
        }
        
        using var stream = File.Create(outputFile);
        JsonSerializer.Serialize(stream, mergedPaths, new JsonSerializerOptions { WriteIndented = true });
    }

    public ExtendedWeedle()
    {

        // "UULLLLLUUUUUUURAUUAUUUUURRRURURRRRUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUULLLLLLLLDDDDDDADLLLLUUUUUUUUUUUUULLLLLLDDDDDDDDDDDDDDLDDDDDLLLLUUU"
        // WeedleRecordTest();
        // WeedleExt();
        // CheckWeedlePaths();


        // string json = File.ReadAllText("p2.json");
        // var data = JsonSerializer.Deserialize<Dictionary<string, PathInfo>>(json);
        // Console.WriteLine(data.Count);


        // Console.WriteLine("no paths: " + noPaths);
        // Console.WriteLine("Paths: " + numPaths);

        // string link = "https://gunnermaniac.com/pokeworld?local=51#21/59/";
        // List<byte[]> states = new List<byte[]>();
        // string forest = "UUAUULLLLLUUUUAUURUAUUUUURRRRUURRRRUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUULLLLLLLLDDDDDDDLLLLUUUUUUUUUUUUULLLLLLDDDDDDDDDDDDDLDDDDDDLLLLUUU";
        // foreach (var path in SeparateForestPath(forest)) Console.WriteLine(path);
        // var res = EvalWeedle(2, forest, 58, 0, 30, 12, 13, 19, 22, states);
        // // Console.WriteLine("score: " + res.score + " npc: " + res.npc);
        // string postFight = ExtWeedleSearch.SearchPostWeedle(8, 4, states);
        // Console.WriteLine(link + forest + postFight);

        // RecheckWeedleIgtSecond();
        // string json = File.ReadAllText("p2c.json");
        // var pathData = JsonSerializer.Deserialize<Dictionary<string, PathInfo>>(json);
        // string forest = "UUALLLLLUUAUUUURUUUUUUUURRRRRRRRUUUUUUUUUUUUUUAUUUUUUUUUUUUUUUUUUUULLLLLLLLDDDDDDDLLLLUUUUUUUUUUUUULLLLLLDDDDDDDDDDDDDDDDDDLDLLLLUUU";
        // List<byte[]> states = new List<byte[]>();
        // var res = EvalWeedle2(2, forest, 2, 7, 30, 12, 13, 15, 21, states);
        // Console.WriteLine("successes: " + res.success);
        // Console.WriteLine("state count: " + states.Count);

        // CheckWeedlePaths();
        // var pathFrames = ParseSuccessfulFrames("weedle/p2f6_noanti/p2f6g2_frames.txt");
        // foreach (var stat in pathFrames)
        // {
        //     bool existsPath = false;
        //     foreach (var path in stat.Value)
        //     {
        //         if (path.Value.Count >= 1) existsPath = true;
        //     }
        //     if (!existsPath) Trace.WriteLine(stat.Key);
        // }
        // FindBestWeedlePaths();
        // RecheckWeedleIgtSecond();

        // FindBestWeedlePaths("weedle/p2f0/p2f0_frames.txt", "weedle/p2f0/p2f0.json", true);
        // RecheckWeedleIgtSecond("weedle/p2f0/p2f0.json", "weedle/p2f0/p2f0_sec.json", true);

        // CheckWeedlePaths("weedle/p2f6_noanti/p2f6g2_paths.txt", "weedle/p2f6_noanti/p2f6g2_frames.txt", false);
        // FindBestWeedlePaths("weedle/p2f6_noanti/p2f6g2_frames.txt", "weedle/p2f6_noanti/p2f6g2.json", false);
        // RecheckWeedleIgtSecond("weedle/p2f6_noanti/p2f6g2.json", "weedle/p2f6_noanti/p2f6g2_secs.json", false);

        // FindBestWeedlePaths("weedle/p2f6_noanti/p2f6g3_frames.txt", "weedle/p2f6_noanti/p2f6g3.json", false);
        // RecheckWeedleIgtSecond("weedle/p2f6_noanti/p2f6g3.json", "weedle/p2f6_noanti/p2f6g3_secs.json", false);

        ComparePathScores("weedle/p2f0/p2f0_sec.json", "weedle/p2f7_anti/p2f7g3_sec.json", "p2_anti.json");
    }
}