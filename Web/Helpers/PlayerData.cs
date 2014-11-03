using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wizard.Helpers
{
    public class PlayerData
    {
        public PlayerInfo[] PlayerInfo = null;
    }

    public class PlayerInfo
    {
        public int PlayerId = 0;
        public Card[] Cards = null;
        public int Bid = 0;
        public int TricksTaken = 0;
        public bool IsDealer = false; // is dealer this hand
        public int Points = 0; // number of points awarded
    }
}