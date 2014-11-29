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
        // service
        WizardService wizWS = new WizardService();

        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
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

                // get game data
                Game game = wizWS.GetGameById(gameId);

                // validate
                if (game != null && game.GameId > 0)
                {
                    // send game data
                    Clients.Caller.receiveGameData(game);
                } 
            }
        }

        public void EnterBid(int gameId, int playerId, int bid, string groupNameId)
        {
            // get connectionId
            string connectionId = Context.ConnectionId;

            // get game data
            Game game = wizWS.GetGameById(gameId);

            // get game state data
            GameState gameState = game.GameStateData;

            // get player data
            Player player = gameState.Players.Where(p => p.PlayerId == playerId).FirstOrDefault();

            // make sure gameId is set
            gameState.GameId = gameId;

            // enter player bid
            gameState.EnterBid(playerId, bid);

            // save data in db
            game = wizWS.UpdateGame(game.GameId, game.GameLobbyId, game.OwnerPlayerId, null, gameState, groupNameId);

            // update player last active date
            wizWS.UpdateGamePlayers(game.GameId, playerId, connectionId, ConnectionState.CONNECTED);

            // broadcast game data
            Clients.Group(groupNameId).receiveGameData(game);

            // broadcast enterBid event
            Clients.Group(groupNameId).receiveBid(player.PlayerId, player.Name, bid);
        }

        public void SetTrump(int gameId, int playerId, Suit suit, string groupNameId)
        {
            // get connectionId
            string connectionId = Context.ConnectionId;

            // get game data
            Game game = wizWS.GetGameById(gameId);

            // get game state data
            GameState gameState = game.GameStateData;

            // get player data
            Player player = gameState.Players.Where(p => p.PlayerId == playerId).FirstOrDefault();

            // set trump suit
            gameState.TrumpCard.Suit = suit;

            // save game state in db
            game = wizWS.UpdateGame(game.GameId, game.GameLobbyId, game.OwnerPlayerId, null, gameState, groupNameId);

            // broadcast trump set
            Clients.Group(groupNameId).trumpUpdated(player.PlayerId, player.Name, suit);

            // broadcast game data
            Clients.Group(groupNameId).receiveGameData(game);
        }

        public void PlayCard(int gameId, int playerId, Card card, string groupNameId)
        {
            // get connectionId
            string connectionId = Context.ConnectionId;

            // get game data
            Game game = wizWS.GetGameById(gameId);

            // get game state data
            GameState gameState = game.GameStateData;

            // get player data
            Player player = gameState.Players.Where(p => p.PlayerId == playerId).FirstOrDefault();

            // play card
            gameState.PlayCard(player.PlayerId, card);

            // check if turns ended
            if(gameState.Status == GameStateStatus.TurnEnded)
            {
                // get highest card in pile
                Card highestCard = null;

                // if no trump determined (last round)
                if (gameState.TrumpCard == null)
                {
                    gameState.TrumpCard = gameState.CardsPlayed.FirstOrDefault(c => c.Suit != Suit.Fluff);
                }

                // no trump card found, everyone played a fluff
                if (gameState.TrumpCard == null)
                {
                    // last fluff is the highest card
                    highestCard = gameState.CardsPlayed.LastOrDefault();
                }
                else
                {
                    // look for first wizard (if any)
                    highestCard = gameState.CardsPlayed.FirstOrDefault(c => c.Suit == Suit.Wizard);

                    // no wizard, get highest trump card
                    if (highestCard == null)
                    {
                        var trumpCards = gameState.CardsPlayed.Where(c => c.Suit == gameState.TrumpCard.Suit).ToList();

                        if (trumpCards.Count > 0)
                            highestCard = trumpCards.OrderByDescending(c => c.Value).FirstOrDefault();
                    }

                    // no trump cards, first card played is trump
                    if (highestCard == null)
                    {
                        Card trumpCard = gameState.CardsPlayed.FirstOrDefault(c => c.Suit != Suit.Fluff);

                        var trumpCards = gameState.CardsPlayed.Where(c => c.Suit == trumpCard.Suit).ToList();

                        highestCard = trumpCards.OrderByDescending(c => c.Value).FirstOrDefault();
                    }
                }

                

                // get winning player
                Player playerWinner = gameState.Players.Where(p => p.PlayerId == highestCard.OwnerPlayerId).FirstOrDefault();

                // incrememnt num of tricks taken
                playerWinner.TricksTaken++;

                // broadcast trick winner
                Clients.Group(groupNameId).playerWonTrick(playerWinner.PlayerId, playerWinner.Name, highestCard.ToString());

                // set turn flag
                playerWinner.IsTurn = true;

                // get last to act index
                int winningPlayerIndex = Array.IndexOf(gameState.Players, playerWinner);
                int lastToActIndex = winningPlayerIndex - 1;

                if (lastToActIndex < 0)
                    lastToActIndex = gameState.Players.Length - 1;

                // set last to act flag
                gameState.Players[lastToActIndex].IsLastToAct = true;

                // erase cards played
                gameState.CardsPlayed = null;

                // update game status
                gameState.Status = GameStateStatus.RoundInProgress;
            }

            // check if round ended
            if (gameState.HasRoundEnded())
            {
                // update score cards
                gameState.AddScoreEntries();

                // announce end of round
                Clients.Group(groupNameId).roundEnded();

                // start next round
                bool canStartNextRound = gameState.StartNextRound();

                if (!canStartNextRound)
                {
                    // broadcast game has ended
                    Clients.Group(groupNameId).gameEnded();
                }
            }

            // save game state in db
            game = wizWS.UpdateGame(game.GameId, game.GameLobbyId, game.OwnerPlayerId, null, gameState, groupNameId);

            // broadcast game data
            Clients.Group(groupNameId).receiveGameData(game);
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

        public void keepAlive(int playerId, int gameId, string groupNameId)
        {
            // connection id
            string connectionId = Context.ConnectionId;

            // update database + last active time
            wizWS.UpdateGamePlayers(gameId, playerId, connectionId, ConnectionState.CONNECTED);
        }
    }
}