using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using WizardGame.Services;
using WizardGame.Helpers;

namespace WizardGame
{
    public class HomeHub : Hub
    {
        public WizardService wizWS = new WizardService();

        public void CancelGame(int playerId, int gameId)
        {
            // get player ref
            Player player = wizWS.GetPlayerById(playerId);

            // get game data
            Game game = wizWS.GetGameById(gameId);

            // verify player is the host
            if (playerId == game.OwnerPlayerId)
            {
                // mark game as completed
                game.DateCompleted = DateTime.Now;

                // save game in db
                game = wizWS.UpdateGame(game.GameId, game.GameLobbyId, game.OwnerPlayerId, game.DateCompleted, game.GameStateData, game.GroupNameId);

                // broadcast game cancelled
                Clients.Group(game.GroupNameId).gameCancelled();
            }
        }

        public void QuitGame(int playerId, int gameId)
        {
            // gamePlayers ref
            GamePlayers gp = wizWS.GetGamePlayersByGameIdAndPlayerId(gameId, playerId);

            // connectionId
            string connectionId = Context.ConnectionId;

            // player belongs to game
            if (gp != null && gp.GamePlayersId > 0)
            {
                // remove player from db
                wizWS.UpdateGamePlayers(gameId, playerId, connectionId, ConnectionState.DISCONNECTED);
            }
        }

        public void ListAllGameLobbies(int maxLobbies)
        {
            // containers
            GameLobby[] gameLobbies = null;
            List<GameLobbyDetail> gameLobbyList = new List<GameLobbyDetail>();

            // get list of game lobbies
            try
            {
                // get all lobbies
                gameLobbies = wizWS.ListAllGameLobbies(false);

                // validate
                if (gameLobbies != null && gameLobbies.Length > 0)
                {
                    // loop through lobbies
                    for (int i = 0; i < gameLobbies.Length; i++)
                    {
                        // stop at max lobby count
                        if (i == maxLobbies)
                            break;

                        GameLobbyDetail lobbyDetail = new GameLobbyDetail();
                        GameLobby gameLobby = gameLobbies[i];
                        Player playerHost = wizWS.GetPlayerById(gameLobby.OwnerPlayerId);

                        if(gameLobby.DateCreated.HasValue)
                        lobbyDetail.DateCreated = gameLobby.DateCreated.Value;

                        lobbyDetail.GameLobbyId = gameLobby.GameLobbyId;
                        lobbyDetail.InProgress = gameLobby.InProgress;
                        lobbyDetail.MaxPlayers = gameLobby.MaxPlayers;
                        lobbyDetail.Name = gameLobby.Name;
                        lobbyDetail.NumPlayersInLobby = gameLobby.NumPlayersInLobby;
                        lobbyDetail.OwnerPlayerId = gameLobby.OwnerPlayerId;
                        lobbyDetail.OwnerPlayerName = playerHost.Name;
                        lobbyDetail.Password = gameLobby.Password;

                        gameLobbyList.Add(lobbyDetail);
                    }
                }
            }
            catch (Exception ex)
            {
                wizWS.LogError(ex);
            }

            // copy to array
            GameLobbyDetail[] lobbies = gameLobbyList.ToArray();

            // call client function
            Clients.Caller.getListOfGameLobbies(lobbies);
        }
    }
}