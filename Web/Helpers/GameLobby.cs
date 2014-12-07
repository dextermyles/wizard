using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WizardGame.Helpers
{
    [Serializable]
    public class GameLobby
    {
        public int GameLobbyId = 0;
        public int OwnerPlayerId = 0;
        public string Name = string.Empty;
        public int MaxPlayers = 0; // between 3-6
        public DateTime? DateCreated = null;
        public string GroupNameId = string.Empty;
        public string Password = string.Empty;
        public bool InProgress = false;
        public int NumPlayersInLobby = 0;
    }
}