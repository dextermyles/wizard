using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WizardGame.Helpers
{
    [Serializable]
    public class GamePlayers
    {
        public int GamePlayersId = 0;
        public int GameId = 0;
        public int PlayerId = 0;
        public string ConnectionId = string.Empty;
        public ConnectionState ConnectionState = ConnectionState.DISCONNECTED;
        public DateTime DateLastActive;
    }
}