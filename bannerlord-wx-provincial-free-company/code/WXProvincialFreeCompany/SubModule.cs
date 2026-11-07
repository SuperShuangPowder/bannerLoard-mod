using System;
using System.IO;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace WXProvincialFreeCompany
{
    /// <summary>
    /// v0.5.1 read-only campaign module.
    ///
    /// Loads and validates ModuleData/regional_recruitment.xml at activation
    /// and exposes the wx_pfc.* diagnostic commands. This milestone does NOT
    /// wrap or replace VolunteerModel: volunteer results stay vanilla.
    /// </summary>
    public sealed class SubModule : MBSubModuleBase
    {
        private const ulong DefaultDebugFilter = 17592186044416UL;
        private const string ModuleId = "WXProvincialFreeCompany";

        public override void OnSubModuleActivated()
        {
            base.OnSubModuleActivated();
            LoadRecruitmentConfig();
        }

        public override void OnCampaignStart(Game game, object starterObject)
        {
            base.OnCampaignStart(game, starterObject);
            LogCurrentVolunteerModel(starterObject);
        }

        private static void LoadRecruitmentConfig()
        {
            try
            {
                string path = GetModuleDataPath();
                if (!File.Exists(path))
                {
                    RegionalRecruitmentRegistry.Fail($"配置文件不存在: {path}");
                    Log($"[{ModuleId}] regional_recruitment.xml missing: {path} — vanilla behavior preserved.");
                    return;
                }

                string xmlText = File.ReadAllText(path);
                RegionalRecruitmentRegistry.Load(xmlText);
                RegionalRecruitmentRegistry registry = RegionalRecruitmentRegistry.Instance;

                if (RegionalRecruitmentRegistry.LoadFailureReason != null)
                {
                    Log($"[{ModuleId}] regional_recruitment.xml rejected: " +
                        $"{RegionalRecruitmentRegistry.LoadFailureReason} — vanilla behavior preserved.");
                    return;
                }

                Log($"[{ModuleId}] regional_recruitment.xml loaded: {registry.ProfileCount} profiles " +
                    $"(North={registry.CountByRegion(RegionalRegion.North)}, " +
                    $"South={registry.CountByRegion(RegionalRegion.South)}, " +
                    $"West={registry.CountByRegion(RegionalRegion.West)}, " +
                    $"East={registry.CountByRegion(RegionalRegion.East)}, " +
                    $"Core={registry.CountByRegion(RegionalRegion.Core)}).");
            }
            catch (Exception exception)
            {
                RegionalRecruitmentRegistry.Fail(exception.Message);
                Log($"[{ModuleId}] regional_recruitment.xml load failed: {exception.Message} " +
                    "— vanilla behavior preserved.");
            }
        }

        private static void LogCurrentVolunteerModel(object starterObject)
        {
            try
            {
                if (starterObject is CampaignGameStarter starter)
                {
                    VolunteerModel model = starter.GetModel<VolunteerModel>();
                    Log($"[{ModuleId}] current VolunteerModel: {model.GetType().FullName} " +
                        "(read-only milestone: not wrapped, not replaced).");
                }
                else
                {
                    Log($"[{ModuleId}] OnCampaignStart starterObject is " +
                        $"{(starterObject == null ? "null" : starterObject.GetType().FullName)}; " +
                        "VolunteerModel inspection skipped.");
                }
            }
            catch (Exception exception)
            {
                Log($"[{ModuleId}] VolunteerModel inspection failed: {exception.Message}");
            }
        }

        private static string GetModuleDataPath()
        {
            // <game>/bin/Win64_Shipping_Client (or _wEditor) -> <game>/Modules/...
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string gameRoot = Path.GetFullPath(Path.Combine(baseDirectory, "..", ".."));
            return Path.Combine(gameRoot, "Modules", ModuleId, "ModuleData", "regional_recruitment.xml");
        }

        private static void Log(string message)
        {
            Debug.Print(message, 0, Debug.DebugColor.White, DefaultDebugFilter);
        }
    }
}
