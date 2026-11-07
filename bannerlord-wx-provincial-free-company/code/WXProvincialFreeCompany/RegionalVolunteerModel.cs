using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace WXProvincialFreeCompany
{
    /// <summary>
    /// Decorator over the currently effective VolunteerModel (record 032 §5).
    ///
    /// Every member is delegated to the inner model except GetBasicVolunteer,
    /// which may substitute the regional T1 for the vanilla imperial_recruit
    /// on configured imperial recruit points. Any exception inside the
    /// substitution path falls back to the vanilla result: this wrapper must
    /// never break recruitment.
    /// </summary>
    public sealed class RegionalVolunteerModel : VolunteerModel
    {
        private const ulong DefaultDebugFilter = 17592186044416UL;

        private readonly VolunteerModel _inner;
        private Dictionary<RegionalRegion, CharacterObject>? _regionalTroops;
        private bool _replacementErrorLogged;

        public RegionalVolunteerModel(VolunteerModel inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public VolunteerModel InnerModel => _inner;

        private Dictionary<RegionalRegion, CharacterObject> RegionalTroops
        {
            get { return _regionalTroops ?? (_regionalTroops = ResolveRegionalTroops()); }
        }

        // ---- delegated members: everything else stays vanilla ----

        public override int MaximumIndexHeroCanRecruitFromHero(Hero buyerHero, Hero sellerHero, int useValueAsRelation)
        {
            return _inner.MaximumIndexHeroCanRecruitFromHero(buyerHero, sellerHero, useValueAsRelation);
        }

        public override int MaximumIndexGarrisonCanRecruitFromHero(Settlement settlement, Hero sellerHero)
        {
            return _inner.MaximumIndexGarrisonCanRecruitFromHero(settlement, sellerHero);
        }

        public override float GetDailyVolunteerProductionProbability(Hero hero, int index, Settlement settlement)
        {
            return _inner.GetDailyVolunteerProductionProbability(hero, index, settlement);
        }

        public override bool CanHaveRecruits(Hero hero)
        {
            return _inner.CanHaveRecruits(hero);
        }

        public override int MaxVolunteerTier => _inner.MaxVolunteerTier;

        // ---- the single replaced member ----

        public override CharacterObject? GetBasicVolunteer(Hero seller)
        {
            CharacterObject? original = _inner.GetBasicVolunteer(seller);
            if (original == null)
            {
                return null; // pass through whatever the inner model returned
            }

            try
            {
                return ReplaceIfEligible(original, seller);
            }
            catch (Exception exception)
            {
                if (!_replacementErrorLogged)
                {
                    _replacementErrorLogged = true;
                    Debug.Print(
                        $"[{RegionalRecruitmentRegistry.ModuleId}] regional replacement failed: {exception.Message}",
                        0, Debug.DebugColor.Red, DefaultDebugFilter);
                }

                return original;
            }
        }

        private CharacterObject? ReplaceIfEligible(CharacterObject original, Hero seller)
        {
            if (original == null || original.StringId != "imperial_recruit" || seller == null)
            {
                return original;
            }

            RegionalRecruitmentRegistry registry = RegionalRecruitmentRegistry.Instance;
            if (registry.ProfileCount == 0)
            {
                return original; // config not loaded: vanilla fallback
            }

            // Record 033 §4: the first version uses HomeSettlement only.
            Settlement? home = seller.HomeSettlement;
            if (home == null)
            {
                return original;
            }

            RegionalRecruitmentProfile? profile = registry.GetProfile(home.StringId);
            if (profile == null)
            {
                return original; // no config: vanilla fallback
            }

            string? troopId = registry.GetTroopId(profile.Region);
            if (troopId == null || troopId.Length == 0)
            {
                return original;
            }

            RegionalRecruitmentSettings settings = RegionalRecruitmentSettings.Instance;
            int regionalSlotsCount = CountRegionalSlots(seller);

            // The real campaign roll uses the game RNG (record 033 §5); the
            // fixed-seed simulation stays exclusive to the wx_pfc.sample command.
            // Chance and slot cap come from the player-adjustable settings
            // (v0.5.4); the XML chance remains the static design fallback.
            bool replace = RegionalVolunteerDecision.ShouldReplace(
                original.StringId,
                troopId,
                MBRandom.RandomFloat,
                settings.GetChance(profile.Tier),
                settings.GetMaxSlots(profile.Tier),
                regionalSlotsCount);
            if (!replace || !RegionalTroops.TryGetValue(profile.Region, out CharacterObject regional))
            {
                return original;
            }

            return regional;
        }

        private static int CountRegionalSlots(Hero seller)
        {
            CharacterObject[]? volunteers = seller.VolunteerTypes;
            if (volunteers == null)
            {
                return 0;
            }

            int count = 0;
            foreach (CharacterObject volunteer in volunteers)
            {
                if (volunteer != null && volunteer.StringId != null &&
                    volunteer.StringId.StartsWith("wx_", StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        private static Dictionary<RegionalRegion, CharacterObject> ResolveRegionalTroops()
        {
            var result = new Dictionary<RegionalRegion, CharacterObject>();
            RegionalRegion[] regions = { RegionalRegion.North, RegionalRegion.South, RegionalRegion.West, RegionalRegion.East };
            foreach (RegionalRegion region in regions)
            {
                string? troopId = RegionalRecruitmentRegistry.Instance.GetTroopId(region);
                if (troopId == null || troopId.Length == 0)
                {
                    continue;
                }

                string bareId = troopId.StartsWith("NPCCharacter.", StringComparison.Ordinal)
                    ? troopId.Substring("NPCCharacter.".Length)
                    : troopId;

                CharacterObject? troop = MBObjectManager.Instance?.GetObject<CharacterObject>(bareId);
                if (troop != null)
                {
                    result[region] = troop;
                }
            }

            return result;
        }
    }
}
