using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WizardGame.Helpers
{
    [Serializable]
    public class GameLobbyDetail
    {
        public int GameLobbyId = 0;
        public DateTime DateCreated;
        public bool InProgress = false;
        public int MaxPlayers = 0;
        public string Name = string.Empty;
        public int NumPlayersInLobby = 0;
        public int OwnerPlayerId = 0;
        public string OwnerPlayerName = string.Empty;
        public string Password = string.Empty;
    }
}