using System;
using System.Collections.Generic;

namespace WXProvincialFreeCompany
{
    /// <summary>
    /// v0.5.2 five-point prototype gate (frozen per record 032 §10 and
    /// record 033 §3): exactly one core settlement plus one representative
    /// per region, jointly covering the 0 / 30 / 55 / 80 chance bands.
    ///
    ///   town_EN3              Saneopa    Core/Core        0     (town)
    ///   town_EN4              Argoron    North/Transition 0.30  (town)
    ///   town_ES2              Vostrum    South/Border     0.55  (town)
    ///   castle_village_EW1_1  Garontor   West/Frontier    0.80  (castle-bound village)
    ///   town_EN6              Amprela    East/Frontier    0.80  (town)
    ///
    /// All other 111 recruit points keep vanilla results. Full-map enablement
    /// simply removes this gate.
    /// </summary>
    public static class FivePointPrototype
    {
        public static readonly string[] EnabledSettlementIds =
        {
            "town_EN3",
            "town_EN4",
            "town_ES2",
            "castle_village_EW1_1",
            "town_EN6",
        };

        private static readonly HashSet<string> EnabledSet = CreateSet();

        public static bool IsEnabled(string settlementId)
        {
            return settlementId != null && EnabledSet.Contains(settlementId);
        }

        private static HashSet<string> CreateSet()
        {
            return new HashSet<string>(EnabledSettlementIds, StringComparer.Ordinal);
        }
    }
}
