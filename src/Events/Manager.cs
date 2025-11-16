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
        static bool Prefix(EventManager __instance)
        {
            var eventsField = AccessTools.Field(typeof(EventManager), "events");
            var crisisField = AccessTools.Field(typeof(EventManager), "crisisEvents");

            var eventsDict = (Dictionary<int, AbstractEvent>)eventsField.GetValue(__instance);
            var crisisDict = (Dictionary<int, AbstractEvent>)crisisField.GetValue(__instance);

            DateTime startDate = new CoupBegin().EventInfo.startDate; 
            foreach (var eventData in eventsDict)
            {
                if (eventData.Value.EventInfo.endDate < startDate)
                {
                    eventsDict.Remove(eventData.Key);
                }
            }
            
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

            return false;
        }
    }
}