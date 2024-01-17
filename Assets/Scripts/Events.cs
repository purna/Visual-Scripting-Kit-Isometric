
    // The Game Events used across the Game.
    // Anytime there is a need for a new event, it should be added here.

    public static class Events
    {
        public static OptionsMenuEvent OptionsMenuEvent = new OptionsMenuEvent();
        public static GameOverEvent GameOverEvent = new GameOverEvent();
    }

    // UI Events.
    public class OptionsMenuEvent: GameEvent
    {
        public bool Active;
    }

    // Events.


    public class GameOverEvent : GameEvent
    {
        public bool Win;
    }


