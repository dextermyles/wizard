using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WizardGame.Services;
using WizardGame.Helpers;

namespace WizardGame
{
    public class GameSessionHub : Hub
    {
        private WizardService wizWS = new WizardService();

        public override Task OnConnected()
        {
            // Add your own code here.
            // For example: in a chat application, record the association between
            // the current connection ID and user name, and mark the user as online.
            // After the code in this method completes, the client is informed that
            // the connection is established; for example, in a JavaScript client,
            // the start().done callback is executed.
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            // connection id
            string connectionId = Context.ConnectionId;

            // remove player from game lobby
            wizWS.DeletePlayerFromGameLobby(0, 0, connectionId);

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            // Add your own code here.
            // For example: in a chat application, you might have marked the
            // user as offline after a period of inactivity; in that case 
            // mark the user as online again.
            return base.OnReconnected();
        }

        public void SendChatMessage(string playerName, string message, string groupNameId)
        {
            // get connectionId
            string connectionId = Context.ConnectionId;

            // call receiveChatMessage on client
            Clients.Group(groupNameId).receiveChatMessage(playerName, message);
        }

        public async Task JoinGameLobby(int playerId, int gameLobbyId, string groupNameId)
        {
            // add user to group
            await Groups.Add(Context.ConnectionId, groupNameId);

            // get player data
            Player player = wizWS.GetPlayerById(playerId);

            // get connectionId
            string connectionId = Context.ConnectionId;

            // call playerJoinedLobby on client
            Clients.Group(groupNameId).playerJoinedLobby(playerId, player.Name, connectionId);

            // add player to game lobby
            wizWS.UpdateGameLobbyPlayers(gameLobbyId, playerId, connectionId);
        }

        public async Task LeaveGameLobby(string playerName, string groupNameId)
        {
            // connection id
            string connectionId = Context.ConnectionId;

            // remove user from group
            await Groups.Remove(connectionId, groupNameId);

            // client playerLeftLobby on client
            Clients.Group(groupNameId).playerLeftLobby(playerName, connectionId);

            // remove player from lobby table
            wizWS.DeletePlayerFromGameLobby(0, 0, connectionId);
        }
    }
}