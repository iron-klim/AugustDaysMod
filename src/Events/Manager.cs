using System;
using System.Collections.Generic;
using Backend.Gamedesign.EventSystem.Events;
using Backend.Gamedesign.EventSystem.Utils;
using HarmonyLib;

namespace AugustDaysMod.Events
{
    public static class Manager
    {
        public const int RESULT_START = 5;
    }
    
    [HarmonyPatch(typeof(EventManager), "OrganizeEvents")]
    public static class PatchOrganizeEventsReplace
    {
        static void Postfix(EventManager __instance)
        {
            var eventsField = AccessTools.Field(typeof(EventManager), "events");
            var crisisField = AccessTools.Field(typeof(EventManager), "crisisEvents");

            var eventsDict = (Dictionary<int, AbstractEvent>)eventsField.GetValue(__instance);
            var crisisDict = (Dictionary<int, AbstractEvent>)crisisField.GetValue(__instance);

            DateTime startDate = new CoupBegin().EventInfo.startDate;
            for (int i = 0; i < eventsDict.Count; i++)
            {
                if (eventsDict.TryGetValue(i, out var eventData))
                {
                    if (eventData.EventInfo.endDate < startDate || 
                        (eventData.EventInfo.startDate > new DateTime(1985, 3, 11) && eventData.EventInfo.startDate < startDate))
                    {
                        eventsDict.Remove(eventData.Id);
                    }
                }
            }

            //Ивент на антиалкогольную кампанию по решению
            eventsDict.Add(5, new Backend.Gamedesign.EventSystem.Events.GameEvents.Event5());
            //Смещение даты начала ивента Невада-Семипалатинск
            eventsDict[290].EventInfo.startDate = new DateTime(1991, 8, 29);

            // Загружаем в словарь кризисных событий, потому что обычные проверяются только в определенные дни, а нам нужно каждый день
            crisisDict.Add(Negotiations.ID, new Negotiations());
            crisisDict.Add(CoupBegin.ID, new CoupBegin());
            crisisDict.Add(MiltaryAction.ID, new MiltaryAction());
            crisisDict.Add(YeltsinProblem.ID, new YeltsinProblem());
            crisisDict.Add(Broadcast.ID, new Broadcast());
            crisisDict.Add(PoliticalDirection.ID, new PoliticalDirection());
            crisisDict.Add(SovietsPalace.ID, new SovietsPalace());
            crisisDict.Add(Foros.ID, new Foros());
            crisisDict.Add(Final.ID, new Final());

            return;
        }
    }
}