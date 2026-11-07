namespace WXProvincialFreeCompany
{
    /// <summary>
    /// Pure decision logic for regional volunteer replacement. No game state,
    /// no RNG: the caller passes the roll, the chance, the slot cap and the
    /// current regional slot count, so this is fully unit-testable.
    ///
    /// v0.5.4: chance and cap come from the player-adjustable settings
    /// (defaults 50/30/20% and caps 3/2/1 per record 039's rebalance).
    /// </summary>
    public static class RegionalVolunteerDecision
    {
        /// <summary>
        /// Returns true when the vanilla basic volunteer should be replaced by
        /// the regional T1 for this notable update.
        /// </summary>
        /// <param name="originalTroopId">StringId returned by the inner model.</param>
        /// <param name="troopId">Configured regional root troop id; empty when missing.</param>
        /// <param name="roll">Random value in [0, 1); the campaign passes MBRandom.RandomFloat.</param>
        /// <param name="chance">Tier chance in [0, 1] from the player settings.</param>
        /// <param name="maxSlots">Tier regional slot cap from the player settings.</param>
        /// <param name="regionalSlotsCount">Regional troops currently in the notable's volunteer slots.</param>
        public static bool ShouldReplace(
            string originalTroopId,
            string troopId,
            double roll,
            double chance,
            int maxSlots,
            int regionalSlotsCount)
        {
            if (originalTroopId != "imperial_recruit")
            {
                return false; // protect noble recruits and everything else (record 032 §5)
            }

            if (string.IsNullOrEmpty(troopId))
            {
                return false; // missing mapping: vanilla fallback
            }

            if (chance <= 0.0)
            {
                return false;
            }

            if (maxSlots <= 0 || regionalSlotsCount >= maxSlots)
            {
                return false; // slot cap reached
            }

            return roll < chance;
        }
    }
}
