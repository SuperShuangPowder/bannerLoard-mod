namespace WXProvincialFreeCompany
{
    /// <summary>
    /// Pure decision logic for regional volunteer replacement. No game state,
    /// no RNG: the caller passes the roll value, so this is fully unit-testable
    /// and its behavior is deterministic for a given input.
    /// </summary>
    public static class RegionalVolunteerDecision
    {
        /// <summary>
        /// Returns true when the vanilla basic volunteer should be replaced by
        /// the regional T1 for this notable update. Full-map enablement: every
        /// settlement with a configured profile participates; the chance band
        /// of the profile decides the outcome.
        /// </summary>
        /// <param name="originalTroopId">StringId returned by the inner model.</param>
        /// <param name="profile">Recruitment profile of the notable's home settlement, or null.</param>
        /// <param name="troopId">Configured regional root troop id; empty when missing.</param>
        /// <param name="roll">Random value in [0, 1); the campaign passes MBRandom.RandomFloat.</param>
        public static bool ShouldReplace(
            string originalTroopId,
            RegionalRecruitmentProfile profile,
            string troopId,
            double roll)
        {
            if (originalTroopId != "imperial_recruit")
            {
                return false; // protect noble recruits and everything else (record 032 §5)
            }

            if (profile == null)
            {
                return false;
            }

            if (profile.IsCoreRegion)
            {
                return false; // core band is 0% by design
            }

            if (string.IsNullOrEmpty(troopId))
            {
                return false; // missing mapping: vanilla fallback
            }

            return roll < profile.AuxiliaryChance;
        }
    }
}
