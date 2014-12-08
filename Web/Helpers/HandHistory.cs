using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WizardGame.Helpers
{
    [Serializable]
    public class HandHistory
    {
        public int HandHistoryId = 0;
        public int GameId = 0;
        public DateTime? DateCreated = null;
        public Card TrumpCard = null;
        public Suit SuitToFollow = Suit.None;
        public Card[] CardsPlayed = null;
        public int WinnerPlayerId = 0;
        public int Round = 0;
    }
}