using System.Collections.Generic;
using AugustDaysMod.Assets;
using AugustDaysMod.Events;
using Backend.Gamedesign;
using Backend.Gamedesign.EndingsSystem.Endings;
using Frontend.Ending;
using UnityEngine;

namespace AugustDaysMod.Endings
{
    public class LiberalVictory : AbstractEnding
    {
        public const int ID = 1000;
        public const int TITLE = 0;
        public const int DESCRIPTION = 1;
        public const int YELTSIN_ALIVE = 2;
        
        public override List<EndingDrawInfo> GetEndingDrawInfos(GameState gameState)
        {
            var ending = new EndingDrawInfo()
            {
                text = Texts.GetEndings(ID, TITLE),
                header = Texts.GetEndings(ID, DESCRIPTION),
                markColor = Color.cyan,
                iconId = 10,
                useOldBackground = true
            };

            if (gameState.characters[Characters.Yeltsin].status != CharacterInfo.PoliticStatus.Dead)
            {
                ending.header += Texts.GetEndings(ID, YELTSIN_ALIVE);
            }
            
            return new List<EndingDrawInfo>() {ending};
        }

        public override bool TestConditions(GameState gameState) => false;
    }
    
}