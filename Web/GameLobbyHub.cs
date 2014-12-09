using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using WizardGame.Services;
using WizardGame.Helpers;

namespace WizardGame
{
    public class GameLobbyHub : Hub
    {
        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            // service
            WizardService wizWS = new WizardService();

            // connection id
            string connectionId = Context.ConnectionId;

            // get game lobby players data
            GameLobbyPlayers glp = wizWS.GetGameLobbyPlayersByConnectionId(connectionId);

            // validate
            if (glp != null && glp.GameLobbyId > 0)
            {
                // get game lobby data
                GameLobby lobby = wizWS.GetGameLobbyById(glp.GameLobbyId);

                // validate
                if (lobby != null && !string.IsNullOrEmpty(lobby.GroupNameId))
                {
                    // get player
                    Player player = wizWS.GetPlayerById(glp.PlayerId);

                    // validate
                    if (player != null && player.PlayerId > 0)
                    {
                        // player quit
                        if (stopCalled)
                        {
                            // remove player from game lobby
                            wizWS.DeletePlayerFromGameLobby(0, 0, connectionId);

                            // broadcast player left
                            Clients.Group(lobby.GroupNameId).playerLeftLobby(player.PlayerId, player.Name);
                        }
                        else
                        {
                            // player timed out / went inactive
                            wizWS.UpdateGameLobbyPlayers(lobby.GameLobbyId, player.PlayerId, connectionId, ConnectionState.DISCONNECTED);

                            // broadcast player timed out
                            Clients.Group(lobby.GroupNameId).playerTimedOut(player.PlayerId, player.Name);
                        }
                    }
                }
            }
            
            return base.OnDisconnected(stopCalled);
        }

        public void SendChatMessage(string playerName, string message, string groupNameId)
        {
            // get connectionId
            string connectionId = Context.ConnectionId;

            // call receiveChatMessage on client
            Clients.Group(groupNameId).receiveChatMessage(playerName, message);
        }

        public void StartGame(int gameLobbyId, string groupNameId)
        {
            // service
            WizardService wizWS = new WizardService();

            // create game
            GameLobby gameLobby = wizWS.GetGameLobbyById(gameLobbyId);
            Player[] players = wizWS.ListPlayersByGameLobbyId(gameLobbyId);

            if (gameLobby != null && players != null && players.Length > 2)
            {
                // create game
                GameState gameState = new GameState();

                // assign players to game
                gameState.StartGame(players);

                // update db
                Game game = wizWS.UpdateGame(0, gameLobby.GameLobbyId, gameLobby.OwnerPlayerId, null, gameState, groupNameId);

                // set in progress flag
                wizWS.UpdateGameLobby(gameLobbyId, gameLobby.OwnerPlayerId, gameLobby.Name, gameLobby.MaxPlayers, gameLobby.GroupNameId, gameLobby.Password, true);
                
                // redirect players to game
                Clients.Group(groupNameId).gameStarted(game);
            }
            else
            {
                // send error
                SendChatMessage("Server", "Game requires at least 3 players to start", groupNameId);
            }
        }

        public void RefreshPlayerList(int gameLobbyId, string groupNameId) {
            // service
            WizardService wizWS = new WizardService();

            // get list of players in lobby
            Player[] playerList = wizWS.ListPlayersByGameLobbyId(gameLobbyId);

            // broadcast game cancelled
            Clients.Caller.refreshPlayerList(playerList);
        }

        public void CancelGame(int gameLobbyId, string groupNameId)
        {
            // service
            WizardService wizWS = new WizardService();

            // delete game from database
            wizWS.DeleteGameLobbyById(gameLobbyId);

            // broadcast game cancelled
            Clients.Group(groupNameId).gameCancelled();
        }

        public async Task JoinGameLobby(int playerId, int gameLobbyId, string groupNameId)
        {
            // service
            WizardService wizWS = new WizardService();

            // get connectionId
            string connectionId = Context.ConnectionId;

            // add user to group
            await Groups.Add(Context.ConnectionId, groupNameId);

            // get player data
            Player player = wizWS.GetPlayerById(playerId);

            // validation
            if (player != null && player.PlayerId > 0)
            {
                // get game lobby players data
                GameLobbyPlayers glp = wizWS.GetGameLobbyPlayersByGameLobbyIdAndPlayerId(gameLobbyId, playerId);

                // get game if exists/created while user was disconnected
                Game game = wizWS.GetGameByGameLobbyId(gameLobbyId);

                // validation
                if (glp != null && glp.GameLobbyPlayersId > 0)
                {
                    // call playerJoinedLobby on client
                    Clients.Group(groupNameId).playerReconnected(playerId, player.Name, connectionId, game.GameId);
                }
                else
                {
                    // call playerJoinedLobby on client
                    Clients.Group(groupNameId).playerJoinedLobby(playerId, player.Name, connectionId, game.GameId);
                }

                // add player to game lobby
                wizWS.UpdateGameLobbyPlayers(gameLobbyId, playerId, connectionId, ConnectionState.CONNECTED);
            }
        }

        public async Task LeaveGameLobby(int gameLobbyId, int playerId, string playerName, string groupNameId)
        {
            // service
            WizardService wizWS = new WizardService();

            // connection id
            string connectionId = Context.ConnectionId;

            // remove user from group
            await Groups.Remove(connectionId, groupNameId);

            // client playerLeftLobby on client
            Clients.Group(groupNameId).playerLeftLobby(playerId, playerName);

            // remove player from lobby table
            wizWS.DeletePlayerFromGameLobby(0, 0, connectionId);
        }

        public void keepAlive(int playerId, int gameLobbyId, string groupNameId)
        {
            // service
            WizardService wizWS = new WizardService();

            // connection id
            string connectionId = Context.ConnectionId;

            // update database + last active time
            wizWS.UpdateGameLobbyPlayers(gameLobbyId, playerId, connectionId, ConnectionState.CONNECTED);
        }
    }
}