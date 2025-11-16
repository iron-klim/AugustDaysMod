using System;
using AugustDaysMod.Assets;
using Backend.Gamedesign;
using Backend.Gamedesign.EventSystem.DataStructures;
using Backend.Gamedesign.EventSystem.Events;
using Backend.Gamedesign.Utils;
using Backend.Gamedesign.Utils.UnitsOfWork;

namespace AugustDaysMod.Events
{
    public class Negotiations : AbstractEvent
    {
        public const int ID = 1000;
        public const int TITLE = 0;
        public const int DESCRIPTION = 1;
        public const int ANSWER_1 = 2;
        public const int RESULT_1 = 3;
        
        private EventInfo eventInfo = new EventInfo()
        {
            priority = 0,
            startDate = new DateTime(1991, 8, 17),
            endDate = new DateTime(1991, 8, 18),
            countryLimitation = Countries.USSR,
            eventType = EventType.Dialog,
        };
        
        public override EventInfo EventInfo => eventInfo;
        
        public override bool CheckConditions(GameState gameState)
        {
            return !gameState.eventsDone.ContainsKey(ID) && gameState.date >= eventInfo.startDate;
        }

        public override UnitOfWork[] StartEvent(GameState gameState, EventDrawInfo eventDrawInfo)
        {
            const int optionsCount = 1;
            string eventTitle = Texts.GetEvent(ID, TITLE);
            string eventDesc =  Texts.GetEvent(ID, DESCRIPTION);
            
            EventUtils.SetupEventDrawInfo(eventDrawInfo, ID, eventInfo, optionsCount, eventTitle, eventDesc, Characters.Shenin);
            UnitOfWork[] unitOfWorkArray = new UnitOfWork[optionsCount];
            eventDrawInfo.optionsNames[0] = Texts.GetEvent(ID, ANSWER_1);
            eventDrawInfo.optionsEnabled[0] = true;
            
            return unitOfWorkArray;
        }
        
        public override string FinishEvent(GameState gameState, int choice)
        {
            EventUtils.ExecuteEventEffects(gameState, ID, choice);
            return Texts.GetEvent(ID, RESULT_1);
        }
    }
}