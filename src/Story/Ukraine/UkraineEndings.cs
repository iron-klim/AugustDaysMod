using Backend.Gamedesign;
using Backend.Gamedesign.EndingsSystem;
using UnityEngine;

namespace AugustDaysMod.Story.Ukraine
{
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
