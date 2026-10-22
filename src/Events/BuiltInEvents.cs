using System;
using Backend.Gamedesign.EventSystem.DataStructures;
using Backend.Gamedesign.EventSystem.Utils;
using HarmonyLib;

namespace AugustDaysMod.Events
{
    // Встроенные события игры: кризисные работают все, а обычные — только те, что происходят после начала сценария мода.
    // Ограничиваем только автоматический выбор событий, поэтому события, которые игра вызывает напрямую
    // (из решений, заговоров, доктрин), продолжают работать
    [HarmonyPatch(typeof(EventManager), "IsEventAllowed")]
    public static class PatchEventManagerIsEventAllowed
    {
        // Дата начала сценария мода
        private static readonly DateTime ScenarioStart = new DateTime(1991, 8, 16);

        // Самый ранний старт игры. Обычные события, доступные с этой даты или раньше, не привязаны к истории:
        // это игровые механики вроде выборов, войн и пятилеток
        private static readonly DateTime EarliestGameStart = new DateTime(1985, 3, 11);

        static void Postfix(EventInfo eventInfo, ref bool __result)
        {
            if (__result && !eventInfo.isCrisis && eventInfo.startDate > EarliestGameStart && eventInfo.startDate < ScenarioStart)
            {
                __result = false;
            }
        }
    }
}
