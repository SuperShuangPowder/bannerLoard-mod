using System;
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
    /// Loads and validates ModuleData/regional_recruitment.xml and exposes the
    /// wx_pfc.* diagnostic commands. This milestone does NOT wrap or replace
    /// VolunteerModel: volunteer results stay vanilla.
    /// </summary>
    public sealed class SubModule : MBSubModuleBase
    {
        private const ulong DefaultDebugFilter = 17592186044416UL;

        private static bool _configStateLogged;

        public override void OnSubModuleActivated()
        {
            base.OnSubModuleActivated();
            EnsureConfigLoadedAndLog();
        }

        public override void BeginGameStart(Game game)
        {
            base.BeginGameStart(game);
            EnsureConfigLoadedAndLog();
        }

        public override void OnCampaignStart(Game game, object starterObject)
        {
            base.OnCampaignStart(game, starterObject);
            EnsureConfigLoadedAndLog();
            LogCurrentVolunteerModel(starterObject);
        }

        private static void EnsureConfigLoadedAndLog()
        {
            RegionalRecruitmentRegistry.EnsureLoaded();
            if (_configStateLogged)
            {
                return;
            }

            _configStateLogged = true;
            RegionalRecruitmentRegistry registry = RegionalRecruitmentRegistry.Instance;
            if (RegionalRecruitmentRegistry.LoadFailureReason != null)
            {
                Log($"[{RegionalRecruitmentRegistry.ModuleId}] regional_recruitment.xml NOT loaded: " +
                    $"{RegionalRecruitmentRegistry.LoadFailureReason} — vanilla behavior preserved.");
                return;
            }

            Log($"[{RegionalRecruitmentRegistry.ModuleId}] regional_recruitment.xml loaded: " +
                $"{registry.ProfileCount} profiles " +
                $"(North={registry.CountByRegion(RegionalRegion.North)}, " +
                $"South={registry.CountByRegion(RegionalRegion.South)}, " +
                $"West={registry.CountByRegion(RegionalRegion.West)}, " +
                $"East={registry.CountByRegion(RegionalRegion.East)}, " +
                $"Core={registry.CountByRegion(RegionalRegion.Core)}).");
        }

        private static void LogCurrentVolunteerModel(object starterObject)
        {
            try
            {
                if (starterObject is CampaignGameStarter starter)
                {
                    VolunteerModel model = starter.GetModel<VolunteerModel>();
                    Log($"[{RegionalRecruitmentRegistry.ModuleId}] current VolunteerModel: {model.GetType().FullName} " +
                        "(read-only milestone: not wrapped, not replaced).");
                }
                else
                {
                    Log($"[{RegionalRecruitmentRegistry.ModuleId}] OnCampaignStart starterObject is " +
                        $"{(starterObject == null ? "null" : starterObject.GetType().FullName)}; " +
                        "VolunteerModel inspection skipped.");
                }
            }
            catch (Exception exception)
            {
                Log($"[{RegionalRecruitmentRegistry.ModuleId}] VolunteerModel inspection failed: {exception.Message}");
            }
        }

        private static void Log(string message)
        {
            Debug.Print(message, 0, Debug.DebugColor.White, DefaultDebugFilter);
        }
    }
}
