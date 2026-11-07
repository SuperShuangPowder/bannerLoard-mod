using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace WXProvincialFreeCompany
{
    /// <summary>
    /// v0.5.5 full-map regional recruitment module.
    ///
    /// Loads and validates ModuleData/regional_recruitment.xml, wraps the
    /// current VolunteerModel with RegionalVolunteerModel (all 116 configured
    /// recruit points), and exposes the wx_pfc.* diagnostic commands.
    /// Noble recruits and everything outside the config stay vanilla.
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

        /// <summary>
        /// Official model-registration hook (same one StoryMode uses to add
        /// its 18 replacement models): called by Campaign.OnInitialize with
        /// the IGameStarter that carries the CampaignGameStarter.
        /// </summary>
        protected override void InitializeGameStarter(Game game, IGameStarter gameStarter)
        {
            base.InitializeGameStarter(game, gameStarter);
            EnsureConfigLoadedAndLog();
            WrapVolunteerModel(gameStarter);
        }

        private static void EnsureConfigLoadedAndLog()
        {
            RegionalRecruitmentRegistry.EnsureLoaded();
            RegionalRecruitmentSettings.EnsureLoaded();
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
        /// decorator (all configured recruit points). Reentrant: a double wrap on the
        /// same starter is guarded; each campaign receives a fresh starter.
        /// </summary>
        private static void WrapVolunteerModel(IGameStarter gameStarter)
        {
            try
            {
                if (!(gameStarter is CampaignGameStarter starter))
                {
                    Log($"[{RegionalRecruitmentRegistry.ModuleId}] InitializeGameStarter received " +
                        $"{(gameStarter == null ? "null" : gameStarter.GetType().FullName)}; " +
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
                    "full-map enablement: all configured recruit points participate.");
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
