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

                // game is has not completed
                if (game != null && !game.DateCompleted.HasValue)
                {
                    // validate
                    if (!string.IsNullOrEmpty(game.GroupNameId))
                    {
                        // get player
                        Player player = wizWS.GetPlayerById(gp.PlayerId);

                        // validate
                        if (player.PlayerId > 0)
                        {
                            // update player state to disconnected
                            wizWS.UpdateGamePlayers(game.GameId, player.PlayerId, connectionId, ConnectionState.DISCONNECTED);

                            // players in game ref
                            Player[] playersInGame = wizWS.ListPlayersByGameId(gp.GameId);

                            int numPlayersInGame = 0;

                            // get num connected players
                            if (playersInGame != null)
                                numPlayersInGame = playersInGame.Count(p => p.ConnectionState == ConnectionState.CONNECTED);

                            // player left (navigated away from game page)
                            if (stopCalled)
                            {
                                // player left game
                                Clients.Group(game.GroupNameId).playerQuit(player, numPlayersInGame, false);
                            }
                            else
                            {
                                // broadcast player timed out
                                Clients.Group(game.GroupNameId).playerTimedOut(player, numPlayersInGame);
                            }
                        }
                    }
                } 
            }

            return base.OnDisconnected(stopCalled);
        }

        public async Task JoinGame(int playerId, int gameId, string groupNameId, bool reconnected)
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
                // game ref
                Game game = wizWS.GetGameById(gameId);

                // game has not completed
                if (game != null && !game.DateCompleted.HasValue)
                {
                    // gamePlayers ref
                    GamePlayers gp = wizWS.GetGamePlayersByGameIdAndPlayerId(gameId, playerId);

                    // add player to game lobby
                    wizWS.UpdateGamePlayers(gameId, playerId, connectionId, ConnectionState.CONNECTED);

                    // get connected players
                    Player[] playersInGame = wizWS.ListPlayersByGameId(gameId);

                    int numPlayersInGame = 0;

                    if (playersInGame != null)
                        numPlayersInGame = playersInGame.Count(p => p.ConnectionState == ConnectionState.CONNECTED);

                    // player exists in game already
                    if (gp != null && gp.GamePlayersId > 0)
                    {
                        // player reconnected
                        Clients.Group(groupNameId).playerReconnected(player, numPlayersInGame);

                        reconnected = true;
                    }
                    else
                    {
                        // player connected for first time
                        Clients.Group(groupNameId).playerJoinedGame(player, numPlayersInGame);
                    }

                    // broadcast game data
                    Clients.Caller.receiveGameData(game, reconnected, numPlayersInGame);
                }
                else
                {
                    // point leader ref
                    Player pointLeader = game.GameStateData.GetPointLeader();

                    // broadcast game data
                    Clients.Caller.gameEnded(pointLeader, game);
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

            Session session = Functions.GetSessionFromCookie();

            if(session != null) {
                wizWS.UpdateSession(session.Secret, session.UserId, player.PlayerId, connectionId);
            }
            

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

            // set trump card
            gameState.TrumpCard = new Card();
            gameState.TrumpCard.Suit = suit;
            gameState.TrumpCard.Value = 0;

            // set suit to follow
            gameState.SuitToFollow = Suit.None;

            // update game state status to bidding
            gameState.Status = GameStateStatus.BiddingInProgress;

            // save game state in db
            game = wizWS.UpdateGame(game.GameId, game.GameLobbyId, game.OwnerPlayerId, null, gameState, groupNameId);

            // broadcast trump set
            Clients.Group(groupNameId).trumpUpdated(player, gameState.TrumpCard, game);
        }

        public void CancelGame(int gameId, int playerId)
        {
            // get connectionId
            string connectionId = Context.ConnectionId;

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
            // player ref
            Player player = wizWS.GetPlayerById(playerId);

            // game ref
            Game game = wizWS.GetGameById(gameId);

            // gamePlayers ref
            GamePlayers gp = wizWS.GetGamePlayersByGameIdAndPlayerId(gameId, playerId);

            // player belongs to game
            if (gp != null && gp.GamePlayersId > 0)
            {
                // remove player from db
                wizWS.DeletePlayerFromGame(playerId, gameId, string.Empty);

                // get num remaining players
                Player[] playersInGame = wizWS.ListPlayersByGameId(gp.GameId);

                int numPlayersInGame = 0;

                if (playersInGame != null)
                    numPlayersInGame = playersInGame.Count(p => p.ConnectionState == ConnectionState.CONNECTED);

                // broadcast player quit
                Clients.Group(game.GroupNameId).playerQuit(player, numPlayersInGame, true);
            }
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

            // date game completed
            DateTime? dateGameEnded = null;

            // score from last round
            PlayerScore[] roundScoreHistory = null;

            // play card
            bool cardPlayedResult = gameState.PlayCard(player.PlayerId, card);

            // has round ended
            bool IsRoundOver = false;

            // player could not play card
            if (!cardPlayedResult)
            {
                // broadcast failed attempt to player
                Clients.Caller.cardPlayedFailed(card.ToString(), game);

                return;
            }

            // check if turns ended
            bool IsTurnEnded = (gameState.Status == GameStateStatus.TurnEnded);

            // turn has ended
            if (IsTurnEnded)
            {
                // get best card from CardsPlayed
                Card bestCard = gameState.GetBestCardFromCardsPlayed();

                // get winning player
                playerWinner = gameState.Players.Where(p => p.PlayerId == bestCard.OwnerPlayerId).FirstOrDefault();

                // player found
                if (playerWinner != null)
                {
                    // incrememnt num of tricks taken
                    playerWinner.TricksTaken++;

                    // clear turn flags (IsTurn + IsLastToAct)
                    gameState.ClearTurnFlags();

                    // get index of winning player
                    int winningPlayerIndex = -1;

                    // loop through players
                    for (int i = 0; i < gameState.Players.Length; i++)
                    {
                        if(gameState.Players[i].PlayerId == playerWinner.PlayerId)
                        {
                            // set winning player index
                            winningPlayerIndex = i;

                            break;
                        }
                    }

                    // update current player index
                    gameState.PlayerTurnIndex = winningPlayerIndex;

                    // set turn flag
                    gameState.Players[gameState.PlayerTurnIndex].IsTurn = true;

                    // update last to act index
                    gameState.LastToActIndex = (winningPlayerIndex - 1);

                    // validate last to act
                    if (gameState.LastToActIndex < 0)
                        gameState.LastToActIndex = (gameState.Players.Length - 1);

                    // set last to act flag
                    gameState.Players[gameState.LastToActIndex].IsLastToAct = true;

                    // save hand history in db before clearing cards
                    

                    // erase cards played
                    gameState.CardsPlayed = null;

                    // reset suit to follow
                    gameState.SuitToFollow = Suit.None;

                    // update game status
                    gameState.Status = GameStateStatus.RoundInProgress;
                }
            }

            // players have no cards
            if (!gameState.PlayersHaveCards())
            {
                // round has ended
                gameState.Status = GameStateStatus.RoundEnded;

                // update flag
                IsRoundOver = true;
            }
                
            // round has ended
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

                    // save game state
                    game = wizWS.UpdateGame(game.GameId, game.GameLobbyId, game.OwnerPlayerId, dateGameEnded, gameState, groupNameId);

                    // broadcast game has ended
                    Clients.Group(groupNameId).gameEnded(pointLeader, game);

                    return;
                }
            }

            // save game state in db
            game = wizWS.UpdateGame(game.GameId, game.GameLobbyId, game.OwnerPlayerId, dateGameEnded, gameState, groupNameId);

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