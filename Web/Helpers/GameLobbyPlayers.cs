using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WizardGame.Helpers
{
    [Serializable]
    public class GameLobbyPlayers
    {
        public int GameLobbyPlayersId = 0;
        public int GameLobbyId = 0;
        public int PlayerId = 0;
        public string ConnectionId = string.Empty;
        public ConnectionState ConnectionState = ConnectionState.DISCONNECTED;
        public DateTime DateCreated;
        public DateTime DateLastActive;
    }
}