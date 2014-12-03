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

                            // broadcast player timed out
                            Clients.Group(game.GroupNameId).playerQuit(player);
                        }
                        else
                        {
                            // player timed out / went inactive
                            wizWS.UpdateGamePlayers(game.GameId, player.PlayerId, connectionId, ConnectionState.DISCONNECTED);

                            // broadcast player timed out
                            Clients.Group(game.GroupNameId).playerTimedOut(player);
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
                    Clients.Group(groupNameId).playerReconnected(player);
                }
                else
                {
                    // call playerJoinedLobby on client
                    Clients.Group(groupNameId).playerJoinedGame(player);
                }

                // add player to game lobby
                wizWS.UpdateGamePlayers(gameId, playerId, connectionId, ConnectionState.CONNECTED);

                // get game data
                Game game = wizWS.GetGameById(gameId);
                game.EventId = Guid.NewGuid().ToString();

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
            game.EventId = Guid.NewGuid().ToString();

            // update player last active date
            wizWS.UpdateGamePlayers(game.GameId, playerId, connectionId, ConnectionState.CONNECTED);

            // broadcast enterBid event
            Clients.Group(groupNameId).receiveBid(player, bid, game);
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
            gameState.SuitToFollow = suit;

            // update game state status to bidding
            gameState.Status = GameStateStatus.BiddingInProgress;

            // save game state in db
            game = wizWS.UpdateGame(game.GameId, game.GameLobbyId, game.OwnerPlayerId, null, gameState, groupNameId);
            game.EventId = Guid.NewGuid().ToString();

            // broadcast trump set
            Clients.Group(groupNameId).trumpUpdated(player, gameState.TrumpCard, game);
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

            // winning player (if round is over)
            Player playerWinner = null;

            // play card
            gameState.PlayCard(player.PlayerId, card);

            // check if turns ended
            bool IsTurnEnded = (gameState.Status == GameStateStatus.TurnEnded);

            if (IsTurnEnded)
            {
                // get highest card in pile
                Card highestCard = null;

                // if no trump determined (first non fluff led determines suit)
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

                        // no trump cards (fluff was likely led, first non fluff card led is new suit)
                        if (highestCard == null)
                        {
                            Card trumpCard = gameState.CardsPlayed.FirstOrDefault(c => c.Suit != Suit.Fluff);

                            trumpCards = gameState.CardsPlayed.Where(c => c.Suit == trumpCard.Suit).ToList();

                            highestCard = trumpCards.OrderByDescending(c => c.Value).FirstOrDefault();
                        }
                    }  
                }

                // get winning player
                playerWinner = gameState.Players.Where(p => p.PlayerId == highestCard.OwnerPlayerId).FirstOrDefault();

                // incrememnt num of tricks taken
                playerWinner.TricksTaken++;

                // clear turn flags (IsTurn + IsLastToAct)
                gameState.ClearTurnFlags();

                // get last to act index
                int winningPlayerIndex = Array.IndexOf(gameState.Players, playerWinner);

                // update current player index
                gameState.PlayerTurnIndex = winningPlayerIndex;

                // set turn flag
                gameState.Players[gameState.PlayerTurnIndex].IsTurn = true;

                // update last to act index
                gameState.LastToActIndex = winningPlayerIndex - 1;

                if (gameState.LastToActIndex < 0)
                    gameState.LastToActIndex = gameState.Players.Length - 1;

                // set last to act flag
                gameState.Players[gameState.LastToActIndex].IsLastToAct = true;

                // erase cards played
                gameState.CardsPlayed = null;

                // update game status
                gameState.Status = GameStateStatus.RoundInProgress;
            }

            // check if round ended
            if (gameState.HasRoundEnded())
                gameState.Status = GameStateStatus.RoundEnded;

            DateTime? dateGameEnded = null;
            bool IsRoundOver = (gameState.Status == GameStateStatus.RoundEnded);
            PlayerScore[] roundScoreHistory = null;

            // check if round ended
            if (IsRoundOver)
            {
                // update score cards
                gameState.AddScoreEntries();

                // save score history
                roundScoreHistory = gameState.GetPlayerScoreByRound(gameState.Round);

                // start next round
                bool canStartNextRound = gameState.StartNextRound();

                if (!canStartNextRound)
                {
                    // get point leader
                    Player pointLeader = gameState.GetPointLeader();

                    // broadcast game has ended
                    Clients.Group(groupNameId).gameEnded(pointLeader.PlayerId, pointLeader.Name);

                    // update game history
                    for(int i = 0; i < gameState.Players.Length; i++ ){
                        Player currentPlayer = gameState.Players[i];

                        int won = 0;

                        if(currentPlayer.PlayerId == pointLeader.PlayerId)
                            won = 1;

                        // update won flag
                        currentPlayer.Won = (won == 1);

                        // update game history table
                        wizWS.UpdateGameHistory(0, game.GameId, currentPlayer.PlayerId, currentPlayer.Score, won);
                    }
                    
                    // get date
                    dateGameEnded = DateTime.Now;
                }
            }

            // save game state in db
            game = wizWS.UpdateGame(game.GameId, game.GameLobbyId, game.OwnerPlayerId, dateGameEnded, gameState, groupNameId);
            game.EventId = Guid.NewGuid().ToString();

            // broadcast game data
            Clients.Group(groupNameId).cardPlayed(card, player, IsTurnEnded, playerWinner, IsRoundOver, roundScoreHistory, game);
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