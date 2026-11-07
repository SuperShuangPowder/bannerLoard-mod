using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace WXProvincialFreeCompany
{
    /// <summary>
    /// v0.5.4 player-adjustable recruitment settings: per-tier probability
    /// and per-notable regional slot cap. Persisted as a small XML file in
    /// the Bannerlord user Configs folder so the in-game option sliders (and
    /// manual edits) survive across sessions and saves.
    ///
    /// Defaults mirror the frozen design bands: transition 20% / border 30% /
    /// frontier 50%, caps 1/2/3. A missing or corrupt file falls back to
    /// defaults and never blocks the campaign.
    /// </summary>
    public sealed class RegionalRecruitmentSettings
    {
        public static RegionalRecruitmentSettings Instance { get; private set; } = Defaults();

        /// <summary>Null when loaded (or defaults used); otherwise the load error.</summary>
        public static string? LoadFailureReason { get; private set; }

        private static bool _loadAttempted;

        public int CoreChancePct { get; set; }
        public int TransitionChancePct { get; set; }
        public int BorderChancePct { get; set; }
        public int FrontierChancePct { get; set; }
        public int CoreMaxSlots { get; set; }
        public int TransitionMaxSlots { get; set; }
        public int BorderMaxSlots { get; set; }
        public int FrontierMaxSlots { get; set; }

        public static RegionalRecruitmentSettings Defaults()
        {
            return new RegionalRecruitmentSettings
            {
                CoreChancePct = 0,
                TransitionChancePct = 20,
                BorderChancePct = 30,
                FrontierChancePct = 50,
                CoreMaxSlots = 0,
                TransitionMaxSlots = 1,
                BorderMaxSlots = 2,
                FrontierMaxSlots = 3,
            };
        }

        /// <summary>Chance in [0, 1] for the given tier.</summary>
        public double GetChance(RegionalTier tier)
        {
            return GetChancePct(tier) / 100.0;
        }

        public int GetChancePct(RegionalTier tier)
        {
            switch (tier)
            {
                case RegionalTier.Transition:
                    return TransitionChancePct;
                case RegionalTier.Border:
                    return BorderChancePct;
                case RegionalTier.Frontier:
                    return FrontierChancePct;
                default:
                    return CoreChancePct;
            }
        }

        public int GetMaxSlots(RegionalTier tier)
        {
            switch (tier)
            {
                case RegionalTier.Transition:
                    return TransitionMaxSlots;
                case RegionalTier.Border:
                    return BorderMaxSlots;
                case RegionalTier.Frontier:
                    return FrontierMaxSlots;
                default:
                    return CoreMaxSlots;
            }
        }

        /// <summary>Lazy, idempotent load from the user Configs folder.</summary>
        public static void EnsureLoaded()
        {
            if (_loadAttempted)
            {
                return;
            }

            _loadAttempted = true;
            try
            {
                string path = GetSettingsFilePath();
                if (!File.Exists(path))
                {
                    Instance = Defaults();
                    return;
                }

                Instance = Parse(File.ReadAllText(path));
                LoadFailureReason = null;
            }
            catch (Exception exception)
            {
                LoadFailureReason = exception.Message;
                Instance = Defaults();
            }
        }

        /// <summary>Pure parse with clamping; missing elements fall back to defaults.</summary>
        public static RegionalRecruitmentSettings Parse(string xmlText)
        {
            var document = new XmlDocument();
            document.LoadXml(xmlText);

            XmlElement root = document.DocumentElement
                ?? throw new InvalidOperationException("settings file has no root element.");
            if (root.Name != "WXProvincialFreeCompanySettings")
            {
                throw new InvalidOperationException(
                    $"settings root must be <WXProvincialFreeCompanySettings>, got <{root.Name}>.");
            }

            var fallback = Defaults();
            var result = Defaults();
            result.CoreChancePct = Clamp(ReadInt(root, "ChanceCore", fallback.CoreChancePct), 0, 100);
            result.TransitionChancePct = Clamp(ReadInt(root, "ChanceTransition", fallback.TransitionChancePct), 0, 100);
            result.BorderChancePct = Clamp(ReadInt(root, "ChanceBorder", fallback.BorderChancePct), 0, 100);
            result.FrontierChancePct = Clamp(ReadInt(root, "ChanceFrontier", fallback.FrontierChancePct), 0, 100);
            result.CoreMaxSlots = Clamp(ReadInt(root, "MaxSlotsCore", fallback.CoreMaxSlots), 0, 6);
            result.TransitionMaxSlots = Clamp(ReadInt(root, "MaxSlotsTransition", fallback.TransitionMaxSlots), 0, 6);
            result.BorderMaxSlots = Clamp(ReadInt(root, "MaxSlotsBorder", fallback.BorderMaxSlots), 0, 6);
            result.FrontierMaxSlots = Clamp(ReadInt(root, "MaxSlotsFrontier", fallback.FrontierMaxSlots), 0, 6);
            return result;
        }

        /// <summary>Serializes the current values (used by Save and by the smoke test).</summary>
        public string ToXml()
        {
            var builder = new StringBuilder();
            builder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            builder.AppendLine("<!-- WXProvincialFreeCompany player settings. Editable; invalid values are clamped. -->");
            builder.AppendLine("<WXProvincialFreeCompanySettings>");
            builder.AppendLine($"  <ChanceCore>{CoreChancePct}</ChanceCore>");
            builder.AppendLine($"  <ChanceTransition>{TransitionChancePct}</ChanceTransition>");
            builder.AppendLine($"  <ChanceBorder>{BorderChancePct}</ChanceBorder>");
            builder.AppendLine($"  <ChanceFrontier>{FrontierChancePct}</ChanceFrontier>");
            builder.AppendLine($"  <MaxSlotsCore>{CoreMaxSlots}</MaxSlotsCore>");
            builder.AppendLine($"  <MaxSlotsTransition>{TransitionMaxSlots}</MaxSlotsTransition>");
            builder.AppendLine($"  <MaxSlotsBorder>{BorderMaxSlots}</MaxSlotsBorder>");
            builder.AppendLine($"  <MaxSlotsFrontier>{FrontierMaxSlots}</MaxSlotsFrontier>");
            builder.AppendLine("</WXProvincialFreeCompanySettings>");
            return builder.ToString();
        }

        /// <summary>Writes the current settings to the user Configs folder.</summary>
        public static void Save()
        {
            try
            {
                string path = GetSettingsFilePath();
                string directory = Path.GetDirectoryName(path)
                    ?? throw new InvalidOperationException("settings path has no directory.");
                Directory.CreateDirectory(directory);
                File.WriteAllText(path, Instance.ToXml(), new UTF8Encoding(false));
            }
            catch (Exception exception)
            {
                LoadFailureReason = exception.Message;
            }
        }

        public static string GetSettingsFilePath()
        {
            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documents, "Mount and Blade II Bannerlord", "Configs", "WXProvincialFreeCompany.xml");
        }

        private static int ReadInt(XmlElement root, string elementName, int fallback)
        {
            XmlNode? node = root.SelectSingleNode(elementName);
            if (node == null || !int.TryParse(node.InnerText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                return fallback;
            }

            return value;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}
