using System.Collections.Generic;
using Backend.Gamedesign;
using Backend.Gamedesign.EndingsSystem.Endings;
using Frontend.Ending;
using UnityEngine;

namespace AugustDaysMod.Story
{
    // Концовка мода с текстами из assets/story.json. Наступает только из событий
    public abstract class StoryEnding : AbstractEnding
    {
        protected abstract int EndingId { get; }

        protected virtual Color MarkColor => Color.yellow;

        public override List<EndingDrawInfo> GetEndingDrawInfos(GameState gameState)
        {
            StoryEndingText text = StoryTexts.Ending(EndingId);
            return new List<EndingDrawInfo>
            {
                new EndingDrawInfo
                {
                    text = text.title,
                    header = text.description,
                    markColor = MarkColor,
                    iconId = 10,
                    useOldBackground = true
                }
            };
        }

        public override bool TestConditions(GameState gameState) => false;
    }
}
