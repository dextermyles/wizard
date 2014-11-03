using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wizard.Helpers
{
    public class Game
    {
        public int GameId = 0;
        public int OwnerPlayerId = 0;
        public DateTime? DateCreated = null;
        public DateTime? DateCompleted = null;
        public int NumPlayers = 0;
        public int MaxHands = 0;
        public int InitialDealerPosition = 0;
        public string ScoreData = string.Empty;
    }
}