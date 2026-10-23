using Backend.Gamedesign.EndingsSystem;
using UnityEngine;

namespace AugustDaysMod.Story.Russia
{
    public static class RussiaEndings
    {
        [Ending(ID)]
        public class Reelected : StoryEnding
        {
            public const int ID = 2101;
            protected override int EndingId => ID;
            protected override Color MarkColor => new Color(0f, 0.22f, 0.65f);
        }

        [Ending(ID)]
        public class CommunistRevenge : StoryEnding
        {
            public const int ID = 2102;
            protected override int EndingId => ID;
            protected override Color MarkColor => new Color(0.8f, 0f, 0f);
        }

        [Ending(ID)]
        public class Impeached : StoryEnding
        {
            public const int ID = 2103;
            protected override int EndingId => ID;
            protected override Color MarkColor => Color.gray;
        }
    }
}
