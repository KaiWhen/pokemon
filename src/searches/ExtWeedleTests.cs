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

    static PathInfo createPathInfo(int pidgeypath, string path, string postFight, int score, string link, string npcs, List<int> frames, SortedSet<int> igtSecs, Dictionary<int, List<string>> frameLogs, bool antidote)
    {
        List<string> separatedPaths = SeparateForestPath(path);
        string pathString;
        if (antidote) pathString = pidgeypath + "a";
        else pathString = pidgeypath + "b";
        PathInfo res = new PathInfo
        {
            route2 = separatedPaths[0],
            gate = separatedPaths[1],
            forest = separatedPaths[2],
            postFight = postFight,
            link = link + path + postFight,
            npcs = npcs,
            path = pathString,
            frames = frames,
            igtSecs = igtSecs.ToList(),
            score = score,
            frameLogs = frameLogs
        };

        return res;
    }
    
    static Dictionary<string, byte[]> WeedleStates = new Dictionary<string, byte[]>();

    static int PathFrame(int path)
    {
        return path > 2 ? path + 1 : path;
    }

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
    
    static List<List<int>> GroupContiguousIGT(IEnumerable<int> nums, bool frames)
    {
        var numSet = new HashSet<int>(nums.Select(f => f % 60));
        if (numSet.Count == 0) return new List<List<int>>();
        var sorted = numSet.OrderBy(f => f).ToList();
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

        // accept only 3-5 frame window
        if (frames)
        {
            result = result.Where(g => g.Count >= 3 && g.Count < 6).ToList();
            result = result.OrderByDescending(g => g.First()).ToList();
            return result;
        }
        else
        {
            // group seconds
            result = result.Where(g => g.Count >= 2).ToList();
            return result;
        }
    }

    public static int FrameCount(int start, int end)
    {
        return start <= end
            ? end - start + 1
            : 60 - start + end + 1;
    }
    
    static Dictionary<int, List<string>> SortFrameLogs(Dictionary<int, List<string>> frameLogs)
    {
        var res = frameLogs
        .OrderBy(kvp =>
        {
            var frame = kvp.Key % 60;
            return frame < 50 ? frame + 60 : frame;
        }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return res;
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
    
    public static bool LoadForestState(Red gb, int hp, int maxhp, int framesToWait, string path, int igts, int igtf)
    {
        string state = hp + "/" + maxhp + " p" + framesToWait + " " + igts + "," + igtf;
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
            gb.AdvanceFrames(framesToWait);
            gb.Press(Joypad.A);
            // gb.AdvanceFrame();
            gb.Press(Joypad.Start);
            if (gb.Execute(SpacePath(path)) != gb.WildEncounterAddress) return false;
            if (gb.EnemyMon.Species.Name != "PIDGEY") return false;
            if (!gb.Yoloball(0, Joypad.B)) return false;
            gb.ClearText(Joypad.A);
            gb.Press(Joypad.B);
            // lock (WeedleStates) { WeedleStates[state] = gb.SaveState(); }
        }
        // else
        // {
        //     // gb.LoadState(WeedleStates[state]);
        // }
        return true;
    }

    static List<int> CheckWeedle(int p, string forest, int igtf1, int igtf2, int igts, bool print, int atk, int def, int hp, int maxhp, int maxdmg=6, bool antidote=true, List<byte[]> successes = null, int numThreads=6)
    {
        int numigt = FrameCount(igtf1, igtf2);
        const int weedleframes = 3;
        List<int> successfulFrames = new List<int>();
        
        RedCb[] gbs = MultiThread.MakeThreads<RedCb>(numThreads);
        // RedCb gb = new RedCb();
        MultiThread.For(numigt, gbs, (gb, it) =>
        // for (int it = 0; it < numigt; ++it)
        {
            int igtf = (igtf1 + it) % 60;
            if (IgnoredFramesP2.Contains(igtf)) return;
            int weedleSuccess = 0;
            if (!LoadForestState(gb, hp, maxhp, p, igts, igtf))
                return;
            int adr;
            if (antidote)
            {
                adr = gb.Execute(SpacePath(forest), (gb.Maps[51][25, 12], gb.PickupItem));
            }
            else
            {
                adr = gb.Execute(SpacePath(forest));
            }
            bool enc = false;
            if (adr == gb.WildEncounterAddress) {
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
                    string log = "";
                    gb.LoadState(weedlestate);
                    gb.AdvanceFrames(weedleframe);
                    gb.Inject(Joypad.B);
                    gb.AdvanceFrame(Joypad.B);
                    gb.ClearText(Joypad.B);
                    int lasthp = hp;
                    bool crit = false;
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
                    if (gb.EnemyMon.HP == 0 && !gb.BattleMon.Poisoned && gb.BattleMon.HP >= hp - maxdmg && !crit) weedleSuccess++;
                }
            }
            if (weedleSuccess == weedleframes)
            {
                lock (successfulFrames) successfulFrames.Add(igtf);
            }
        });
        SortFrames(successfulFrames);
        foreach (var gb in gbs) gb.Dispose();
        return successfulFrames;
    }

     static (int score, string npc, Dictionary<int, List<string>> frameLogs, int encs, int neighbourSuccess) EvalWeedle(int p, string forest, int igtf1, int igtf2, int igts, int atk, int def, int hp, int maxhp, bool antidote=true, List<byte[]> successes = null, int numThreads = 8)
    {
        // check neighbouring frames
        int frameWindow = FrameCount(igtf1, igtf2);
        int f1 = igtf1, f2 = igtf2;
        if (f1 == 0) f1 = 59;
        else f1--;
        if (f2 == 59) f2 = 0;
        else f2++;

        int numigt = FrameCount(f1, f2);
        const int weedleframes = 3;
        int score = 0;
        // int score9 = 0;
        int neighbourSuccess = 0;

        SearchCommon.Path path = new SearchCommon.Path(forest);
        int APresses = path.A;
        score -= APresses;
        int turns = path.T;
        score += (20 - turns) * 2;
        string ssMiss = "STRING SHOT Miss";
        string ps = "POISON STING";
        int encs = 0;

        Dictionary<int, List<string>> frameLogs = new Dictionary<int, List<string>>();
        RedCb[] gbs = MultiThread.MakeThreads<RedCb>(numThreads);
        // RedCb gb = new RedCb();
        SortedSet<string> goodnpcs = new SortedSet<string>();
        MultiThread.For(numigt, gbs, (gb, it) =>
        // for (int it = 0; it < numigt; ++it)
        {
            int igtf = (f1 + it) % 60;
            string encLog = "";
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
				encLog = gb.EnemyMon.ToString();
				if (!frameLogs.ContainsKey(igtf)) frameLogs[igtf] = new List<string>();
				frameLogs[igtf].Add(encLog);
                enc = true;
                Increment(ref encs);
            }
            if (!enc)
            {
                if (igtf != f1 && igtf != f2) score+=3;
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
                    gb.AdvanceFrames(weedleframe);
                    gb.Inject(Joypad.B);
                    gb.AdvanceFrame(Joypad.B);
                    gb.ClearText(Joypad.B);
                    int lasthp = hp;
                    bool crit = false;
                    gb.Press(Joypad.A, Joypad.Down, Joypad.A); // t1
                    turn = LogTurn(gb);
                    log += turn;
					if (igtf != f1 && igtf != f2) {
						if (turn.Contains(ssMiss)) Increment(ref score);
						else if (turn.Contains(ps)) Decrement(ref score);
					}
                    gb.ClearText(Joypad.A);
                    if (gb.BattleMon.HP <= lasthp - 5) crit = true;
                    lasthp = gb.BattleMon.HP;
                    gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t2
                    turn = LogTurn(gb);
                    log += turn;
					if (igtf != f1 && igtf != f2) {
						if (turn.Contains(ssMiss)) Increment(ref score);
						else if (turn.Contains(ps)) Decrement(ref score);
					}
                    gb.ClearText(Joypad.A);
                    if (gb.BattleMon.HP <= lasthp - 5) crit = true;
                    lasthp = gb.BattleMon.HP;
                    gb.Press(Joypad.A, Joypad.Up, Joypad.A); // t3
                    turn = LogTurn(gb);
                    log += turn;
					if (igtf != f1 && igtf != f2) {
						if (turn.Contains(ssMiss)) Increment(ref score);
						else if (turn.Contains(ps)) Decrement(ref score);
					}
                    gb.ClearText(Joypad.A);
                    if (gb.BattleMon.HP <= lasthp - 5) crit = true;
                    lasthp = gb.BattleMon.HP;
                    gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t4
                    turn = LogTurn(gb);
                    log += turn;
					if (igtf != f1 && igtf != f2) {
						if (turn.Contains(ssMiss)) Increment(ref score);
						else if (turn.Contains(ps)) Decrement(ref score);
					}
                    gb.ClearText(Joypad.A);
                    if (gb.BattleMon.HP <= lasthp - 5) crit = true;
                    lasthp = gb.BattleMon.HP;
                    gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t5
                    turn = LogTurn(gb);
                    log += turn;
					if (igtf != f1 && igtf != f2) {
						if (turn.Contains(ssMiss)) Increment(ref score);
						else if (turn.Contains(ps)) Decrement(ref score);
					}
                    gb.ClearText(Joypad.A);
                    if (gb.BattleMon.HP <= lasthp - 5) crit = true;
                    gb.Press(Joypad.A, Joypad.Select, Joypad.A); // t6
                    turn = LogTurn(gb);
                    log += turn;
					if (igtf != f1 && igtf != f2) {
						if (turn.Contains(ssMiss)) Increment(ref score);
						else if (turn.Contains(ps)) Decrement(ref score);
					}
                    if (gb.EnemyMon.HP == 0 && !gb.BattleMon.Poisoned && gb.BattleMon.HP >= hp - 6 && !crit && igtf != f1 && igtf != f2)
                    {
                        if (gb.BattleMon.HP == hp) Add(ref score, 3);
                        else if (gb.BattleMon.HP == hp - 3) Add(ref score, 2);
                        else Increment(ref score);
                        // if (igtf > ((igtf < tf1) ? tf1-60 : tf1) && igtf < ((igtf > tf2) ? tf2+60 : tf2)) Increment(ref score9);
                        lock (goodnpcs) { goodnpcs.Add(npcs); }
                        if (successes != null) successes.Add(gb.SaveState());
                    }
                    else if (gb.EnemyMon.HP == 0 && !gb.BattleMon.Poisoned && gb.BattleMon.HP >= hp - 6 && !crit && igtf == f1 && igtf != f2)
                    {
                        Increment(ref neighbourSuccess);
                    }
					lock (frameLogs)
					{
						if (!frameLogs.ContainsKey(igtf)) frameLogs[igtf] = new List<string>();
						frameLogs[igtf].Add(log);
					}
                }
            }
            else
            {
                if (igtf == f1 || igtf == f2)
                {
                    // Trace.WriteLine("f" + igtf + " " + encLog);
                    if (frameWindow == 3) score -= 5;
                    else if (frameWindow == 4) score -= 2;
                }
            }
        });
        string npc = "";
        foreach (string s in goodnpcs) if (npc == "") npc = s; else npc += " or " + s;
        if (successes != null && successes.Count < 9) score = -9999;
        foreach (var gb in gbs) gb.Dispose();
        return (score, npc, frameLogs, encs, neighbourSuccess);
    }

    static int EvalWeedle2(int p, string forest, string postFight, int igtf1, int igtf2, int igts, RedCb gb, int atk, int def, int hp, int maxhp, bool antidote = true, List<byte[]> successes = null)
    {
        int numigt = FrameCount(igtf1, igtf2);
        const int weedleframes = 3;
        int success = 0;

        Dictionary<int, List<string>> frameLogs = new Dictionary<int, List<string>>();
        // RedCb gb = new RedCb();
        SortedSet<string> goodnpcs = new SortedSet<string>();
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
                        }
                    }
                }
            }
        }
        return success;
    }

    public static List<(int hp, int maxhp, int atk, int def)> ReadWeedleStats()
    {
        var paths = new List<(int hp, int maxhp, int atk, int def)>();
        foreach (string line in File.ReadAllLines("weedle/wpaths_new.txt"))
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
            var r2 = Regex.Match(line, "([LRUDA]+)");
            if (r1.Success) currStats = (int.Parse(r1.Groups[3].Value), int.Parse(r1.Groups[4].Value), int.Parse(r1.Groups[1].Value), int.Parse(r1.Groups[2].Value));
            else
            {
                if (!paths.ContainsKey(currStats)) paths[currStats] = new List<string>();
                paths[currStats].Add(r2.Groups[1].Value);
            }
        }
        return paths;
    }
    
    void CheckWeedlePaths(int pidgeypath, int igtf1, int igtf2, string inputFile, string outputFile, bool antidote, string unfinished="", int maxdmg=6)
    {
        var paths = ParseLogs(inputFile);
        var listener = new TextWriterTraceListener(File.CreateText(outputFile));
        Trace.Listeners.Add(listener);
        Trace.WriteLine("Stats with Paths: " + paths.Count);
        List<(int hp, int maxhp, int atk, int def)> paths2 = new List<(int hp, int maxhp, int atk, int def)>();
        if (File.Exists(unfinished))
        {
            // Dictionary<(int hp, int maxhp, int atk, int def), List<string>> frameRes = new Dictionary<(int hp, int maxhp, int atk, int def), List<string>>();
            foreach (string line in File.ReadLines(unfinished)) Trace.WriteLine(line);
            paths2 = ParseLogs2(unfinished);
        }
        foreach (var stat in paths)
        {
            int atk = stat.Key.atk, def = stat.Key.def, hp = stat.Key.hp, maxhp = stat.Key.maxhp;
            // if (hp >= 15) continue;
            if (paths2.Contains(stat.Key)) continue;
            Trace.WriteLine(atk + " " + def + " " + hp + "/" + maxhp);
            foreach (var path in stat.Value)
            {
                Trace.WriteLine(path);
                var successfulFrames = CheckWeedle(pidgeypath, path, igtf1, igtf2, 30, false, atk, def, hp, maxhp, maxdmg, antidote);
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
        paths2.Clear();
    }

    static List<(int hp, int maxhp, int atk, int def)> ParseLogs2(string unfinished)
    {
        var stats = new List<(int hp, int maxhp, int atk, int def)>();
        foreach (string line in System.IO.File.ReadAllLines(unfinished))
        {
            var r1 = Regex.Match(line, @"([0-9]+) ([0-9]+) ([0-9]+)/([0-9]+)");
            if (r1.Success) stats.Add((int.Parse(r1.Groups[3].Value), int.Parse(r1.Groups[4].Value), int.Parse(r1.Groups[1].Value), int.Parse(r1.Groups[2].Value)));
        }
        return stats;
    }

    static Dictionary<(int hp, int maxhp, int atk, int def), Dictionary<string, List<List<int>>>> ParseSuccessfulFrames(string file, int f1, int f2)
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
                IEnumerable<int> numbers;
                if (f1 > f2) numbers = r3.Value.Split(',').Select(int.Parse).Where(f => f % 60 >= f1 || f % 60 <= f2);
                else numbers = r3.Value.Split(',').Select(int.Parse).Where(f => f % 60 >= f1 && f % 60 <= f2);
                var contiguousFrames = GroupContiguousIGT(numbers, true);
                paths[currStats][currPath].AddRange(contiguousFrames);
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

        // accept 3-5 windows
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

    static void FindBestWeedlePaths(string inputFile, string outputFile, int pidgeypath, bool antidote, int tf1, int tf2)
    {
        string link = "https://gunnermaniac.com/pokeworld?local=51#21/59/";
        var paths = ParseSuccessfulFrames(inputFile, tf1, tf2);
        var finalPaths = new Dictionary<string, PathInfo>();
        var rejectedPaths = new Dictionary<string, List<PathInfo>>();
        int igtSecDeduction = 3;

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
            string bestPostFight = "";
            int bestEncCount = 2;
            foreach (var path in stat.Value)
            {
                var clusters = LargestFrameClusters(path.Value);
                if (clusters.Count == 1)
                {
                    int igtf1 = clusters[0][0], igtf2 = clusters[0][clusters[0].Count - 1];
                    int frameWindow = FrameCount(igtf1, igtf2);
                    if (currLargestWindow >= frameWindow && bestIgtSecs.Count <= 10 && bestEncCount < 2 && bestScore >= 10 && bestPostFight != "") continue;
                    currLargestWindow = Math.Max(currLargestWindow, frameWindow);
                    List<byte[]> states = new List<byte[]>();
                    var res = EvalWeedle(pidgeypath, path.Key, igtf1, igtf2, 30, atk, def, hp, maxhp, antidote, successes: states);
                    var sortedFrameLogs = SortFrameLogs(res.frameLogs);
                    int successes = states.Count;
                    SortedSet<int> igtSecs;
                    if (successes == frameWindow * 3)
                    {
                        string postFight = ExtWeedleSearch.SearchPostWeedle(8, 42, states);
                        igtSecs = CheckWeedleIgtSecond(pidgeypath, path.Key, postFight, igtf1, igtf2, atk, def, hp, maxhp, successes, antidote);
                        List<List<int>> contiguousSeconds = GroupContiguousIGT(igtSecs, false);
                        int score = res.score;
                        foreach (var group in contiguousSeconds) score -= group.Count * igtSecDeduction;
                        // score += res.s9 * 3;
                        string keyString = "(" + hp + "," + maxhp + "," + atk + "," + def + ")";
                        if (score > bestScore)
                        {
                            if (bestScore != -9999)
                            {
                                if (!rejectedPaths.ContainsKey(keyString)) rejectedPaths[keyString] = new List<PathInfo>();
                                rejectedPaths[keyString].Add(createPathInfo(pidgeypath, bestPath, bestPostFight, bestScore, link, bestNpcs, bestFrames, bestIgtSecs, bestFrameLogs, antidote));
                                using var stream = File.Create(outputFile + "_rejected.json");
                                JsonSerializer.Serialize(stream, rejectedPaths, new JsonSerializerOptions { WriteIndented = true });
                            }
                            bestPath = path.Key;
                            bestScore = score;
                            bestFrameLogs = sortedFrameLogs;
                            bestNpcs = res.npc;
                            bestFrames = clusters[0];
                            bestStates = states;
                            bestIgtSecs = igtSecs;
                            bestPostFight = postFight;
                            bestEncCount = res.encs;
                        }
                        else
                        {
                            if (!rejectedPaths.ContainsKey(keyString)) rejectedPaths[keyString] = new List<PathInfo>();
                            rejectedPaths[keyString].Add(createPathInfo(pidgeypath, path.Key, postFight, score, link, res.npc, clusters[0], igtSecs, res.frameLogs, antidote));
                            using var stream = File.Create(outputFile + "_rejected.json");
                            JsonSerializer.Serialize(stream, rejectedPaths, new JsonSerializerOptions { WriteIndented = true });
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
                    string bestClusterPostFight = "";
                    int bestClusterEncCount = 2;
                    string keyString = "(" + hp + "," + maxhp + "," + atk + "," + def + ")";
                    foreach (var cluster in clusters)
                    {
                        int igtf1 = cluster[0], igtf2 = cluster[cluster.Count - 1];
                        int frameWindow = FrameCount(igtf1, igtf2);
                        if (currLargestWindow >= frameWindow && bestIgtSecs.Count <= 10 && bestEncCount < 2 && bestScore >= 10 && bestPostFight != "") continue;
                        currLargestWindow = Math.Max(currLargestWindow, frameWindow);
                        List<byte[]> states = new List<byte[]>();
                        var res = EvalWeedle(pidgeypath, path.Key, igtf1, igtf2, 30, atk, def, hp, maxhp, successes: states);
                        var sortedFrameLogs = SortFrameLogs(res.frameLogs);
                        int successes = states.Count;
                        SortedSet<int> igtSecs;
                        if (successes == frameWindow * 3)
                        {
                            string postFight = ExtWeedleSearch.SearchPostWeedle(8, 42, states);
                            igtSecs = CheckWeedleIgtSecond(pidgeypath, path.Key, postFight, igtf1, igtf2, atk, def, hp, maxhp, successes, antidote);
                            List<List<int>> contiguousSeconds = GroupContiguousIGT(igtSecs, false);
                            int score = res.score;
                            foreach (var group in contiguousSeconds) score -= group.Count * igtSecDeduction;
                            // score += res.s9 * 3;
                            // int score = res.score;
                            if (score > bestClusterScore)
                            {
                                bestClusterScore = score;
                                bestCluster = cluster;
                                bestClusterFrameLogs = sortedFrameLogs;
                                bestClusterNpcs = res.npc;
                                bestClusterStates = states;
                                bestClusterIgtSecs = igtSecs;
                                bestClusterPostFight = postFight;
                                bestClusterEncCount = res.encs;
                            }
                        }
                    }
                    if (bestClusterScore > bestScore)
                    {
                        if (bestScore != -9999)
                        {
                            if (!rejectedPaths.ContainsKey(keyString)) rejectedPaths[keyString] = new List<PathInfo>();
                            rejectedPaths[keyString].Add(createPathInfo(pidgeypath, bestPath, bestPostFight, bestScore, link, bestNpcs, bestFrames, bestIgtSecs, bestFrameLogs, antidote));
                            using var stream = File.Create(outputFile + "_rejected.json");
                            JsonSerializer.Serialize(stream, rejectedPaths, new JsonSerializerOptions { WriteIndented = true });
                        }
                        bestPath = path.Key;
                        bestScore = bestClusterScore;
                        bestFrameLogs = bestClusterFrameLogs;
                        bestNpcs = bestClusterNpcs;
                        bestFrames = bestCluster;
                        bestStates = bestClusterStates;
                        bestIgtSecs = bestClusterIgtSecs;
                        bestPostFight = bestClusterPostFight;
                        bestEncCount = bestClusterEncCount;
                    }
                    else
                    {
                        if (!rejectedPaths.ContainsKey(keyString)) rejectedPaths[keyString] = new List<PathInfo>();
                        rejectedPaths[keyString].Add(createPathInfo(pidgeypath, path.Key, bestClusterPostFight, bestClusterScore, link, bestClusterNpcs, bestCluster, bestClusterIgtSecs, bestClusterFrameLogs, antidote));
                        using var stream = File.Create(outputFile + "_rejected.json");
                        JsonSerializer.Serialize(stream, rejectedPaths, new JsonSerializerOptions { WriteIndented = true });
                    }
                }
            }

            if (bestPath != "" && bestStates.Count >= 9)
            {
                // string postFight = ExtWeedleSearch.SearchPostWeedle(8, 42, bestStates);
                List<string> separatedPaths = SeparateForestPath(bestPath);
                string keyString = "(" + hp + "," + maxhp + "," + atk + "," + def + ")";
                if (!finalPaths.ContainsKey(keyString))
                {
                    finalPaths[keyString] = createPathInfo(pidgeypath, bestPath, bestPostFight, bestScore, link, bestNpcs, bestFrames, bestIgtSecs, bestFrameLogs, antidote);
                    using var stream = File.Create(outputFile);
                    JsonSerializer.Serialize(stream, finalPaths, new JsonSerializerOptions { WriteIndented = true });
                }
            }
        }
        finalPaths.Clear();
        rejectedPaths.Clear();
        paths.Clear();
    }

    static void RecheckWeedleIgtSecond(string inputFile, string outputFile, bool antidote, int numThreads=6)
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
            int numigt = 60;
            RedCb[] gbs = MultiThread.MakeThreads<RedCb>(numThreads);
            MultiThread.For(numigt, gbs, (gb, s) =>
            {
                int success = EvalWeedle2(2, path, postFight, pathInfo.frames[0], pathInfo.frames[pathInfo.frames.Count - 1], s, gb, atk, def, hp, maxhp, antidote);
                int successes = pathInfo.frames.Count * 3;
                Console.WriteLine("IGT Second: " + s);
                Console.WriteLine("Original success: " + successes);
                Console.WriteLine("New success: " + success);
                Console.WriteLine(" ");
                if (success < successes)
                    lock (set) { set.Add(s); }
            });
            foreach (var gb in gbs) gb.Dispose();
            int score = pathData[stat.Key].score;
            var oldIgtSecs = pathData[stat.Key].igtSecs;
            // Console.WriteLine(set.ToList().Count - oldIgtSecs.ToList().Count);
            pathData[stat.Key].score = score - ((set.ToList().Count - oldIgtSecs.ToList().Count) * 2);
            pathData[stat.Key].igtSecs = set.ToList();
        }
        using var stream = File.Create(outputFile);
        JsonSerializer.Serialize(stream, pathData, new JsonSerializerOptions { WriteIndented = true });
        pathData.Clear();
    }

    static SortedSet<int> CheckWeedleIgtSecond(int p, string path, string postFight, int igtf1, int igtf2, int atk, int def, int hp, int maxhp, int successes, bool antidote, int numThreads=6)
    {
        SortedSet<int> set = new SortedSet<int>();
        int numigt = 60;
        RedCb[] gbs = MultiThread.MakeThreads<RedCb>(numThreads);
        MultiThread.For(numigt, gbs, (gb, s) =>
        {
            int success = EvalWeedle2(p, path, postFight, igtf1, igtf2, s, gb, atk, def, hp, maxhp, antidote);
            if (success < successes)
                lock (set) { set.Add(s); }
        });
        foreach (var gb in gbs) gb.Dispose();
        return set;
    }

    void ComparePathsAndMerge(string file1, string file2, string outputFile)
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

    void MergePaths(string file1, string file2, string outputFile)
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
            if (!mergedPaths.ContainsKey(key)) mergedPaths[key] = path2;
        }

        using var stream = File.Create(outputFile);
        JsonSerializer.Serialize(stream, mergedPaths, new JsonSerializerOptions { WriteIndented = true });
    }

    // ignore
    void FilterGoodPaths(string inputFile, string outputFile)
    {
        string json = File.ReadAllText(inputFile);
        var pathData = JsonSerializer.Deserialize<Dictionary<string, PathInfo>>(json);
        var res = new Dictionary<string, PathInfo>();
        foreach (var stat in pathData)
        {
            PathInfo pathInfo = stat.Value;
            int enc = 0;
            foreach (var log in pathInfo.frameLogs)
            {
                foreach (var turn in log.Value) if (turn.Contains("DVs")) enc++;
            }
            if (pathInfo.igtSecs.Count <= 6 && (enc < 2 || pathInfo.frames.Count > 3) && pathInfo.score > 0) res[stat.Key] = pathInfo;
        }
        using var stream = File.Create(outputFile);
        JsonSerializer.Serialize(stream, res, new JsonSerializerOptions { WriteIndented = true });
    }

    void ReEvaluateScores(string inputFile, string outputFile, int p, bool antidote)
    {
        string json = File.ReadAllText(inputFile);
        var pathData = JsonSerializer.Deserialize<Dictionary<string, PathInfo>>(json);
        int igtSecDeduction = 3;
        foreach (var stat in pathData)
        {
            var pathInfo = stat.Value;
            int pidgeypath;
            if (pathInfo.path == null) pidgeypath = p;
            else pidgeypath = int.Parse(pathInfo.path.Substring(0, 1));
            string path = pathInfo.route2 + pathInfo.gate + pathInfo.forest;
            int igtf1 = pathInfo.frames[0], igtf2 = pathInfo.frames[pathInfo.frames.Count - 1];
            var values = stat.Key.Trim('(', ')').Split(',');
            int hp = int.Parse(values[0]);
            int maxhp = int.Parse(values[1]);
            int atk = int.Parse(values[2]);
            int def = int.Parse(values[3]);
            List<byte[]> states = new List<byte[]>();
            var res = EvalWeedle(pidgeypath, path, igtf1, igtf2, 30, atk, def, hp, maxhp, antidote, successes: states);
            List<List<int>> contiguousSeconds = GroupContiguousIGT(pathInfo.igtSecs, false);
            int score = res.score;
            foreach (var group in contiguousSeconds) score -= group.Count * igtSecDeduction;
            var sortedFrameLogs = SortFrameLogs(res.frameLogs);
            // score += res.s9 * 3;

            pathData[stat.Key].score = score;
            pathData[stat.Key].frameLogs = sortedFrameLogs;
            if (pathInfo.path == null) pathData[stat.Key].path = p + (antidote ? "a" : "b");
        }
        using var stream = File.Create(outputFile);
        JsonSerializer.Serialize(stream, pathData, new JsonSerializerOptions { WriteIndented = true });
    }

    void FilterPathsByFrames(string inputFile, string outputFile, int f1, int f2)
    {
        string json = File.ReadAllText(inputFile);
        var pathData = JsonSerializer.Deserialize<Dictionary<string, PathInfo>>(json);
        var newPathData = new Dictionary<string, PathInfo>();
        foreach (var stat in pathData)
        {
            var pathInfo = stat.Value;
            var required = new List<int>();
            for (int i = 0; i < FrameCount(f1, f2); i++)
            {
                required.Add((f1 + i) % 60);
            }
            var sortedFrameLogs = SortFrameLogs(pathInfo.frameLogs);
            if (required.All(f => pathInfo.frames.Contains(f)) && pathInfo.igtSecs.Count < 20)
            {
                newPathData[stat.Key] = pathInfo;
                newPathData[stat.Key].frameLogs = sortedFrameLogs;
            }
        }
        using var stream = File.Create(outputFile);
        JsonSerializer.Serialize(stream, newPathData, new JsonSerializerOptions { WriteIndented = true });
    }
    
    void TestPidgeyPaths(string file, int f1, int f2, int numThreads=6)
    {
        string link = "https://gunnermaniac.com/pokeworld?map=1#33/181/DRRUUURRRRRRRRRRRRRRRRRRRRRURUUUUUUR";
        List<string> paths = new List<string>();
        foreach (string line in File.ReadAllLines(file))
        {
            var r1 = Regex.Match(line, "([LRUDA]+)");
            if (r1.Success) paths.Add(r1.Groups[1].Value);
        }

        List<(int hp, int maxhp)> hplist = new List<(int hp, int maxhp)>();
        for (int hp = 10; hp < 24; hp++)
            for (int maxhp = 21; maxhp < 24; maxhp++)
            {
                if (hp > maxhp) continue;
                hplist.Add((hp, maxhp));
            }
        int hpCount = hplist.Count;
        foreach (var path in paths)
        {
            int numFrames = FrameCount(f1, f2);
            int pathSuccess = 0;
            RedCb[] gbs = MultiThread.MakeThreads<RedCb>(numThreads);
            MultiThread.For(59, gbs, (gb, s) =>
            {
                for (int i = 0; i < hpCount; i++)
                {
                    for (int f = 0; f < numFrames; f++)
                    {
                        int igtf = (f1 + f) % 60;
                        if (LoadForestState(gb, hplist[i].hp, hplist[i].maxhp, 1, BasePathToGirl + path, s, igtf)) pathSuccess++;
                    }
                }
            });
            Trace.WriteLine(link + path + " success: " + pathSuccess + "/" + (hpCount * numFrames * 59));
        }
    }

    public ExtendedWeedle()
    {
        // CheckWeedlePaths(57, 1, "weedle/p2f0_antiskip/log.txt", "weedle/p2f0_antiskip/p2f0g1_2_frames.txt", false);

        // CheckWeedlePaths(2, 57, 1, "weedle/p2f0/p2f0g1/p2f0g1.txt", "weedle/p2f0/p2f0g1/p2f0g1_f57-1.txt", true);
        // CheckWeedlePaths(2, 57, 1, "weedle/p2f0/p2f0g3/p2f0g3.txt", "weedle/p2f0/p2f0g3/p2f0g3_f57-1.txt", true);
        // CheckWeedlePaths(2, 57, 1, "weedle/p2f0/p2f0g24a/p2f0g24a.txt", "weedle/p2f0/p2f0g24a/p2f0g24a_f57-1.txt", true);

        // CheckWeedlePaths(2, 3, 8, "weedle/p2bf5/p2bf5g2a4.txt", "weedle/p2bf5/p2bf5g2a4_f3-8.txt", false);

        // ReEvaluateScores("weedle/p2f6_noanti/p2f6_reval.json", "weedle/p2f6_noanti/p2f6_reval.json", 2, false);

        // FindBestWeedlePaths("weedle/p2bf5/p2bf5g2a4_f3-8.txt", "weedle/p2bf5/p2bf5g2a4.json", 2, false, 4, 7);

        ComparePathsAndMerge("p2b_f57-1_f3-8.json", "weedle/p2f6_noanti/p2f6_reval.json", "p2b_merged_forruns.json");


        // CheckWeedlePaths(4, 56, 8, "weedle/p4f0test/p4f0test.txt", "weedle/p4f0test/p4f0test_f56-8.txt", true);
        // CheckWeedlePaths(5, 56, 8, "weedle/p5f0test/p5f0test.txt", "weedle/p5f0test/p5f0test_f56-8.txt", true);
        // CheckWeedlePaths(6, 56, 8, "weedle/p6f0test/p6f0test.txt", "weedle/p6f0test/p6f0test_f56-8.txt", false);
        // CheckWeedlePaths(2, 57, 8, "weedle/p2f0/p2f0_paths.txt", "weedle/p2f0/p2f0_f57-8.txt", true);

        // CheckWeedlePaths(2, 56, 8, "weedle/p2altf0/p2altf0g3.txt", "weedle/p2altf0/p2altf0g3_frames.txt", true);

        // TestPidgeyPaths("p1.txt", 56, 2, 6);

        // Check missing stats
        // var stats = ExtendedWeedle.ReadWeedleStats();
        // string json = File.ReadAllText("p2a_f58-0_f4-7.json");
        // var pathData = JsonSerializer.Deserialize<Dictionary<string, PathInfo>>(json);
        // List<(int hp, int maxhp, int atk, int def)> seenStats = new List<(int hp, int maxhp, int atk, int def)>();
        // foreach (var stat in pathData)
        // {
        //     var values = stat.Key.Trim('(', ')').Split(',');
        //     int hp = int.Parse(values[0]);
        //     int maxhp = int.Parse(values[1]);
        //     int atk = int.Parse(values[2]);
        //     int def = int.Parse(values[3]);
        //     var statKey = (hp, maxhp, atk, def);
        //     seenStats.Add(statKey);
        // }

        // foreach(var stat in stats)
        // {
        //     if (!seenStats.Contains(stat)) Trace.WriteLine(stat);
        // }


        // var paths = ParseSuccessfulFrames("weedle/p3af0/p3f0g4_frames.txt", 57, 1);
        // foreach (var stat in paths)
        // {
        //     Trace.WriteLine(stat.Key);
        //     int atk = stat.Key.atk, def = stat.Key.def, hp = stat.Key.hp, maxhp = stat.Key.maxhp;
        //     foreach (var path in stat.Value)
        //     {
        //         var clusters = LargestFrameClusters(path.Value);
        //         int igtf1 = clusters[0][0], igtf2 = clusters[0][clusters[0].Count - 1];
        //         var res = EvalWeedle(3, path.Key, igtf1, igtf2, 30, atk, def, hp, maxhp, true);
        //         var sortedFrameLogs = SortFrameLogs(res.frameLogs);
        //         Trace.WriteLine("F58 success: " + res.neighbourSuccess);
        //         foreach (var frameLog in sortedFrameLogs)
        //         {
        //             Trace.WriteLine(frameLog.Key);
        //             foreach (var frame in frameLog.Value) Trace.WriteLine(frame);
        //         }
        //     }
        //     Trace.WriteLine(" ");
        // }

        // FilterPathsByFrames("p2_anti_reval.json", "p2_anti_new.json", 58, 0);

        // FindBestWeedlePaths("p2_frames_combined.txt", "p2a_f58-0_f4-7.json", 2, true, 3, 7);

        // CheckWeedlePaths(57, 1, "weedle/p2f0_antiskip/p2f0g1.txt", "weedle/p2f0_antiskip/p2f0g1_frames.txt", false);
        // FindBestWeedlePaths("weedle/p2f0_antiskip/p2f0g1_frames.txt", "weedle/p2f0_antiskip/p2f0g1.json", 2, false, 57, 1);

        // ReEvaluateScores("weedle/p2f6_noanti/p2f6.json", "weedle/p2f6_noanti/p2f6_reval.json", false);
        // FilterPathsByFrames("weedle/p2f6_noanti/p2f6_reval.json", "weedle/p2f6_noanti/p2f6_filtered.json", 4, 6);

        // string link = "https://gunnermaniac.com/pokeworld?local=51#21/59/";
        // List<byte[]> states = new List<byte[]>();
        // string forest = "UUAUULLLLLUUUUAUURUAUUUUURRRRUURRRRUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUULLLLLLLLDDDDDDDLLLLUUUUUUUUUUUUULLLLLLDDDDDDDDDDDDDLDDDDDDLLLLUUU";
        // foreach (var path in SeparateForestPath(forest)) Console.WriteLine(path);
        // var res = EvalWeedle(2, forest, 58, 0, 30, 12, 13, 19, 22, states);
        // // Console.WriteLine("score: " + res.score + " npc: " + res.npc);
        // string postFight = ExtWeedleSearch.SearchPostWeedle(8, 4, states);
        // Console.WriteLine(link + forest + postFight);

        // CheckWeedlePaths();
        // var pathFrames = ParseSuccessfulFrames("p2_frames_combined.txt", 57, 7);
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

        // FilterGoodPaths("weedle/p2f2_anti/p2f2_oopsie.json", "weedle/p2f2_anti/p2f2_filtered.json");

        // RecheckWeedleIgtSecond("weedle/p2f2_anti/p2f2_filtered.json", "weedle/p2f2_anti/p2f2_filtered_recheck.json", true);

        // StartWatch();
        // string json = File.ReadAllText("test.json");
        // var pathData = JsonSerializer.Deserialize<Dictionary<string, PathInfo>>(json);
        // foreach (var stat in pathData)
        // {
        //     var pathInfo = stat.Value;
        //     int pidgeypath = int.Parse(pathInfo.path.Substring(0, 1));
        //     string path = pathInfo.route2 + pathInfo.gate + pathInfo.forest;
        //     int igtf1 = pathInfo.frames[0], igtf2 = pathInfo.frames[pathInfo.frames.Count - 1];
        //     var values = stat.Key.Trim('(', ')').Split(',');
        //     int hp = int.Parse(values[0]);
        //     int maxhp = int.Parse(values[1]);
        //     int atk = int.Parse(values[2]);
        //     int def = int.Parse(values[3]);
        //     List<byte[]> states = new List<byte[]>();
        //     var igtSecs1 = CheckWeedleIgtSecond(2, path, pathInfo.postFight, igtf1, igtf2, atk, def, hp, maxhp, pathInfo.frames.Count * 3, true);
        //     SortedSet<int> igtSecs2 = new SortedSet<int>();
        //     // var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 2 };
        //     // for (int s=0; s<60; s++)
        //     int numigt = 60;
        //     RedCb[] gbs = MultiThread.MakeThreads<RedCb>(6);
        //     MultiThread.For(numigt, gbs, (gb, s) =>
        //     {
        //         int success = EvalWeedle2(2, path, pathInfo.postFight, igtf1, igtf2, s, gb, atk, def, hp, maxhp, true);
        //         int successes = pathInfo.frames.Count * 3;
        //         Console.WriteLine("IGT Second: " + s);
        //         Console.WriteLine("Original success: " + successes);
        //         Console.WriteLine("New success: " + success);
        //         Console.WriteLine(" ");
        //         if (success < successes)
        //             lock (igtSecs2) { igtSecs2.Add(s); }
        //     }
        //     );
        //     foreach (var gb in gbs) gb.Dispose();

        //     Console.WriteLine("igts1");
        //     foreach (var sec in igtSecs1)
        //     {
        //         Trace.WriteLine(sec);
        //     }
        //     Console.WriteLine("igts2");
        //     foreach (var sec in igtSecs2)
        //     {
        //         Trace.WriteLine(sec);
        //     }
        // }
        // Elapsed("test3");
        // 39.838
        // 66.325
    }
}