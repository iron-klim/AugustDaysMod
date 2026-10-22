using System.Collections.Generic;
using AugustDaysMod.Assets;
using Backend.Gamedesign;
using Backend.Gamedesign.EndingsSystem;
using Backend.Gamedesign.EndingsSystem.Endings;
using Frontend.Ending;
using UnityEngine;

namespace AugustDaysMod.Endings
{
    // Игра за Горбачева: путч победил, президента отстранили «по состоянию здоровья»
    [Ending(ID)]
    public class GorbachevResignation : AbstractEnding
    {
        public const int ID = 1001;
        public const int TITLE = 0;
        public const int DESCRIPTION = 1;

        public override List<EndingDrawInfo> GetEndingDrawInfos(GameState gameState)
        {
            var ending = new EndingDrawInfo()
            {
                text = Texts.GetEndings(ID, TITLE),
                header = Texts.GetEndings(ID, DESCRIPTION),
                markColor = Color.red,
                iconId = 10,
                useOldBackground = true
            };

            return new List<EndingDrawInfo>() {ending};
        }

        public override bool TestConditions(GameState gameState) => false;
    }
}
