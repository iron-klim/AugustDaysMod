using System;
using AugustDaysMod.Storage;
using AugustDaysMod.Union;
using Backend.Gamedesign;
using Backend.Gamedesign.EventSystem.DataStructures;
using Backend.Gamedesign.EventSystem;

namespace AugustDaysMod.Story
{
    // «Вести из Союза»: что произошло в СССР, пока игрок управляет другой страной. Повторяется по мере новостей
    [Event(ID)]
    public class UnionNewsEvent : StoryEvent
    {
        public const int ID = 2000;

        private string shownNews = "";

        public UnionNewsEvent() : base(null, new DateTime(1985, 1, 1), null, EventType.Message)
        {
        }

        protected override bool Condition(GameState gameState) =>
            gameState.playerCountry != Countries.USSR && ModStorage.For(gameState).news.Count > 0;

        protected override object[] DescriptionArgs(GameState gameState)
        {
            shownNews = UnionNews.TakeAll(gameState);
            return new object[] { shownNews };
        }

        // Событие повторяемое: снимаем отметку о выполнении
        protected override void AfterChoice(GameState gameState, int choice) => gameState.eventsDone.Remove(Id);
    }
}
