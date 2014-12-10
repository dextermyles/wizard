using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WizardGame.Helpers
{
    public class GameHistoryStats
    {
        public int GameId = 0;
        public int PlayerId = 0;
        public int Score = 0;
        public bool Won = false;
        public string Name = string.Empty;
        public DateTime? DateCompleted;

    }
}