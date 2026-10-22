using System.Collections.Generic;
using Backend.Gamedesign;
using Backend.Gamedesign.EndingsSystem;
using Backend.Gamedesign.EndingsSystem.Endings;
using Frontend.Ending;
using UnityEngine;

namespace AugustDaysMod.Story.Ukraine
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

    public static class UkraineEndings
    {
        [Ending(ID)]
        public class Independent : StoryEnding
        {
            public const int ID = 2001;
            protected override int EndingId => ID;
            protected override Color MarkColor => new Color(1f, 0.84f, 0f);
        }

        [Ending(ID)]
        public class Defeated : StoryEnding
        {
            public const int ID = 2002;
            protected override int EndingId => ID;
            protected override Color MarkColor => new Color(0f, 0.34f, 0.72f);
        }
    }
}
