using System;
using AugustDaysMod.Assets;
using Backend.Gamedesign;
using Backend.Gamedesign.EventSystem;
using Backend.Gamedesign.EventSystem.DataStructures;
using Backend.Gamedesign.Utils;
using Backend.Gamedesign.Utils.UnitsOfWork;

namespace AugustDaysMod.Events
{
    [Event(ID)]
    public class Negotiations : CoupEvent
    {
        public const int ID = 1000;
        public const int TITLE = 0;
        public const int DESCRIPTION = 1;
        public const int ANSWER_1 = 2;
        public const int RESULT_1 = 3;

        public Negotiations() : base(new DateTime(1991, 8, 17))
        {
        }

        public override UnitOfWork[] StartEvent(GameState gameState, EventDrawInfo eventDrawInfo)
        {
            const int optionsCount = 1;
            string eventTitle = Texts.GetEvent(ID, TITLE);
            string eventDesc =  Texts.GetEvent(ID, DESCRIPTION);

            EventUtils.SetupEventDrawInfo(eventDrawInfo, ID, EventInfo, optionsCount, eventTitle, eventDesc, Characters.Shenin);
            eventDrawInfo.optionsNames[0] = Texts.GetEvent(ID, ANSWER_1);
            eventDrawInfo.optionsEnabled[0] = true;

            return new UnitOfWork[optionsCount];
        }

        public override string FinishEvent(GameState gameState, int choice)
        {
            EventUtils.ExecuteEventEffects(gameState, ID, choice);
            return Texts.GetEvent(ID, RESULT_1);
        }
    }
}
