using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WizardGame.Helpers
{
    public class GameLobbyPlayers
    {
        public int GameLobbyPlayersId = 0;
        public int GameLobbyId = 0;
        public int PlayerId = 0;
        public string ConnectionId = string.Empty;
        public DateTime DateCreated;
    }
}