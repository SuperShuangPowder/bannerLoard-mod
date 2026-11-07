using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace WXProvincialFreeCompany
{
    /// <summary>
    /// Read-only diagnostic commands, registered through the official
    /// CommandLineFunctionality extension point (no Harmony).
    ///
    ///   wx_pfc.profile [SettlementId]
    ///   wx_pfc.sample [SettlementId] [Count]
    ///   wx_pfc.audit_volunteers
    ///   wx_pfc.audit_parties
    ///
    /// None of these commands mutate game state; in particular they never
    /// touch volunteer slots, parties, or the campaign RNG.
    ///
    /// All console output is ASCII-only: the in-game debug console font does
    /// not contain CJK glyphs, so Chinese text renders as question marks.
    /// Detailed diagnostics go to the log via Debug.Print.
    /// </summary>
    public static class RegionalRecruitmentDebugCommands
    {
        private const ulong DefaultDebugFilter = 17592186044416UL;
        private const int FixedSampleSeed = 20261107;
        private const int MaxSampleCount = 1000000;

        private static readonly HashSet<string> ImperialNobleTroopIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "imperial_vigla_recruit",
            "imperial_equite",
            "imperial_heavy_horseman",
            "imperial_cataphract",
            "imperial_elite_cataphract",
        };

        [CommandLineFunctionality.CommandLineArgumentFunction("profile", "wx_pfc")]
        public static string Profile(List<string> strings) => Safe(ProfileImpl, strings);

        [CommandLineFunctionality.CommandLineArgumentFunction("sample", "wx_pfc")]
        public static string Sample(List<string> strings) => Safe(SampleImpl, strings);

        [CommandLineFunctionality.CommandLineArgumentFunction("audit_volunteers", "wx_pfc")]
        public static string AuditVolunteers(List<string> strings) => Safe(AuditVolunteersImpl, strings);

        [CommandLineFunctionality.CommandLineArgumentFunction("audit_parties", "wx_pfc")]
        public static string AuditParties(List<string> strings) => Safe(AuditPartiesImpl, strings);

        private static string Safe(Func<List<string>, string> action, List<string> strings)
        {
            try
            {
                return action(strings);
            }
            catch (Exception exception)
            {
                string message = $"wx_pfc error: {exception.GetType().Name}: {exception.Message}";
                Log(message);
                return message;
            }
        }

        private static string ProfileImpl(List<string> strings)
        {
            if (strings == null || strings.Count == 0)
            {
                return "wx_pfc.profile <SettlementId>";
            }

            RegionalRecruitmentRegistry.EnsureLoaded();
            string? loadFailure = RegionalRecruitmentRegistry.LoadFailureReason;
            if (loadFailure != null)
            {
                return $"wx_pfc: config not loaded, vanilla behavior: {loadFailure}";
            }

            string settlementId = strings[0];
            RegionalRecruitmentRegistry registry = RegionalRecruitmentRegistry.Instance;
            RegionalRecruitmentProfile? profile = registry.GetProfile(settlementId);
            if (profile == null)
            {
                return $"{settlementId}: no regional config (vanilla behavior)";
            }

            string troop = profile.IsCoreRegion
                ? "none (core region)"
                : (registry.GetTroopId(profile.Region) ?? "mapping missing");

            RegionalRecruitmentSettings.EnsureLoaded();
            RegionalRecruitmentSettings settings = RegionalRecruitmentSettings.Instance;
            double effectiveChance = settings.GetChance(profile.Tier);
            int maxSlots = settings.GetMaxSlots(profile.Tier);

            string message =
                $"{settlementId}: {profile.Region}/{profile.Tier}, " +
                $"designChance={profile.AuxiliaryChance.ToString("0.00", CultureInfo.InvariantCulture)}, " +
                $"effectiveChance={effectiveChance.ToString("0.00", CultureInfo.InvariantCulture)}, " +
                $"maxSlots={maxSlots}, troop={troop}";
            Log(message);
            return message;
        }

        private static string SampleImpl(List<string> strings)
        {
            if (strings == null || strings.Count < 2)
            {
                return "wx_pfc.sample <SettlementId> <Count>";
            }

            RegionalRecruitmentRegistry.EnsureLoaded();
            string? loadFailure = RegionalRecruitmentRegistry.LoadFailureReason;
            if (loadFailure != null)
            {
                return $"wx_pfc: config not loaded, vanilla behavior: {loadFailure}";
            }

            string settlementId = strings[0];
            if (!int.TryParse(strings[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int count))
            {
                return $"count not parseable: '{strings[1]}'";
            }

            if (count < 1 || count > MaxSampleCount)
            {
                return $"count must be within 1..{MaxSampleCount}";
            }

            RegionalRecruitmentRegistry registry = RegionalRecruitmentRegistry.Instance;
            RegionalRecruitmentProfile? profile = registry.GetProfile(settlementId);
            if (profile == null)
            {
                return $"{settlementId}: no regional config (vanilla behavior)";
            }

            RegionalRecruitmentSettings.EnsureLoaded();
            RegionalRecruitmentSettings settings = RegionalRecruitmentSettings.Instance;
            double effectiveChance = settings.GetChance(profile.Tier);
            int maxSlots = settings.GetMaxSlots(profile.Tier);
            RegionalRecruitmentSimulation.Result result =
                RegionalRecruitmentSimulation.Simulate(effectiveChance, count, FixedSampleSeed);

            string message =
                $"{settlementId}: designChance={profile.AuxiliaryChance.ToString("0.00", CultureInfo.InvariantCulture)}, " +
                $"effectiveChance={effectiveChance.ToString("0.00", CultureInfo.InvariantCulture)}, maxSlots={maxSlots}, " +
                $"n={result.Count}, seed={result.Seed}, hits={result.Hits}, " +
                $"observed={result.ObservedRatio.ToString("0.0000", CultureInfo.InvariantCulture)}, " +
                $"deviation={result.DeviationPoints.ToString("+0.00;-0.00", CultureInfo.InvariantCulture)}pp";
            Log(message);
            return message;
        }

        private static string AuditVolunteersImpl(List<string> strings)
        {
            if (Campaign.Current == null)
            {
                return "wx_pfc: no active campaign, cannot audit volunteer slots";
            }

            int eligibleNotables = 0;
            int notablesWithVolunteers = 0;
            var regionalById = new Dictionary<string, int>();
            var regionalByTier = new Dictionary<int, int>();
            int imperialRegular = 0;
            int imperialNoble = 0;
            int other = 0;

            VolunteerModel volunteerModel = Campaign.Current.Models.VolunteerModel;
            foreach (Hero hero in Hero.AllAliveHeroes)
            {
                if (hero == null || !hero.IsNotable || !volunteerModel.CanHaveRecruits(hero))
                {
                    continue;
                }

                eligibleNotables++;
                CharacterObject[]? volunteers = hero.VolunteerTypes;
                if (volunteers == null || volunteers.Length == 0)
                {
                    continue;
                }

                bool hasVolunteer = false;
                foreach (CharacterObject volunteer in volunteers)
                {
                    if (volunteer == null)
                    {
                        continue;
                    }

                    hasVolunteer = true;
                    string category = Classify(volunteer.StringId);
                    if (category == "regional")
                    {
                        regionalById.TryGetValue(volunteer.StringId, out int existing);
                        regionalById[volunteer.StringId] = existing + 1;

                        int tier = volunteer.Tier;
                        regionalByTier.TryGetValue(tier, out int tierCount);
                        regionalByTier[tier] = tierCount + 1;
                    }
                    else if (category == "imperial-noble")
                    {
                        imperialNoble++;
                    }
                    else if (category == "imperial")
                    {
                        imperialRegular++;
                    }
                    else
                    {
                        other++;
                    }
                }

                if (hasVolunteer)
                {
                    notablesWithVolunteers++;
                }
            }

            int regionalTotal = 0;
            foreach (int value in regionalById.Values)
            {
                regionalTotal += value;
            }

            var summary = new StringBuilder();
            summary.Append("Volunteer slots audit: ");
            summary.Append($"eligibleNotables={eligibleNotables}, notablesWithVolunteers={notablesWithVolunteers}, regional={regionalTotal}, ");
            summary.Append($"imperialRegular={imperialRegular}, imperialNoble={imperialNoble}, other={other}.");
            foreach (var pair in regionalById)
            {
                summary.Append($" {pair.Key}={pair.Value},");
            }

            if (regionalByTier.Count > 0)
            {
                summary.Append(" tier:");
                foreach (var pair in regionalByTier)
                {
                    summary.Append($"T{pair.Key}={pair.Value},");
                }
            }

            string message = summary.ToString().TrimEnd(',');
            Log(message);
            return message;
        }

        private static string AuditPartiesImpl(List<string> strings)
        {
            if (Campaign.Current == null)
            {
                return "wx_pfc: no active campaign, cannot audit parties";
            }

            int partyCount = 0;
            int troopTotal = 0;
            var regionalById = new Dictionary<string, int>();
            int imperialRegular = 0;
            int imperialNoble = 0;
            int other = 0;
            int partiesWithRegional = 0;

            foreach (MobileParty party in MobileParty.All)
            {
                if (party == null || party.MemberRoster == null)
                {
                    continue;
                }

                partyCount++;
                bool partyHasRegional = false;
                foreach (FlattenedTroopRosterElement element in party.MemberRoster.ToFlattenedRoster())
                {
                    CharacterObject character = element.Troop;
                    if (character == null)
                    {
                        continue;
                    }

                    troopTotal++;
                    string category = Classify(character.StringId);
                    if (category == "regional")
                    {
                        regionalById.TryGetValue(character.StringId, out int existing);
                        regionalById[character.StringId] = existing + 1;
                        partyHasRegional = true;
                    }
                    else if (category == "imperial-noble")
                    {
                        imperialNoble++;
                    }
                    else if (category == "imperial")
                    {
                        imperialRegular++;
                    }
                    else
                    {
                        other++;
                    }
                }

                if (partyHasRegional)
                {
                    partiesWithRegional++;
                }
            }

            int regionalTotal = 0;
            foreach (int value in regionalById.Values)
            {
                regionalTotal += value;
            }

            var summary = new StringBuilder();
            summary.Append("Party audit: ");
            summary.Append($"parties={partyCount}, troops={troopTotal}, regional={regionalTotal}, ");
            summary.Append($"imperialRegular={imperialRegular}, imperialNoble={imperialNoble}, other={other}, ");
            summary.Append($"partiesWithRegional={partiesWithRegional}.");
            foreach (var pair in regionalById)
            {
                summary.Append($" {pair.Key}={pair.Value},");
            }

            string message = summary.ToString().TrimEnd(',');
            Log(message);
            return message;
        }

        private static string Classify(string stringId)
        {
            if (string.IsNullOrEmpty(stringId))
            {
                return "other";
            }

            if (stringId.StartsWith("wx_", StringComparison.Ordinal))
            {
                return "regional";
            }

            if (ImperialNobleTroopIds.Contains(stringId))
            {
                return "imperial-noble";
            }

            if (stringId.StartsWith("imperial_", StringComparison.Ordinal))
            {
                return "imperial";
            }

            return "other";
        }

        private static void Log(string message)
        {
            Debug.Print(message, 0, Debug.DebugColor.White, DefaultDebugFilter);
        }
    }
}
