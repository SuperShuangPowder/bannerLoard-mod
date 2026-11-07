using System;
using System.IO;
using System.Linq;
using System.Reflection;
using WXProvincialFreeCompany;

/// <summary>
/// Pure-logic smoke test for the v0.5.1 read-only recruitment layer.
///
/// Scope: config parsing, deterministic simulation and debug-command
/// attribute registration. Runs on any machine with .NET SDK 6.0.428 after
/// a Release build; the attribute check additionally resolves
/// TaleWorlds.Library from BANNERLORD_GAME_DIR when that variable is set
/// (never hardcoded):
///
///   dotnet run --project tests/SmokeTest -c Release
///
/// Game-dependent surfaces (Hero volunteer slots, MobileParty rosters,
/// console invocation) are verified manually in-game per BUILDING.md.
/// </summary>
internal static class Smoke
{
    private static int failures;

    private static void Main()
    {
        // Attribute metadata instantiation needs TaleWorlds.Library; resolve it
        // from BANNERLORD_GAME_DIR when present (never hardcoded).
        string gameDir = Environment.GetEnvironmentVariable("BANNERLORD_GAME_DIR");
        if (!string.IsNullOrEmpty(gameDir))
        {
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                var name = new AssemblyName(e.Name).Name;
                string path = Path.Combine(gameDir, @"bin\Win64_Shipping_Client", name + ".dll");
                return File.Exists(path) ? Assembly.LoadFrom(path) : null;
            };
        }

        string repo = FindRepoRoot();
        var modAsm = Assembly.LoadFrom(Path.Combine(repo,
            @"artifacts\build\Release\WXProvincialFreeCompany.dll"));

        Check("DLL 加载", true);

        // 1. Diagnostic command registration attributes
        var commands = modAsm.GetType("WXProvincialFreeCompany.RegionalRecruitmentDebugCommands");
        var expected = new[] { "Profile", "Sample", "AuditVolunteers", "AuditParties" };
        foreach (var name in expected)
        {
            var method = commands.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            try
            {
                bool hasAttr = method != null && CustomAttributeData.GetCustomAttributes(method)
                    .Any(d => d.AttributeType.FullName.Contains("CommandLineArgumentFunction"));
                Check($"命令特性注册 wx_pfc.{name}", hasAttr);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("  SKIP  命令特性注册（TaleWorlds.Library 不可解析；设置 BANNERLORD_GAME_DIR 可启用该检查）");
            }
        }

        // 2. Parse the real generated config
        string xmlPath = Path.Combine(repo, @"src\WXProvincialFreeCompany\ModuleData\regional_recruitment.xml");
        var registry = RegionalRecruitmentRegistry.Parse(File.ReadAllText(xmlPath));
        Check("征兵点总数 116", registry.ProfileCount == 116);
        Check("North=18", registry.CountByRegion(RegionalRegion.North) == 18);
        Check("South=22", registry.CountByRegion(RegionalRegion.South) == 22);
        Check("West=20", registry.CountByRegion(RegionalRegion.West) == 20);
        Check("East=28", registry.CountByRegion(RegionalRegion.East) == 28);
        Check("Core=28", registry.CountByRegion(RegionalRegion.Core) == 28);
        Check("town_EN6 安普雷拉=East/Frontier/0.80",
            IsProfile(registry, "town_EN6", RegionalRegion.East, RegionalTier.Frontier, 0.80));
        Check("town_ES1 达努斯提卡=South/Frontier/0.80",
            IsProfile(registry, "town_ES1", RegionalRegion.South, RegionalTier.Frontier, 0.80));
        Check("town_EN3 核心=0",
            IsProfile(registry, "town_EN3", RegionalRegion.Core, RegionalTier.Core, 0.0));
        Check("无配置聚落返回 null", registry.GetProfile("retirement_retreat") == null);

        // 3. Simulation tolerance (frozen: 10k samples, 0 strict, others ±2pp)
        Check("sim 0.00 一万次严格 0 命中",
            RegionalRecruitmentSimulation.Simulate(0.00, 10000, 20261107).Hits == 0);
        foreach (var chance in new[] { 0.30, 0.55, 0.80 })
        {
            var result = RegionalRecruitmentSimulation.Simulate(chance, 10000, 20261107);
            Check($"sim {chance:0.00} 一万次偏差≤2pp (实际 {result.DeviationPoints:+0.000;-0.000}pp)",
                Math.Abs(result.DeviationPoints) <= 2.0);
        }

        // 4. Invalid configs are rejected
        ExpectThrow("空 RegionMapping 被拒",
            () => RegionalRecruitmentRegistry.Parse("<RegionalRecruitment><RegionMapping/></RegionalRecruitment>"));
        ExpectThrow("重复聚落 ID 被拒",
            () => RegionalRecruitmentRegistry.Parse(ValidHeader +
                "<Settlements><Settlement id=\"town_EN1\" region=\"North\" tier=\"Border\" chance=\"0.55\"/>" +
                "<Settlement id=\"town_EN1\" region=\"North\" tier=\"Border\" chance=\"0.55\"/></Settlements></RegionalRecruitment>"));
        ExpectThrow("核心区非 0 概率被拒",
            () => RegionalRecruitmentRegistry.Parse(ValidHeader +
                "<Settlements><Settlement id=\"town_EN3\" region=\"Core\" tier=\"Core\" chance=\"0.55\"/></Settlements></RegionalRecruitment>"));
        ExpectThrow("非法概率 0.42 被拒",
            () => RegionalRecruitmentRegistry.Parse(ValidHeader +
                "<Settlements><Settlement id=\"town_EN1\" region=\"North\" tier=\"Border\" chance=\"0.42\"/></Settlements></RegionalRecruitment>"));

        Console.WriteLine();
        Console.WriteLine(failures == 0 ? "ALL CHECKS PASSED" : $"{failures} CHECK(S) FAILED");
        Environment.Exit(failures == 0 ? 0 : 1);
    }

    private const string ValidHeader =
        "<RegionalRecruitment><RegionMapping>" +
        "<Region region=\"North\" troop=\"NPCCharacter.wx_northern_borderlander\"/>" +
        "<Region region=\"South\" troop=\"NPCCharacter.wx_southern_borderlander\"/>" +
        "<Region region=\"West\" troop=\"NPCCharacter.wx_western_borderlander\"/>" +
        "<Region region=\"East\" troop=\"NPCCharacter.wx_eastern_borderlander\"/>" +
        "</RegionMapping>";

    private static string FindRepoRoot()
    {
        // tests/SmokeTest/bin/.../SmokeTest.dll -> walk up to the repo root.
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "global.json")))
        {
            dir = Path.GetDirectoryName(dir);
        }

        if (dir == null)
        {
            throw new InvalidOperationException("Cannot locate repo root (global.json not found).");
        }

        return dir;
    }

    private static bool IsProfile(
        RegionalRecruitmentRegistry registry,
        string id,
        RegionalRegion region,
        RegionalTier tier,
        double chance)
    {
        var profile = registry.GetProfile(id);
        return profile != null && profile.Region == region && profile.Tier == tier
            && Math.Abs(profile.AuxiliaryChance - chance) < 0.0001;
    }

    private static void Check(string name, bool ok)
    {
        Console.WriteLine((ok ? "  PASS  " : "  FAIL  ") + name);
        if (!ok)
        {
            failures++;
        }
    }

    private static void ExpectThrow(string name, Action action)
    {
        try
        {
            action();
            Check(name, false);
        }
        catch
        {
            Check(name, true);
        }
    }
}
