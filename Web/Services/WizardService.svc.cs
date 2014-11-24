using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WizardGame.Helpers;
using WizardGame.Services;
using RestSharp;

namespace WizardGame.Services
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
                    if (!row.IsNameNull())
                        player.Name = row.Name;

                    if (!row.IsPictureURLNull())
                        player.PictureURL = row.PictureURL;

                    player.PlayerId = row.PlayerId;

                    if (!row.IsUserIdNull())
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

        public Game UpdateGame(int gameId, int ownerPlayerId, DateTime? dateCompleted, int numPlayers, int maxHands, int intialDealerPosition, string scoreData, string groupNameId, int gameLobbyId)
        {
            try
            {
                // get db results
                Data.GameTableAdapters.GameTableAdapter gameAdapter = new Data.GameTableAdapters.GameTableAdapter();
                Data.Game.GameDataTable dtGame = gameAdapter.UpdateGame(gameId, ownerPlayerId, dateCompleted, numPlayers, maxHands, intialDealerPosition, scoreData, groupNameId, gameLobbyId);
                Data.Game.GameRow row = (Data.Game.GameRow) dtGame.Rows[0];

                if (row != null)
                {
                    Game game = new Game();

                    game.GameId = row.GameId;

                    if (!row.IsDateCompletedNull())
                        game.DateCompleted = row.DateCompleted;

                    if (!row.IsDateCreatedNull())
                        game.DateCreated = row.DateCreated;

                    game.InitialDealerPosition = row.InitialDealerPosition;
                    game.MaxHands = row.MaxHands;
                    game.NumPlayers = row.NumPlayers;

                    if (!row.IsOwnerPlayerIdNull())
                        game.OwnerPlayerId = row.OwnerPlayerId;

                    if (!row.IsScoreDataNull())
                        game.ScoreData = row.ScoreData;

                    if(!row.IsGroupNameIdNull())
                        game.GroupNameId = row.GroupNameId;

                    game.GameLobbyId = row.GameLobbyId;

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
            Game game = new Game();

            try
            {
                Data.GameTableAdapters.GameTableAdapter adapter = new Data.GameTableAdapters.GameTableAdapter();
                Data.Game.GameDataTable dtGame = adapter.GetGameById(gameId);

                if (dtGame != null && dtGame.Rows.Count > 0)
                {
                    Data.Game.GameRow row = (Data.Game.GameRow)dtGame.Rows[0];

                    game.GameId = row.GameId;

                    if (!row.IsDateCompletedNull())
                        game.DateCompleted = row.DateCompleted;

                    if (!row.IsDateCreatedNull())
                        game.DateCreated = row.DateCreated;

                    game.InitialDealerPosition = row.InitialDealerPosition;
                    game.MaxHands = row.MaxHands;
                    game.NumPlayers = row.NumPlayers;

                    if (!row.IsOwnerPlayerIdNull())
                        game.OwnerPlayerId = row.OwnerPlayerId;

                    if (!row.IsScoreDataNull())
                        game.ScoreData = row.ScoreData;

                    if (!row.IsGroupNameIdNull())
                        game.GroupNameId = row.GroupNameId;

                    game.GameLobbyId = row.GameLobbyId; 
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return game;
        }

        public GameHistory GetGameHistoryByGameId(int gameId)
        {
            GameHistory gameHistory = new GameHistory();

            try
            {
                Data.GameTableAdapters.GameHistoryTableAdapter adapter = new Data.GameTableAdapters.GameHistoryTableAdapter();
                Data.Game.GameHistoryDataTable dtGameHistory = adapter.GetGameHistoryByGameId(gameId);

                if (dtGameHistory != null && dtGameHistory.Rows.Count > 0)
                {
                    Data.Game.GameHistoryRow row = (Data.Game.GameHistoryRow)dtGameHistory.Rows[0];

                    gameHistory.DateCreated = row.DateCreated;
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

        public GameHistory GetGameHistoryById(int gameHistoryId)
        {
            GameHistory gameHistory = new GameHistory();

            try
            {
                Data.GameTableAdapters.GameHistoryTableAdapter adapter = new Data.GameTableAdapters.GameHistoryTableAdapter();
                Data.Game.GameHistoryDataTable dtGameHistory = adapter.GetGameHistoryById(gameHistoryId);

                if (dtGameHistory != null && dtGameHistory.Rows.Count > 0)
                {
                    Data.Game.GameHistoryRow row = (Data.Game.GameHistoryRow)dtGameHistory.Rows[0];

                    gameHistory.DateCreated = row.DateCreated;
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

        public HandHistory GetHandHistoryById(int handHistoryId)
        {
            HandHistory handHistory = new HandHistory();

            try
            {
                Data.GameTableAdapters.HandHistoryTableAdapter adapter = new Data.GameTableAdapters.HandHistoryTableAdapter();
                Data.Game.HandHistoryDataTable dtHandHistory = adapter.GetHandHistoryById(handHistoryId);

                if (dtHandHistory != null && dtHandHistory.Rows.Count > 0)
                {
                    Data.Game.HandHistoryRow row = (Data.Game.HandHistoryRow)dtHandHistory.Rows[0];

                    if (!row.IsDateCreatedNull())
                        handHistory.DateCreated = row.DateCreated;

                    if (!row.IsDateLastModifiedNull())
                        handHistory.DateLastModified = row.DateLastModified;

                    if (!row.IsDeckDataNull())
                        handHistory.DeckData = row.DeckData;

                    handHistory.GameId = row.GameId;
                    handHistory.HandHistoryId = row.HandHistoryId;

                    if (!row.IsPlayerDataNull())
                        handHistory.PlayerData = row.PlayerData;

                    if (!row.IsTrumpNull())
                        handHistory.Trump = row.Trump;
                }
            }
            catch (Exception ex)
            {
                // error handling
                LogError(ex);   
            }

            return handHistory;
        }

        public HandHistory GetLastHandHistoryByGameId(int gameId)
        {
            HandHistory handHistory = new HandHistory();

            try
            {
                Data.GameTableAdapters.HandHistoryTableAdapter adapter = new Data.GameTableAdapters.HandHistoryTableAdapter();
                Data.Game.HandHistoryDataTable dtHandHistory = adapter.GetLastHandHistoryByGameId(gameId);

                if (dtHandHistory != null && dtHandHistory.Rows.Count > 0)
                {
                    Data.Game.HandHistoryRow row = (Data.Game.HandHistoryRow)dtHandHistory.Rows[0];

                    if (!row.IsDateCreatedNull())
                        handHistory.DateCreated = row.DateCreated;

                    if (!row.IsDateLastModifiedNull())
                        handHistory.DateLastModified = row.DateLastModified;

                    if (!row.IsDeckDataNull())
                        handHistory.DeckData = row.DeckData;

                    handHistory.GameId = row.GameId;
                    handHistory.HandHistoryId = row.HandHistoryId;

                    if (!row.IsPlayerDataNull())
                        handHistory.PlayerData = row.PlayerData;

                    if (!row.IsTrumpNull())
                        handHistory.Trump = row.Trump;
                }
            }
            catch (Exception ex)
            {
                // error handling
                LogError(ex);
            }

            return handHistory;
        }

        public Player GetPlayerById(int playerId)
        {
            Player player = new Player();

            try
            {
                Data.SessionTableAdapters.PlayerTableAdapter adapter = new Data.SessionTableAdapters.PlayerTableAdapter();
                Data.Session.PlayerDataTable dtPlayer = adapter.GetPlayerById(playerId);

                if (dtPlayer != null && dtPlayer.Rows.Count > 0)
                {
                    Data.Session.PlayerRow row = (Data.Session.PlayerRow)dtPlayer.Rows[0];

                    if (!row.IsNameNull())
                        player.Name = row.Name;

                    if (!row.IsPictureURLNull())
                        player.PictureURL = row.PictureURL;

                    player.PlayerId = row.PlayerId;
                    
                    if (!row.IsUserIdNull())
                        player.UserId = row.UserId;
                }
            }
            catch (Exception ex)
            {
                // error handling
                LogError(ex);
            }

            return player;
        }

        public Player GetPlayerByName(string name)
        {
            Player player = new Player();

            try
            {
                Data.SessionTableAdapters.PlayerTableAdapter adapter = new Data.SessionTableAdapters.PlayerTableAdapter();
                Data.Session.PlayerDataTable dtPlayer = adapter.GetPlayerByName(name);

                if (dtPlayer != null && dtPlayer.Rows.Count > 0)
                {
                    Data.Session.PlayerRow row = (Data.Session.PlayerRow)dtPlayer.Rows[0];

                    if (!row.IsNameNull())
                        player.Name = row.Name;

                    if (!row.IsPictureURLNull())
                        player.PictureURL = row.PictureURL;

                    player.PlayerId = row.PlayerId;

                    if (!row.IsUserIdNull())
                        player.UserId = row.UserId;
                }
            }
            catch (Exception ex)
            {
                // error handling
                LogError(ex);
            }

            return player;
        }

        public User GetUserById(int userId)
        {
            User user = new User();

            try
            {
                Data.SessionTableAdapters.UserTableAdapter adapter = new Data.SessionTableAdapters.UserTableAdapter();
                Data.Session.UserDataTable dtUser = adapter.GetUserById(userId);

                if (dtUser != null && dtUser.Rows.Count > 0)
                {
                    Data.Session.UserRow row = (Data.Session.UserRow)dtUser.Rows[0];

                    user.Active = row.Active;
                    user.DateCreated = row.DateCreated;

                    if (!row.IsEmailAddressNull())
                        user.EmailAddress = row.EmailAddress;

                    if (!row.IsPasswordNull())
                        user.Password = string.Empty;

                    user.UserId = row.UserId;
                    user.Username = row.Username;

                    if (!row.IsFB_UserIdNull())
                        user.FB_UserId = row.FB_UserId;

                    if (!row.IsFB_SyncDateNull())
                        user.FB_SyncDate = row.FB_SyncDate;
                }
            }
            catch (Exception ex)
            {
                // error handling
                LogError(ex);
            }

            return user;
        }

        public User GetUserByUsername(string username)
        {
            User user = new User();

            try
            {
                Data.SessionTableAdapters.UserTableAdapter adapter = new Data.SessionTableAdapters.UserTableAdapter();
                Data.Session.UserDataTable dtUser = adapter.GetUserByUsername(username);

                if (dtUser != null && dtUser.Rows.Count > 0)
                {
                    Data.Session.UserRow row = (Data.Session.UserRow)dtUser.Rows[0];

                    user.Active = row.Active;
                    user.DateCreated = row.DateCreated;

                    if (!row.IsEmailAddressNull())
                        user.EmailAddress = row.EmailAddress;

                    if (!row.IsPasswordNull())
                        user.Password = string.Empty;

                    user.UserId = row.UserId;
                    user.Username = row.Username;

                    if (!row.IsFB_UserIdNull())
                        user.FB_UserId = row.FB_UserId;

                    if (!row.IsFB_SyncDateNull())
                        user.FB_SyncDate = row.FB_SyncDate;
                }
            }
            catch (Exception ex)
            {
                // error handling
                LogError(ex);
            }

            return user;
        }

        public Player[] ListPlayersByUserId(int userId)
        {
            List<Player> players = new List<Player>();

            try
            {
                Data.SessionTableAdapters.PlayerTableAdapter adapter = new Data.SessionTableAdapters.PlayerTableAdapter();
                Data.Session.PlayerDataTable dtPlayers = adapter.ListPlayersByUserId(userId);

                if (dtPlayers != null && dtPlayers.Rows.Count > 0)
                {
                    for (int i = 0; i < dtPlayers.Rows.Count; i++)
                    {
                        Data.Session.PlayerRow row = (Data.Session.PlayerRow)dtPlayers.Rows[i];
                        
                        Player player = new Player();

                        if (!row.IsNameNull())
                            player.Name = row.Name;

                        if (!row.IsPictureURLNull())
                            player.PictureURL = row.PictureURL;

                        player.PlayerId = row.PlayerId;

                        if (!row.IsUserIdNull())
                            player.UserId = row.UserId;

                        // add to list
                        players.Add(player);
                    }
                }
            }
            catch (Exception ex)
            {
                // error handling
                LogError(ex);
            }

            return players.ToArray();
        }

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
            request.AddParameter("subject", "Error: " + ex.Source);
            request.AddParameter("text", errorMessage.ToString());
            request.Method = Method.POST;

            // response
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return;
        }


        public bool EmailExists(string emailAddress)
        {
            bool result = false;

            // validation
            if (string.IsNullOrEmpty(emailAddress))
                return result;

            try
            {
                Data.SessionTableAdapters.RegisterTableAdapter adapter = new Data.SessionTableAdapters.RegisterTableAdapter();
                
                int emailExists = (int) adapter.EmailExists(emailAddress);

                if (emailExists > 0)
                    result = true;
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return result;
        }

        public bool UsernameExists(string username)
        {
            bool result = false;

            // validation
            if (string.IsNullOrEmpty(username))
                return result;

            try
            {
                Data.SessionTableAdapters.RegisterTableAdapter adapter = new Data.SessionTableAdapters.RegisterTableAdapter();

                int usernameExists = (int)adapter.UsernameExists(username);

                if (usernameExists > 0)
                    result = true;
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return result;
        }

        public Session Login(string username, string password, string ipAddress)
        {
            Session session = new Session();

            // validation
            if (string.IsNullOrEmpty(username) || 
                string.IsNullOrEmpty(password) || 
                string.IsNullOrEmpty(ipAddress))
                return session;

            try
            {
                Data.SessionTableAdapters.SessionTableAdapter adapter = new Data.SessionTableAdapters.SessionTableAdapter();

                Data.Session.SessionDataTable dtSession = adapter.Login(username, password, ipAddress);

                if (dtSession != null && dtSession.Rows.Count > 0)
                {
                    Data.Session.SessionRow row = (Data.Session.SessionRow)dtSession.Rows[0];
                    
                    session.DateCreated = row.DateCreated;
                    session.DateLastActive = row.DateLastActive;
                    
                    if (!row.IsIpAddressNull())
                        session.IpAddress = row.IpAddress;

                    if (!row.IsPlayerIdNull())
                        session.PlayerId = row.PlayerId;

                    if (!row.IsSecretNull())
                        session.Secret = row.Secret;

                    session.SessionId = row.SessionId;

                    if(!row.IsUserIdNull())
                        session.UserId = row.UserId;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);

                throw;
            }

            return session;
        }

        public NewUserResult NewUser(string username, string password, string emailAddress, bool active = true)
        {
            NewUserResult result = new NewUserResult();

            // validation
            if (string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(emailAddress))
            {
                result.Message = "Missing information";
                result.Result = false;
                result.Secret = string.Empty;

                return result;
            }

            try
            {
                Data.SessionTableAdapters.RegisterTableAdapter adapter = new Data.SessionTableAdapters.RegisterTableAdapter();

                Data.Session.RegisterDataTable dtRegister = adapter.NewUser(username, password, emailAddress, Functions.GetUserIPAddress(), active);

                if (dtRegister != null && dtRegister.Rows.Count > 0)
                {
                    System.Data.DataRow row = dtRegister.Rows[0];

                    result.Message = (string) row["Message"];
                    result.Result = (bool)row["Result"];
                    result.Secret = (string)row["Secret"];
                }
            }
            catch (Exception ex)
            {
                LogError(ex);

                throw;
            }

            return result;
        }

        public Session ValidateSession(string secret)
        {
            Session session = new Session();

            try
            {
                Data.SessionTableAdapters.SessionTableAdapter adapter = new Data.SessionTableAdapters.SessionTableAdapter();
                Data.Session.SessionDataTable dtSession = adapter.ValidateSession(secret, Functions.GetUserIPAddress());

                if (dtSession != null && dtSession.Rows.Count > 0)
                {
                    Data.Session.SessionRow row = (Data.Session.SessionRow)dtSession.Rows[0];

                    session.DateCreated = row.DateCreated;
                    session.DateLastActive = row.DateLastActive;

                    if (!row.IsIpAddressNull())
                        session.IpAddress = row.IpAddress;

                    if (!row.IsPlayerIdNull())
                        session.PlayerId = row.PlayerId;

                    if (!row.IsSecretNull())
                        session.Secret = row.Secret;

                    session.SessionId = row.SessionId;

                    if (!row.IsUserIdNull())
                        session.UserId = row.UserId;

                    if (!row.IsConnectionIdNull())
                        session.ConnectionId = row.ConnectionId;
                }
            }
            catch (Exception ex)
            {
                // error handling
                LogError(ex);
            }

            return session;
        }

        public HandHistory UpdateHandHistory(int handHistoryId, int gameId, string deckData, string playerData, string trump)
        {
            HandHistory history = new HandHistory();

            try
            {
                Data.GameTableAdapters.HandHistoryTableAdapter adapter = new Data.GameTableAdapters.HandHistoryTableAdapter();
                Data.Game.HandHistoryDataTable dtHistory = adapter.UpdateHandHistory(handHistoryId, gameId, deckData, playerData, trump);

                if (dtHistory != null && dtHistory.Rows.Count > 0)
                {
                    Data.Game.HandHistoryRow row = (Data.Game.HandHistoryRow)dtHistory.Rows[0];

                    if(!row.IsDateCreatedNull())
                        history.DateCreated = row.DateCreated;

                    if(!row.IsDateLastModifiedNull())
                        history.DateLastModified = row.DateLastModified;

                    if(!row.IsDeckDataNull())
                        history.DeckData = row.DeckData;

                    history.GameId = row.GameId;
                    history.HandHistoryId = row.HandHistoryId;
                    
                    if (!row.IsPlayerDataNull())
                        history.PlayerData = row.PlayerData;

                    if (!row.IsTrumpNull())
                        history.Trump = row.Trump;
                }
            }
            catch (Exception ex)
            {
                // error handling
                LogError(ex);
            }

            return history;
        }


        public Session UpdateSession(string secret, int userId, int playerId, string connectionId)
        {
            Session session = new Session();

            try
            {
                Data.SessionTableAdapters.SessionTableAdapter adapter = new Data.SessionTableAdapters.SessionTableAdapter();
                Data.Session.SessionDataTable dtSession = adapter.UpdateSession(secret, userId, playerId, Functions.GetUserIPAddress(), connectionId);

                if (dtSession != null && dtSession.Rows.Count > 0)
                {
                    Data.Session.SessionRow row = (Data.Session.SessionRow)dtSession.Rows[0];
                    
                    session.DateCreated = row.DateCreated;
                    session.DateLastActive = row.DateLastActive;

                    if (!row.IsIpAddressNull())
                        session.IpAddress = row.IpAddress;

                    if (!row.IsPlayerIdNull())
                        session.PlayerId = row.PlayerId;

                    if (!row.IsSecretNull())
                        session.Secret = row.Secret;

                    session.SessionId = row.SessionId;

                    if (!row.IsUserIdNull())
                        session.UserId = row.UserId;

                    if(!row.IsConnectionIdNull())
                        session.ConnectionId = row.ConnectionId;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return session;
        }


        public Session FacebookLogin(string fb_email, string fb_userId)
        {
            Session session = new Session();

            try
            {
                Data.SessionTableAdapters.SessionTableAdapter adapter = new Data.SessionTableAdapters.SessionTableAdapter();
                Data.Session.SessionDataTable dtSession = adapter.FacebookLogin(fb_email, fb_userId, Functions.GetUserIPAddress());

                if (dtSession != null && dtSession.Rows.Count > 0)
                {
                    Data.Session.SessionRow row = (Data.Session.SessionRow)dtSession.Rows[0];

                    session.DateCreated = row.DateCreated;
                    session.DateLastActive = row.DateLastActive;

                    if (!row.IsIpAddressNull())
                        session.IpAddress = row.IpAddress;

                    if (!row.IsPlayerIdNull())
                        session.PlayerId = row.PlayerId;

                    if (!row.IsSecretNull())
                        session.Secret = row.Secret;

                    session.SessionId = row.SessionId;

                    if (!row.IsUserIdNull())
                        session.UserId = row.UserId;

                    if (!row.IsConnectionIdNull())
                        session.ConnectionId = row.ConnectionId;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return session;
        }

        public User UpdateUser(int userId, string username, string password, string emailAddress, bool active, string fb_userId)
        {
            User user = new User();

            try
            {
                Data.SessionTableAdapters.UserTableAdapter adapter = new Data.SessionTableAdapters.UserTableAdapter();
                Data.Session.UserDataTable dtUser = adapter.UpdateUser(userId, username, password, emailAddress, active, fb_userId);

                if (dtUser != null && dtUser.Rows.Count > 0)
                {
                    Data.Session.UserRow row = (Data.Session.UserRow)dtUser.Rows[0];

                    user.Active = row.Active;
                    user.DateCreated = row.DateCreated;

                    if (!row.IsEmailAddressNull())
                        user.EmailAddress = row.EmailAddress;

                    if (!row.IsPasswordNull())
                        user.Password = string.Empty;

                    user.UserId = row.UserId;
                    user.Username = row.Username;

                    if (!row.IsFB_UserIdNull())
                        user.FB_UserId = row.FB_UserId;

                    if (!row.IsFB_SyncDateNull())
                        user.FB_SyncDate = row.FB_SyncDate;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return user;
        }


        public void DeleteSession(string secret)
        {
            try
            {
                Data.SessionTableAdapters.SessionTableAdapter adapter = new Data.SessionTableAdapters.SessionTableAdapter();
                adapter.DeleteSession(secret);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        public void DeleteOldSessions(int maxDays = 3)
        {
            try
            {
                Data.SessionTableAdapters.SessionTableAdapter adapter = new Data.SessionTableAdapters.SessionTableAdapter();
                adapter.DeleteOldSessions(maxDays);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }


        public Session GetSessionBySecret(string secret)
        {
            Session session = new Session();

            try
            {
                Data.SessionTableAdapters.SessionTableAdapter adapter = new Data.SessionTableAdapters.SessionTableAdapter();
                Data.Session.SessionDataTable dtSession = adapter.GetSessionBySecret(secret, Functions.GetUserIPAddress());

                if (dtSession != null && dtSession.Rows.Count > 0)
                {
                    Data.Session.SessionRow row = (Data.Session.SessionRow)dtSession.Rows[0];

                    session.DateCreated = row.DateCreated;
                    session.DateLastActive = row.DateLastActive;

                    if (!row.IsIpAddressNull())
                        session.IpAddress = row.IpAddress;

                    if (!row.IsPlayerIdNull())
                        session.PlayerId = row.PlayerId;

                    if (!row.IsSecretNull())
                        session.Secret = row.Secret;

                    if (!row.IsUserIdNull())
                        session.UserId = row.UserId;

                    if (!row.IsConnectionIdNull())
                        session.ConnectionId = row.ConnectionId;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return session;
        }


        public void DeleteGameLobbyById(int gameLobbyId)
        {
            try
            {
                Data.GameTableAdapters.GameLobbyTableAdapter adapter = new Data.GameTableAdapters.GameLobbyTableAdapter();
                adapter.DeleteGameLobbyById(gameLobbyId);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        public GameLobby GetGameLobbyById(int gameLobbyId)
        {
            GameLobby gameLobby = new GameLobby();

            try
            {
                Data.GameTableAdapters.GameLobbyTableAdapter adapter = new Data.GameTableAdapters.GameLobbyTableAdapter();
                Data.Game.GameLobbyDataTable dtGameLobby = adapter.GetGameLobbyById(gameLobbyId);

                if (dtGameLobby != null && dtGameLobby.Rows.Count > 0)
                {
                    Data.Game.GameLobbyRow row = (Data.Game.GameLobbyRow)dtGameLobby.Rows[0];

                    gameLobby.DateCreated = row.DateCreated;
                    gameLobby.GameLobbyId = row.GameLobbyId;
                    
                    if(!row.IsGroupNameIdNull())
                        gameLobby.GroupNameId = row.GroupNameId;

                    if (!row.IsInProgressNull())
                        gameLobby.InProgress = row.InProgress;

                    gameLobby.MaxPlayers = row.MaxPlayers;
                    
                    if (!row.IsNameNull())
                        gameLobby.Name = row.Name;

                    if (!row.IsOwnerPlayerIdNull())
                        gameLobby.OwnerPlayerId = row.OwnerPlayerId;

                    if (!row.IsPasswordNull())
                        gameLobby.Password = row.Password;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return gameLobby;
        }

        public GameLobby UpdateGameLobby(int gameLobbyId, int ownerPlayerId, string name, int maxPlayers, string groupNameId, string password, bool inProgress)
        {
            GameLobby gameLobby = new GameLobby();

            // validation
            if (maxPlayers < 3)
                maxPlayers = 3;

            if (maxPlayers > 6)
                maxPlayers = 6;

            try
            {
                Data.GameTableAdapters.GameLobbyTableAdapter adapter = new Data.GameTableAdapters.GameLobbyTableAdapter();
                Data.Game.GameLobbyDataTable dtGameLobby = adapter.UpdateGameLobby(gameLobbyId, ownerPlayerId, name, maxPlayers, groupNameId, password, inProgress);

                if (dtGameLobby != null && dtGameLobby.Rows.Count > 0)
                {
                    Data.Game.GameLobbyRow row = (Data.Game.GameLobbyRow)dtGameLobby.Rows[0];

                    gameLobby.DateCreated = row.DateCreated;
                    gameLobby.GameLobbyId = row.GameLobbyId;

                    if (!row.IsGroupNameIdNull())
                        gameLobby.GroupNameId = row.GroupNameId;

                    if (!row.IsInProgressNull())
                        gameLobby.InProgress = row.InProgress;

                    gameLobby.MaxPlayers = row.MaxPlayers;

                    if (!row.IsNameNull())
                        gameLobby.Name = row.Name;

                    if (!row.IsOwnerPlayerIdNull())
                        gameLobby.OwnerPlayerId = row.OwnerPlayerId;

                    if (!row.IsPasswordNull())
                        gameLobby.Password = row.Password;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return gameLobby;
        }


        public GameLobby[] ListAllGameLobbies(bool showInProgress)
        {
            List<GameLobby> gameLobbies = new List<GameLobby>();

            try
            {
                Data.GameTableAdapters.GameLobbyTableAdapter adapter = new Data.GameTableAdapters.GameLobbyTableAdapter();
                Data.Game.GameLobbyDataTable dtGameLobbies = adapter.ListAllGameLobbies(showInProgress);

                if (dtGameLobbies != null && dtGameLobbies.Rows.Count > 0)
                {
                    for (int i = 0; i < dtGameLobbies.Rows.Count; i++)
                    {
                        GameLobby gameLobby = new GameLobby();
                        Data.Game.GameLobbyRow row = (Data.Game.GameLobbyRow)dtGameLobbies.Rows[i];

                        gameLobby.DateCreated = row.DateCreated;
                        gameLobby.GameLobbyId = row.GameLobbyId;
                        
                        if (!row.IsGroupNameIdNull())
                            gameLobby.GroupNameId = row.GroupNameId;
                        
                        if (!row.IsInProgressNull())
                            gameLobby.InProgress = row.InProgress;
                        
                        gameLobby.MaxPlayers = row.MaxPlayers;
                       
                        if (!row.IsNameNull())
                            gameLobby.Name = row.Name;
                        
                        if (!row.IsOwnerPlayerIdNull())
                            gameLobby.OwnerPlayerId = row.OwnerPlayerId;
                        
                        if (!row.IsPasswordNull())
                            gameLobby.Password = row.Password;

                        // add to list
                        gameLobbies.Add(gameLobby);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return gameLobbies.ToArray();
        }
    }
}
