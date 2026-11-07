using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace WXProvincialFreeCompany
{
    /// <summary>
    /// Read-only registry of regional recruitment profiles.
    ///
    /// Parsed from ModuleData/regional_recruitment.xml. Any validation
    /// failure produces an empty registry plus a failure reason; the module
    /// then behaves exactly like vanilla because the volunteer wrapper sees an
    /// empty registry and preserves the inner model result.
    /// </summary>
    public sealed class RegionalRecruitmentRegistry
    {
        public const string ModuleId = "WXProvincialFreeCompany";

        private static readonly double[] AllowedChances = { 0.0, 0.20, 0.30, 0.50 };

        private readonly Dictionary<string, RegionalRecruitmentProfile> _profiles;
        private readonly Dictionary<RegionalRegion, string> _troopByRegion;

        private RegionalRecruitmentRegistry(
            Dictionary<string, RegionalRecruitmentProfile> profiles,
            Dictionary<RegionalRegion, string> troopByRegion)
        {
            _profiles = profiles;
            _troopByRegion = troopByRegion;
        }

        public static RegionalRecruitmentRegistry Instance { get; private set; } = Empty();

        /// <summary>Null when loaded successfully; otherwise the load error.</summary>
        public static string? LoadFailureReason { get; private set; }

        public int ProfileCount => _profiles.Count;

        public int CountByRegion(RegionalRegion region)
        {
            int count = 0;
            foreach (var profile in _profiles.Values)
            {
                if (profile.Region == region)
                {
                    count++;
                }
            }

            return count;
        }

        public RegionalRecruitmentProfile? GetProfile(string settlementId)
        {
            if (settlementId == null)
            {
                return null;
            }

            RegionalRecruitmentProfile profile;
            return _profiles.TryGetValue(settlementId, out profile) ? profile : null;
        }

        public string? GetTroopId(RegionalRegion region)
        {
            string troopId;
            return _troopByRegion.TryGetValue(region, out troopId) ? troopId : null;
        }

        /// <summary>
        /// Lazy, idempotent load from the deployed module folder. Safe to call
        /// from any game callback or diagnostic command; the first attempt
        /// wins and its outcome (loaded or failed) is sticky.
        /// </summary>
        public static void EnsureLoaded()
        {
            if (LoadFailureReason != null || Instance.ProfileCount > 0)
            {
                return;
            }

            string path = GetModuleDataPath();
            try
            {
                if (!File.Exists(path))
                {
                    Fail($"config file not found: {path}");
                    return;
                }

                Load(File.ReadAllText(path));
            }
            catch (Exception exception)
            {
                Fail(exception.Message);
            }
        }

        /// <summary>Installs an empty registry and records the given failure reason.</summary>
        public static void Fail(string reason)
        {
            Instance = Empty();
            LoadFailureReason = reason;
        }

        /// <summary>
        /// Loads and validates the config text. On success replaces the static
        /// instance; on failure installs an empty registry and records the
        /// reason. Never throws to the caller.
        /// </summary>
        public static void Load(string xmlText)
        {
            try
            {
                Instance = Parse(xmlText);
                LoadFailureReason = null;
            }
            catch (Exception exception)
            {
                Fail(exception.Message);
            }
        }

        /// <summary>Pure parse with full validation; throws on any error.</summary>
        public static RegionalRecruitmentRegistry Parse(string xmlText)
        {
            var document = new XmlDocument();
            document.LoadXml(xmlText);

            XmlElement root = document.DocumentElement
                ?? throw new InvalidOperationException("regional_recruitment.xml has no root element.");
            if (root.Name != "RegionalRecruitment")
            {
                throw new InvalidOperationException(
                    $"regional_recruitment.xml root must be <RegionalRecruitment>, got <{root.Name}>.");
            }

            var troopByRegion = ParseRegionMapping(root);
            var profiles = ParseSettlements(root, troopByRegion);
            return new RegionalRecruitmentRegistry(profiles, troopByRegion);
        }

        private static Dictionary<RegionalRegion, string> ParseRegionMapping(XmlElement root)
        {
            var result = new Dictionary<RegionalRegion, string>();
            XmlNodeList mappingNodes = root.SelectNodes("RegionMapping/Region")
                ?? throw new InvalidOperationException("regional_recruitment.xml is missing <RegionMapping>.");

            foreach (XmlNode node in mappingNodes)
            {
                if (!(node is XmlElement element))
                {
                    continue;
                }

                string regionText = element.GetAttribute("region");
                string troopId = element.GetAttribute("troop");
                if (!Enum.TryParse(regionText, ignoreCase: false, out RegionalRegion region))
                {
                    throw new InvalidOperationException($"RegionMapping has unknown region '{regionText}'.");
                }

                if (region == RegionalRegion.Core)
                {
                    throw new InvalidOperationException("RegionMapping must not define a Core troop mapping.");
                }

                if (string.IsNullOrWhiteSpace(troopId))
                {
                    throw new InvalidOperationException($"RegionMapping for '{region}' has an empty troop id.");
                }

                if (result.ContainsKey(region))
                {
                    throw new InvalidOperationException($"RegionMapping defines region '{region}' more than once.");
                }

                result.Add(region, troopId);
            }

            if (result.Count != 4)
            {
                throw new InvalidOperationException(
                    $"RegionMapping must define exactly 4 regions (North/South/West/East), got {result.Count}.");
            }

            return result;
        }

        private static Dictionary<string, RegionalRecruitmentProfile> ParseSettlements(
            XmlElement root,
            Dictionary<RegionalRegion, string> troopByRegion)
        {
            var profiles = new Dictionary<string, RegionalRecruitmentProfile>();
            XmlNodeList settlementNodes = root.SelectNodes("Settlements/Settlement")
                ?? throw new InvalidOperationException("regional_recruitment.xml is missing <Settlements>.");

            foreach (XmlNode node in settlementNodes)
            {
                if (!(node is XmlElement element))
                {
                    continue;
                }

                string settlementId = element.GetAttribute("id");
                if (string.IsNullOrWhiteSpace(settlementId))
                {
                    throw new InvalidOperationException("Settlements contains an entry with an empty id.");
                }

                if (!Enum.TryParse(element.GetAttribute("region"), ignoreCase: false, out RegionalRegion region))
                {
                    throw new InvalidOperationException(
                        $"Settlement '{settlementId}' has unknown region '{element.GetAttribute("region")}'.");
                }

                if (!Enum.TryParse(element.GetAttribute("tier"), ignoreCase: false, out RegionalTier tier))
                {
                    throw new InvalidOperationException(
                        $"Settlement '{settlementId}' has unknown tier '{element.GetAttribute("tier")}'.");
                }

                if (!double.TryParse(element.GetAttribute("chance"), NumberStyles.Float, CultureInfo.InvariantCulture, out double chance))
                {
                    throw new InvalidOperationException(
                        $"Settlement '{settlementId}' has an unparsable chance '{element.GetAttribute("chance")}'.");
                }

                if (Array.IndexOf(AllowedChances, chance) < 0)
                {
                    throw new InvalidOperationException(
                        $"Settlement '{settlementId}' has chance {chance}; allowed values are 0, 0.20, 0.30, 0.50.");
                }

                if (region == RegionalRegion.Core && chance != 0.0)
                {
                    throw new InvalidOperationException(
                        $"Core settlement '{settlementId}' must have chance 0, got {chance}.");
                }

                if (region != RegionalRegion.Core && chance == 0.0)
                {
                    throw new InvalidOperationException(
                        $"Non-core settlement '{settlementId}' must not have chance 0.");
                }

                if (region != RegionalRegion.Core && !troopByRegion.ContainsKey(region))
                {
                    throw new InvalidOperationException(
                        $"Settlement '{settlementId}' uses region '{region}' which has no troop mapping.");
                }

                var profile = new RegionalRecruitmentProfile(settlementId, region, tier, chance);
                if (profiles.ContainsKey(settlementId))
                {
                    throw new InvalidOperationException($"Settlement '{settlementId}' is defined more than once.");
                }

                profiles.Add(settlementId, profile);
            }

            if (profiles.Count == 0)
            {
                throw new InvalidOperationException("regional_recruitment.xml defines no settlements.");
            }

            return profiles;
        }

        private static string GetModuleDataPath()
        {
            // <game>/bin/Win64_Shipping_Client (or _wEditor) -> <game>/Modules/...
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string gameRoot = Path.GetFullPath(Path.Combine(baseDirectory, "..", ".."));
            return Path.Combine(gameRoot, "Modules", ModuleId, "ModuleData", "regional_recruitment.xml");
        }

        private static RegionalRecruitmentRegistry Empty()
        {
            return new RegionalRecruitmentRegistry(
                new Dictionary<string, RegionalRecruitmentProfile>(),
                new Dictionary<RegionalRegion, string>());
        }
    }
}
