using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WizardGame.Helpers;
using WizardGame.Services;

namespace WizardGame
{
    public class GameHub : Hub
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

            // get GamePlayers data
            GamePlayers gp = wizWS.GetGamePlayersByConnectionId(connectionId);

            // validate
            if (gp != null && gp.GameId > 0)
            {
                // get game data
                Game game = wizWS.GetGameById(gp.GameId);

                // validate
                if (!string.IsNullOrEmpty(game.GroupNameId))
                {
                    // get player
                    Player player = wizWS.GetPlayerById(gp.PlayerId);

                    // validate
                    if (player.PlayerId > 0)
                    {
                        // player quit
                        if (stopCalled)
                        {
                            // broadcast player left
                            Clients.Group(game.GroupNameId).playerLeftGame(player.PlayerId, player.Name);

                            // remove player from game lobby
                            wizWS.DeletePlayerFromGame(player.PlayerId, game.GameId, string.Empty);
                        }
                        else
                        {
                            // player timed out / went inactive
                            wizWS.UpdateGamePlayers(game.GameId, player.PlayerId, connectionId, ConnectionState.DISCONNECTED);

                            // broadcast player timed out
                            Clients.Group(game.GroupNameId).playerTimedOut(player.PlayerId, player.Name);
                        }
                    }
                }
            }

            return base.OnDisconnected(stopCalled);
        }

        public async Task JoinGame(int playerId, int gameId, string groupNameId)
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
                GamePlayers gp = wizWS.GetGamePlayersByGameIdAndPlayerId(gameId, playerId);

                // validation
                if (gp != null && gp.GamePlayersId > 0)
                {
                    // call playerJoinedLobby on client
                    Clients.Group(groupNameId).playerReconnected(playerId, player.Name, connectionId);
                }
                else
                {
                    // call playerJoinedLobby on client
                    Clients.Group(groupNameId).playerJoinedGame(playerId, player.Name, connectionId);
                }

                // add player to game lobby
                wizWS.UpdateGamePlayers(gameId, playerId, connectionId, ConnectionState.CONNECTED);
            }
        }

        public void SendChatMessage(string playerName, string message, string groupNameId)
        {
            // get connectionId
            string connectionId = Context.ConnectionId;

            // call receiveChatMessage on client
            Clients.Group(groupNameId).receiveChatMessage(playerName, message);
        }

        public void ListPlayersInGame(int gameId, string groupNameId)
        {
            // service
            WizardService wizWS = new WizardService();

            // get connectionId
            string connectionId = Context.ConnectionId;

            // get list of players
            Player[] players = wizWS.ListPlayersByGameId(gameId);

            // validate
            if (players != null && players.Length > 0)
            {
                Clients.Group(groupNameId).receivePlayerList(players);
            }
        }

        public void Ping()
        {
            Clients.Caller.ping();
        }
    }
}