using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Wizard.Helpers;
using Wizard.Services;
using RestSharp;

namespace Wizard.Services
{
    public class WizardService : IWizardService
    {
        public Card[] GenerateDeck()
        {
            // deck of cards
            Deck cardDeck = new Deck();

            // shuffle deck 
            cardDeck.ShuffleDeck();

            Card[] cards = cardDeck.Cards();

            return cards;
        }


        public Player UpdatePlayer(int playerId, string name, string pictureUrl, int userId)
        {
            try
            {
                // get db results
                Data.SessionTableAdapters.PlayerTableAdapter playerApapter = new Data.SessionTableAdapters.PlayerTableAdapter();
                Data.Session.PlayerDataTable dtPlayer = playerApapter.UpdatePlayer(playerId, name, pictureUrl, userId);
                Data.Session.PlayerRow row = (Data.Session.PlayerRow)dtPlayer.Rows[0];

                if (row != null)
                {
                    // new player
                    Player player = new Player();

                    // update object with db results
                    player.PlayerId = row.PlayerId;
                    player.Name = row.Name;
                    player.PictureURL = row.PictureURL;
                    player.UserId = row.UserId;

                    return player;
                }
            }
            catch (Exception ex)
            {
                // error handling
                LogError(ex);
            }

            return null;
        }

        public Game UpdateGame(int gameId, int ownerPlayerId, DateTime? dateCompleted, int numPlayers, int maxHands, int intialDealerPosition, string scoreData)
        {
            try
            {
                // get db results
                Data.GameTableAdapters.GameTableAdapter gameAdapter = new Data.GameTableAdapters.GameTableAdapter();
                Data.Game.GameDataTable dtGame = gameAdapter.UpdateGame(gameId, ownerPlayerId, dateCompleted, numPlayers, maxHands, intialDealerPosition, scoreData);
                Data.Game.GameRow row = (Data.Game.GameRow) dtGame.Rows[0];

                if (row != null)
                {
                    Game game = new Game();

                    game.GameId = row.GameId;
                    game.DateCompleted = row.DateCompleted;
                    game.DateCreated = row.DateCreated;
                    game.InitialDealerPosition = row.InitialDealerPosition;
                    game.MaxHands = row.MaxHands;
                    game.NumPlayers = row.NumPlayers;
                    game.OwnerPlayerId = row.OwnerPlayerId;
                    game.ScoreData = row.ScoreData;

                    return game;
                }

            }
            catch (Exception ex)
            {
                // error handling
                LogError(ex);
            }

            return null;
        }


        public GameHistory UpdateGameHistory(int gameHistoryId, int gameId, int playerId, int score, int won)
        {
            GameHistory gameHistory = null;

            try
            {
                // get db results
                Data.GameTableAdapters.GameHistoryTableAdapter adapter = new Data.GameTableAdapters.GameHistoryTableAdapter();
                Data.Game.GameHistoryDataTable dtGameHistory = adapter.UpdateGameHistory(gameHistoryId, gameId, playerId, score, won);
                Data.Game.GameHistoryRow row = (Data.Game.GameHistoryRow)dtGameHistory.Rows[0];

                if (row != null)
                {
                    gameHistory = new GameHistory();

                    gameHistory.GameHistoryId = row.GameHistoryId;
                    gameHistory.GameId = row.GameId;
                    gameHistory.PlayerId = row.PlayerId;
                    gameHistory.Score = row.Score;
                    gameHistory.Won = row.Won;     
                }
            }
            catch (Exception ex)
            {
                // error handling
                LogError(ex);
            }

            return gameHistory;
        }


        public Game GetGameById(int gameId)
        {
            Game game = null;

            try
            {

            }
            catch (Exception)
            {
                
                throw;
            }

            return game;
        }

        public GameHistory GetGameHistoryByGameId(int gameId)
        {
            GameHistory gameHistory = null;

            try
            {

            }
            catch (Exception)
            {
                // error handling
            }

            return gameHistory;
        }

        public GameHistory GetGameHistoryById(int gameHistoryId)
        {
            GameHistory gameHistory = null;

            try
            {

            }
            catch (Exception)
            {
                // error handling
            }

            return gameHistory;
        }

        public HandHistory GetHandHistoryById(int handHistoryId)
        {
            HandHistory handHistory = null;

            try
            {

            }
            catch (Exception)
            {
                // error handling
            }

            return handHistory;
        }

        public HandHistory GetLastHandHistoryByGameId(int gameId)
        {
            HandHistory handHistory = null;

            try
            {

            }
            catch (Exception)
            {
                // error handling
            }

            return handHistory;
        }

        public Player GetPlayerById(int playerId)
        {
            Player player = null;

            try
            {

            }
            catch (Exception)
            {
                // error handling
            }

            return player;
        }

        public Player GetPlayerByName(string name)
        {
            Player player = null;

            try
            {

            }
            catch (Exception)
            {
                // error handling
            }

            return player;
        }

        public User GetUserById(int userId)
        {
            User user = null;

            try
            {

            }
            catch (Exception)
            {
               // error handling
            }

            return user;
        }

        public Player[] ListPlayersByUserId(int userId)
        {
            Player[] playerList = null;

            try
            {

            }
            catch (Exception)
            {
                
                throw;
            }

            return playerList;
        }

        [OperationContract]
        public void LogError(Exception ex)
        {
            // get api key
            string apiKey = (string) ConfigurationManager.AppSettings["mailgun-api-key"];

            // format error
            StringBuilder errorMessage = new StringBuilder();

            errorMessage.AppendLine("Error details:");
            errorMessage.AppendLine(ex.Message + "\r\n");
            errorMessage.AppendLine("Stack trace:");
            errorMessage.AppendLine(ex.StackTrace);

            // has inner exception
            if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
            {
                errorMessage.AppendLine("\r\nInner exception details:");
                errorMessage.AppendLine(ex.InnerException.Message + "\r\n");
                errorMessage.AppendLine("Inner exception stack trace:");
                errorMessage.AppendLine(ex.InnerException.StackTrace);
            }

            // client
            RestClient client = new RestClient();
            client.BaseUrl = new Uri("https://api.mailgun.net/v2");
            client.Authenticator = new HttpBasicAuthenticator("api", apiKey);
            
            // request
            RestRequest request = new RestRequest();
            request.AddParameter("domain","wizard.apphb.com", ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "Administrator <no-reply@wizard.apphb.com>");
            request.AddParameter("to", "dexter.brock@gmail.com");
            request.AddParameter("subject", "Error:" + ex.Source);
            request.AddParameter("text", errorMessage.ToString());
            request.Method = Method.POST;

            // response
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return;
        }
    }
}
