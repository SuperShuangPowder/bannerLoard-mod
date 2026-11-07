using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using WXProvincialFreeCompany;

/// <summary>
/// Pure-logic smoke test for the v0.5.6 regional recruitment layer.
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

        string subModuleText = File.ReadAllText(Path.Combine(repo,
            @"src\WXProvincialFreeCompany\SubModule.xml"));
        Check("module: version v0.5.6", subModuleText.Contains("<Version value=\"v0.5.6\""));
        Check("module: optional Sandbox ordering dependency",
            subModuleText.Contains("Id=\"Sandbox\" DependentVersion=\"v1.4.8\" Optional=\"true\""));
        Check("module: optional StoryMode ordering dependency",
            subModuleText.Contains("Id=\"StoryMode\" DependentVersion=\"v1.4.8\" Optional=\"true\""));
        Check("module: campaign GameText registered",
            subModuleText.Contains("<XmlName id=\"GameText\" path=\"wx_campaign_strings\""));

        // NumericCampaignOptionData looks up labels and descriptions via
        // GameTexts.FindText(baseId, variation). Language files can translate
        // an existing GameText, but cannot create that variation by themselves.
        string gameTextPath = Path.Combine(repo,
            @"src\WXProvincialFreeCompany\ModuleData\wx_campaign_strings.xml");
        var gameTextDocument = new XmlDocument();
        gameTextDocument.Load(gameTextPath);
        var gameTextNodes = gameTextDocument.SelectNodes("/strings/string");
        Check("campaign options: exactly 12 GameText variations", gameTextNodes.Count == 12);

        var localizationKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (XmlNode node in gameTextNodes)
        {
            string id = node.Attributes["id"]?.Value;
            string text = node.Attributes["text"]?.Value;
            Check($"campaign options: valid localized GameText {id}",
                !string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(text) && text.StartsWith("{="));
            int keyEnd = text == null ? -1 : text.IndexOf('}');
            if (keyEnd > 2)
            {
                localizationKeys.Add(text.Substring(2, keyEnd - 2));
            }
        }

        string[] optionVariations =
        {
            "str_wx_pfc_chance_frontier", "str_wx_pfc_chance_border", "str_wx_pfc_chance_transition",
            "str_wx_pfc_slots_frontier", "str_wx_pfc_slots_border", "str_wx_pfc_slots_transition",
        };
        var gameTextIds = new HashSet<string>(
            gameTextNodes.Cast<XmlNode>().Select(n => n.Attributes["id"]?.Value),
            StringComparer.Ordinal);
        foreach (string variation in optionVariations)
        {
            Check($"campaign options: type variation {variation}",
                gameTextIds.Contains("str_campaign_options_type." + variation));
            Check($"campaign options: description variation {variation}",
                gameTextIds.Contains("str_campaign_options_description." + variation));
        }

        foreach (string languagePath in new[]
        {
            @"src\WXProvincialFreeCompany\ModuleData\Languages\en\wx_provincial_free_company_xml-eng.xml",
            @"src\WXProvincialFreeCompany\ModuleData\Languages\CNs\wx_provincial_free_company_xml-zho-CN.xml",
        })
        {
            var languageDocument = new XmlDocument();
            languageDocument.Load(Path.Combine(repo, languagePath));
            var languageIds = new HashSet<string>(
                languageDocument.SelectNodes("/base/strings/string").Cast<XmlNode>()
                    .Select(n => n.Attributes["id"]?.Value),
                StringComparer.Ordinal);
            Check($"campaign options: all localization keys in {Path.GetDirectoryName(languagePath)}",
                localizationKeys.All(languageIds.Contains));
        }

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

        // The audit classifier must recognize the complete imperial noble line,
        // not only the T1 vigla recruit.
        MethodInfo classify = commands.GetMethod("Classify", BindingFlags.NonPublic | BindingFlags.Static);
        foreach (string nobleId in new[]
        {
            "imperial_vigla_recruit",
            "imperial_equite",
            "imperial_heavy_horseman",
            "imperial_cataphract",
            "imperial_elite_cataphract",
        })
        {
            string category = (string)classify.Invoke(null, new object[] { nobleId });
            Check($"audit: noble classification {nobleId}", category == "imperial-noble");
        }
        Check("audit: regular imperial troop stays regular",
            (string)classify.Invoke(null, new object[] { "imperial_infantryman" }) == "imperial");

        // 2. Parse the real generated config
        string xmlPath = Path.Combine(repo, @"src\WXProvincialFreeCompany\ModuleData\regional_recruitment.xml");
        var registry = RegionalRecruitmentRegistry.Parse(File.ReadAllText(xmlPath));
        Check("征兵点总数 116", registry.ProfileCount == 116);
        Check("North=18", registry.CountByRegion(RegionalRegion.North) == 18);
        Check("South=22", registry.CountByRegion(RegionalRegion.South) == 22);
        Check("West=20", registry.CountByRegion(RegionalRegion.West) == 20);
        Check("East=28", registry.CountByRegion(RegionalRegion.East) == 28);
        Check("Core=28", registry.CountByRegion(RegionalRegion.Core) == 28);
        Check("town_EN6 安普雷拉=East/Frontier/0.50",
            IsProfile(registry, "town_EN6", RegionalRegion.East, RegionalTier.Frontier, 0.50));
        Check("town_ES1 达努斯提卡=South/Frontier/0.50",
            IsProfile(registry, "town_ES1", RegionalRegion.South, RegionalTier.Frontier, 0.50));
        Check("town_EN4 阿耳戈隆=North/Transition/0.20",
            IsProfile(registry, "town_EN4", RegionalRegion.North, RegionalTier.Transition, 0.20));
        Check("town_ES2 沃斯特鲁姆=South/Border/0.30",
            IsProfile(registry, "town_ES2", RegionalRegion.South, RegionalTier.Border, 0.30));
        Check("town_EN3 核心=0",
            IsProfile(registry, "town_EN3", RegionalRegion.Core, RegionalTier.Core, 0.0));
        Check("无配置聚落返回 null", registry.GetProfile("retirement_retreat") == null);

        // Diagnostic commands must report and simulate the effective player
        // setting, while retaining the static design value for comparison.
        RegionalRecruitmentRegistry.Load(File.ReadAllText(xmlPath));
        RegionalRecruitmentSettings.EnsureLoaded();
        double effectiveFrontierChance = RegionalRecruitmentSettings.Instance.GetChance(RegionalTier.Frontier);
        string effectiveFrontierText = effectiveFrontierChance.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        string profileOutput = RegionalRecruitmentDebugCommands.Profile(new List<string> { "town_EN6" });
        Check("diagnostic: profile reports design and effective chance",
            profileOutput.Contains("designChance=0.50")
            && profileOutput.Contains("effectiveChance=" + effectiveFrontierText)
            && profileOutput.Contains("maxSlots=" + RegionalRecruitmentSettings.Instance.FrontierMaxSlots));
        string sampleOutput = RegionalRecruitmentDebugCommands.Sample(new List<string> { "town_EN6", "100" });
        Check("diagnostic: sample uses effective chance",
            sampleOutput.Contains("designChance=0.50")
            && sampleOutput.Contains("effectiveChance=" + effectiveFrontierText));

        // 3. Simulation tolerance (frozen: 10k samples, 0 strict, others ±2pp)
        Check("sim 0.00 一万次严格 0 命中",
            RegionalRecruitmentSimulation.Simulate(0.00, 10000, 20261107).Hits == 0);
        foreach (var chance in new[] { 0.20, 0.30, 0.50 })
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
                "<Settlements><Settlement id=\"town_EN1\" region=\"North\" tier=\"Border\" chance=\"0.30\"/>" +
                "<Settlement id=\"town_EN1\" region=\"North\" tier=\"Border\" chance=\"0.30\"/></Settlements></RegionalRecruitment>"));
        ExpectThrow("核心区非 0 概率被拒",
            () => RegionalRecruitmentRegistry.Parse(ValidHeader +
                "<Settlements><Settlement id=\"town_EN3\" region=\"Core\" tier=\"Core\" chance=\"0.30\"/></Settlements></RegionalRecruitment>"));
        ExpectThrow("非法概率 0.42 被拒",
            () => RegionalRecruitmentRegistry.Parse(ValidHeader +
                "<Settlements><Settlement id=\"town_EN1\" region=\"North\" tier=\"Border\" chance=\"0.42\"/></Settlements></RegionalRecruitment>"));

        // 5. Pure replacement decision logic (probability + slot cap)
        const string northTroop = "NPCCharacter.wx_northern_borderlander";
        Check("decision: 贵族兵(imperial_vigla_recruit)永不替换",
            !RegionalVolunteerDecision.ShouldReplace("imperial_vigla_recruit", northTroop, 0.01, 0.50, 3, 0));
        Check("decision: 非帝国兵种永不替换",
            !RegionalVolunteerDecision.ShouldReplace("sturgian_recruit", northTroop, 0.01, 0.50, 3, 0));
        Check("decision: 空兵种映射不替换",
            !RegionalVolunteerDecision.ShouldReplace("imperial_recruit", "", 0.01, 0.50, 3, 0));
        Check("decision: 概率为 0 不替换",
            !RegionalVolunteerDecision.ShouldReplace("imperial_recruit", northTroop, 0.01, 0.0, 3, 0));
        Check("decision: 上限为 0 不替换",
            !RegionalVolunteerDecision.ShouldReplace("imperial_recruit", northTroop, 0.01, 0.50, 0, 0));
        Check("decision: 槽数已达上限不替换 (2>=2)",
            !RegionalVolunteerDecision.ShouldReplace("imperial_recruit", northTroop, 0.01, 0.50, 2, 2));
        Check("decision: 槽数超上限不替换 (3>=2)",
            !RegionalVolunteerDecision.ShouldReplace("imperial_recruit", northTroop, 0.01, 0.50, 2, 3));
        Check("decision: 命中且未达上限即替换 (0.49<0.50, 1<2)",
            RegionalVolunteerDecision.ShouldReplace("imperial_recruit", northTroop, 0.49, 0.50, 2, 1));
        Check("decision: 未命中保持原版 (0.51>=0.50)",
            !RegionalVolunteerDecision.ShouldReplace("imperial_recruit", northTroop, 0.51, 0.50, 2, 0));
        Check("decision: 边界值恰等于概率不替换 (0.50>=0.50)",
            !RegionalVolunteerDecision.ShouldReplace("imperial_recruit", northTroop, 0.50, 0.50, 2, 0));

        // 6. Player settings (defaults, parse round-trip, clamping)
        var settingsDefaults = RegionalRecruitmentSettings.Defaults();
        Check("settings: 默认概率 50/30/20 与上限 3/2/1",
            settingsDefaults.FrontierChancePct == 50 && settingsDefaults.BorderChancePct == 30
            && settingsDefaults.TransitionChancePct == 20 && settingsDefaults.CoreChancePct == 0
            && settingsDefaults.FrontierMaxSlots == 3 && settingsDefaults.BorderMaxSlots == 2
            && settingsDefaults.TransitionMaxSlots == 1 && settingsDefaults.CoreMaxSlots == 0);
        Check("settings: 按层级取值",
            settingsDefaults.GetChancePct(RegionalTier.Frontier) == 50
            && settingsDefaults.GetChancePct(RegionalTier.Transition) == 20
            && settingsDefaults.GetMaxSlots(RegionalTier.Border) == 2
            && Math.Abs(settingsDefaults.GetChance(RegionalTier.Border) - 0.30) < 0.0001);
        var modified = RegionalRecruitmentSettings.Defaults();
        modified.FrontierChancePct = 65;
        modified.BorderMaxSlots = 5;
        var roundTrip = RegionalRecruitmentSettings.Parse(modified.ToXml());
        Check("settings: XML 往返一致",
            roundTrip.FrontierChancePct == 65 && roundTrip.BorderMaxSlots == 5
            && roundTrip.TransitionChancePct == 20 && roundTrip.FrontierMaxSlots == 3);
        var clamped = RegionalRecruitmentSettings.Parse(
            "<WXProvincialFreeCompanySettings><ChanceFrontier>150</ChanceFrontier><MaxSlotsBorder>-3</MaxSlotsBorder></WXProvincialFreeCompanySettings>");
        Check("settings: 越界值被钳制 (150→100, -3→0)",
            clamped.FrontierChancePct == 100 && clamped.BorderMaxSlots == 0);
        var withFallback = RegionalRecruitmentSettings.Parse(
            "<WXProvincialFreeCompanySettings><ChanceFrontier>40</ChanceFrontier></WXProvincialFreeCompanySettings>");
        Check("settings: 缺失元素回退默认",
            withFallback.FrontierChancePct == 40 && withFallback.BorderChancePct == 30 && withFallback.BorderMaxSlots == 2);

        Check("全图启用: 全部 116 个征兵点均有档案且四地域根兵映射齐备",
            registry.ProfileCount == 116
            && registry.GetProfile("town_EN6") != null
            && registry.GetProfile("castle_village_EW1_1") != null
            && registry.GetProfile("village_ES7_2") != null
            && registry.GetTroopId(RegionalRegion.North) != null
            && registry.GetTroopId(RegionalRegion.South) != null
            && registry.GetTroopId(RegionalRegion.West) != null
            && registry.GetTroopId(RegionalRegion.East) != null);

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
