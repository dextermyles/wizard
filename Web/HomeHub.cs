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