using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace WXProvincialFreeCompany
{
    /// <summary>
    /// v0.5.2 five-point prototype module.
    ///
    /// Loads and validates ModuleData/regional_recruitment.xml, wraps the
    /// current VolunteerModel with RegionalVolunteerModel (five prototype
    /// settlements only), and exposes the wx_pfc.* diagnostic commands.
    /// All other volunteer behavior stays vanilla.
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
            WrapVolunteerModel(starterObject);
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

        /// <summary>
        /// Wraps the currently effective VolunteerModel with the regional
        /// decorator (five-point prototype). Reentrant: each campaign start
        /// receives a fresh CampaignGameStarter, and a double wrap is guarded.
        /// </summary>
        private static void WrapVolunteerModel(object starterObject)
        {
            try
            {
                if (!(starterObject is CampaignGameStarter starter))
                {
                    Log($"[{RegionalRecruitmentRegistry.ModuleId}] OnCampaignStart starterObject is " +
                        $"{(starterObject == null ? "null" : starterObject.GetType().FullName)}; " +
                        "VolunteerModel wrapper skipped.");
                    return;
                }

                VolunteerModel current = starter.GetModel<VolunteerModel>();
                if (current is RegionalVolunteerModel)
                {
                    Log($"[{RegionalRecruitmentRegistry.ModuleId}] VolunteerModel already wrapped; reentrancy guard.");
                    return;
                }

                var wrapper = new RegionalVolunteerModel(current);
                starter.AddModel(wrapper);
                Log($"[{RegionalRecruitmentRegistry.ModuleId}] VolunteerModel wrapped: inner={current.GetType().FullName}; " +
                    $"prototype points={FivePointPrototype.EnabledSettlementIds.Length} " +
                    $"({string.Join(", ", FivePointPrototype.EnabledSettlementIds)}).");
            }
            catch (Exception exception)
            {
                Log($"[{RegionalRecruitmentRegistry.ModuleId}] VolunteerModel wrap failed: {exception.Message}; " +
                    "vanilla recruitment preserved.");
            }
        }

        private static void Log(string message)
        {
            Debug.Print(message, 0, Debug.DebugColor.White, DefaultDebugFilter);
        }
    }
}
