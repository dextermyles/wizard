using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WizardGame.Helpers
{
    public class Game
    {
        public int GameId = 0;
        public int GameLobbyId = 0;
        public int OwnerPlayerId = 0;
        public DateTime? DateCreated = null;
        public DateTime? DateCompleted = null;
        public GameState GameStateData = null;
        public string GroupNameId = string.Empty;
    }
}