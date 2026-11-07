using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.ViewModelCollection;

namespace WXProvincialFreeCompany
{
    /// <summary>
    /// Official in-game campaign-options provider. Discovered automatically by
    /// CampaignOptionsManager, which scans all active game assemblies for
    /// implementations of ICampaignOptionProvider (verified via IL). No
    /// Harmony, no manual registration.
    ///
    /// Exposes six numeric sliders (three tiers x probability + slot cap).
    /// The Core tier is intentionally absent: it has no regional troop
    /// identity (core = imperial heartland) and its chance is frozen at 0.
    /// </summary>
    public sealed class WxPfcCampaignOptionsProvider : ICampaignOptionProvider
    {
        public IEnumerable<ICampaignOptionData> GetGameplayCampaignOptions()
        {
            RegionalRecruitmentSettings settings = RegionalRecruitmentSettings.Instance;

            yield return Chance("str_wx_pfc_chance_frontier", 0,
                () => settings.FrontierChancePct, v => SetChance(settings, t => t.FrontierChancePct = (int)v));
            yield return Chance("str_wx_pfc_chance_border", 1,
                () => settings.BorderChancePct, v => SetChance(settings, t => t.BorderChancePct = (int)v));
            yield return Chance("str_wx_pfc_chance_transition", 2,
                () => settings.TransitionChancePct, v => SetChance(settings, t => t.TransitionChancePct = (int)v));
            yield return Slots("str_wx_pfc_slots_frontier", 3,
                () => settings.FrontierMaxSlots, v => SetSlots(settings, t => t.FrontierMaxSlots = (int)v));
            yield return Slots("str_wx_pfc_slots_border", 4,
                () => settings.BorderMaxSlots, v => SetSlots(settings, t => t.BorderMaxSlots = (int)v));
            yield return Slots("str_wx_pfc_slots_transition", 5,
                () => settings.TransitionMaxSlots, v => SetSlots(settings, t => t.TransitionMaxSlots = (int)v));
        }

        public IEnumerable<ICampaignOptionData> GetCharacterCreationCampaignOptions()
        {
            yield break;
        }

        private static ICampaignOptionData Chance(
            string identifier, int priority, Func<float> getValue, Action<float> setValue)
        {
            return new NumericCampaignOptionData(
                identifier, priority, CampaignOptionEnableState.Enabled,
                getValue, setValue, 0f, 100f, true,
                null, false, null, null);
        }

        private static ICampaignOptionData Slots(
            string identifier, int priority, Func<float> getValue, Action<float> setValue)
        {
            return new NumericCampaignOptionData(
                identifier, priority, CampaignOptionEnableState.Enabled,
                getValue, setValue, 0f, 6f, true,
                null, false, null, null);
        }

        private static void SetChance(RegionalRecruitmentSettings settings, Action<RegionalRecruitmentSettings> set)
        {
            set(settings);
            RegionalRecruitmentSettings.Save();
        }

        private static void SetSlots(RegionalRecruitmentSettings settings, Action<RegionalRecruitmentSettings> set)
        {
            set(settings);
            RegionalRecruitmentSettings.Save();
        }
    }
}
