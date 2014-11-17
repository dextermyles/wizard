using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WizardGame.Helpers
{
    public class HandHistory
    {
        public int HandHistoryId = 0;
        public int GameId = 0;
        public DateTime? DateCreated = null;
        public DateTime? DateLastModified = null;
        public string DeckData = string.Empty;
        public string PlayerData = string.Empty;
        public string Trump = string.Empty;
    }
}