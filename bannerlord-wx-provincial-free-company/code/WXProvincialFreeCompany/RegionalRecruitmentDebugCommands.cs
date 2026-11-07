using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using TaleWorlds.CampaignSystem;
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
    /// </summary>
    public static class RegionalRecruitmentDebugCommands
    {
        private const ulong DefaultDebugFilter = 17592186044416UL;
        private const int FixedSampleSeed = 20261107;
        private const int MaxSampleCount = 1000000;

        private static readonly FieldInfo VolunteerTypesField =
            typeof(Hero).GetField("VolunteerTypes", BindingFlags.NonPublic | BindingFlags.Instance);

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

            string? loadFailure = RegionalRecruitmentRegistry.LoadFailureReason;
            if (loadFailure != null)
            {
                return $"配置未加载，按原版处理：{loadFailure}";
            }

            string settlementId = strings[0];
            RegionalRecruitmentRegistry registry = RegionalRecruitmentRegistry.Instance;
            RegionalRecruitmentProfile? profile = registry.GetProfile(settlementId);
            if (profile == null)
            {
                return $"{settlementId}: 无地域配置（按原版处理）";
            }

            string troop = profile.IsCoreRegion
                ? "无（核心区）"
                : (registry.GetTroopId(profile.Region) ?? "缺失映射");

            string message =
                $"{settlementId}: {profile.Region}/{profile.Tier}, " +
                $"chance={profile.AuxiliaryChance.ToString("0.00", CultureInfo.InvariantCulture)}, troop={troop}";
            Log(message);
            return message;
        }

        private static string SampleImpl(List<string> strings)
        {
            if (strings == null || strings.Count < 2)
            {
                return "wx_pfc.sample <SettlementId> <Count>";
            }

            string? loadFailure = RegionalRecruitmentRegistry.LoadFailureReason;
            if (loadFailure != null)
            {
                return $"配置未加载，按原版处理：{loadFailure}";
            }

            string settlementId = strings[0];
            if (!int.TryParse(strings[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int count))
            {
                return $"计数无法解析: '{strings[1]}'";
            }

            if (count < 1 || count > MaxSampleCount)
            {
                return $"计数必须在 1..{MaxSampleCount} 之间";
            }

            RegionalRecruitmentRegistry registry = RegionalRecruitmentRegistry.Instance;
            RegionalRecruitmentProfile? profile = registry.GetProfile(settlementId);
            if (profile == null)
            {
                return $"{settlementId}: 无地域配置（按原版处理）";
            }

            RegionalRecruitmentSimulation.Result result =
                RegionalRecruitmentSimulation.Simulate(profile.AuxiliaryChance, count, FixedSampleSeed);

            string message =
                $"{settlementId}: chance={profile.AuxiliaryChance.ToString("0.00", CultureInfo.InvariantCulture)}, " +
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
                return "当前无战役，无法审计志愿兵槽";
            }

            var regionalById = new Dictionary<string, int>();
            var regionalByTier = new Dictionary<int, int>();
            int imperialRegular = 0;
            int imperialNoble = 0;
            int other = 0;
            int heroesWithSlots = 0;

            foreach (Hero hero in Hero.AllAliveHeroes)
            {
                if (hero == null)
                {
                    continue;
                }

                CharacterObject[]? volunteers = VolunteerTypesField?.GetValue(hero) as CharacterObject[];
                if (volunteers == null || volunteers.Length == 0)
                {
                    continue;
                }

                heroesWithSlots++;
                foreach (CharacterObject volunteer in volunteers)
                {
                    if (volunteer == null)
                    {
                        continue;
                    }

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
            }

            int regionalTotal = 0;
            foreach (int value in regionalById.Values)
            {
                regionalTotal += value;
            }

            var summary = new StringBuilder();
            summary.Append("志愿兵槽审计: ");
            summary.Append($"要人(有槽)={heroesWithSlots}, 地域兵={regionalTotal}, 帝国正规={imperialRegular}, ");
            summary.Append($"帝国贵族={imperialNoble}, 其他={other}。");
            if (regionalById.Count > 0)
            {
                summary.Append(" 地域兵按ID: ");
                foreach (var pair in regionalById)
                {
                    summary.Append($"{pair.Key}={pair.Value}, ");
                }

                summary.Append("按Tier: ");
                foreach (var pair in regionalByTier)
                {
                    summary.Append($"T{pair.Key}={pair.Value}, ");
                }
            }

            string message = summary.ToString().TrimEnd(' ', ',');
            Log(message);
            return message;
        }

        private static string AuditPartiesImpl(List<string> strings)
        {
            if (Campaign.Current == null)
            {
                return "当前无战役，无法审计部队";
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
            summary.Append("部队审计: ");
            summary.Append($"部队数={partyCount}, 总人数={troopTotal}, 地域兵={regionalTotal}, ");
            summary.Append($"帝国正规={imperialRegular}, 帝国贵族={imperialNoble}, 其他={other}, ");
            summary.Append($"含地域兵部队={partiesWithRegional}。");
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

            if (stringId.StartsWith("imperial_vigla", StringComparison.Ordinal))
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
