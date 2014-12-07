using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WizardGame.Helpers
{
    [Serializable]
    public class GameHistory
    {
        public int GameHistoryId = 0;
        public int PlayerId = 0;
        public int GameId = 0;
        public DateTime? DateCreated = null;
        public int Score = 0;
        public int Won = 0;
    }
}