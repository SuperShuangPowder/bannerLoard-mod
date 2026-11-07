namespace WXProvincialFreeCompany
{
    /// <summary>Geographic region of the unified-imperial recruitment design.</summary>
    public enum RegionalRegion
    {
        Core,
        North,
        South,
        West,
        East,
    }

    /// <summary>Frontier intensity tier; determines the auxiliary chance band.</summary>
    public enum RegionalTier
    {
        Core,
        Transition,
        Border,
        Frontier,
    }

    /// <summary>
    /// Immutable per-settlement recruitment profile, parsed from
    /// ModuleData/regional_recruitment.xml.
    /// </summary>
    public sealed class RegionalRecruitmentProfile
    {
        public RegionalRecruitmentProfile(
            string settlementId,
            RegionalRegion region,
            RegionalTier tier,
            double auxiliaryChance)
        {
            SettlementId = settlementId;
            Region = region;
            Tier = tier;
            AuxiliaryChance = auxiliaryChance;
        }

        public string SettlementId { get; }

        public RegionalRegion Region { get; }

        public RegionalTier Tier { get; }

        /// <summary>0 / 0.30 / 0.55 / 0.80; always 0 for the Core region.</summary>
        public double AuxiliaryChance { get; }

        public bool IsCoreRegion => Region == RegionalRegion.Core;

        public bool HasRecruitChance => AuxiliaryChance > 0.0;
    }
}
