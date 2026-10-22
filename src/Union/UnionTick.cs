using System.Collections.Generic;
using System.Linq;
using Backend.Gamedesign;
using Backend.Gamedesign.Utils;
using HarmonyLib;

namespace AugustDaysMod.Union
{
    // Ежемесячный тик стран обрабатывает союзные республики, только если СССР — страна игрока.
    // Когда игрок управляет другой страной, делаем то же самое от лица СССР
    [HarmonyPatch(typeof(CountryUtils), nameof(CountryUtils.TickCountries))]
    public static class PatchTickCountriesUnion
    {
        static void Postfix(GameState gameState)
        {
            if (!SovietUnion.IsSeparateFromPlayer(gameState))
            {
                return;
            }

            using (UnionContext.Enter(gameState))
            {
                foreach (KeyValuePair<Countries, Country> subject in SovietUnion.Subjects(gameState).ToList())
                {
                    if (subject.Value.puppetType >= Country.PuppetType.PuppetOtherState)
                    {
                        continue;
                    }

                    if (subject.Value.stateOfEmergency > 0)
                    {
                        ++subject.Value.stateOfEmergency;
                    }

                    CountryUtils.TickSubjectsLeader(gameState, subject.Key);
                }

                // Как у игрока-СССР: правительства республик пересматриваются раз в квартал
                if (gameState.date.Month % 3 == 0)
                {
                    CountryUtils.RealizeFederalStatesAI(gameState);
                }
            }
        }
    }

    // Внутри союзного контекста не трогаем партию: игра выводит ее из фракций игрока, а они к СССР отношения не имеют
    [HarmonyPatch(typeof(CountryUtils), "ChangeRulingPartyNameForPlayerSubjects")]
    public static class PatchChangeRulingPartyNameForPlayerSubjects
    {
        static bool Prefix() => !UnionContext.Active;
    }
}
