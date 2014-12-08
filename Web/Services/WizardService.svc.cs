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
using Newtonsoft.Json;

namespace WizardGame.Services
{
    public class WizardService : IWizardService
    {
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

        public Game UpdateGame(int gameId, int gameLobbyId, int ownerPlayerId, DateTime? dateCompleted, GameState gameState, string groupNameId)
        {
            try
            {
                // serialize game state
                string gameStateData = JsonConvert.SerializeObject(gameState);

                // get db results
                Data.GameTableAdapters.GameTableAdapter gameAdapter = new Data.GameTableAdapters.GameTableAdapter();
                Data.Game.GameDataTable dtGame = gameAdapter.UpdateGame(gameId, gameLobbyId, ownerPlayerId, dateCompleted, gameStateData, groupNameId);
                Data.Game.GameRow row = (Data.Game.GameRow) dtGame.Rows[0];

                if (row != null)
                {
                    Game game = new Game();

                    game.GameId = row.GameId;

                    if (!row.IsDateCompletedNull())
                        game.DateCompleted = row.DateCompleted;

                    if (!row.IsDateCreatedNull())
                        game.DateCreated = row.DateCreated;

                    if (!row.IsOwnerPlayerIdNull())
                        game.OwnerPlayerId = row.OwnerPlayerId;

                    if (!row.IsGameStateDataNull())
                        game.GameStateData = JsonConvert.DeserializeObject<GameState>(row.GameStateData);

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

        public Game GetGameByConnectionId(string connectionId)
        {
            Game game = new Game();

            try
            {
                Data.GameTableAdapters.GameTableAdapter adapter = new Data.GameTableAdapters.GameTableAdapter();
                Data.Game.GameDataTable dtGame = adapter.GetGameByConnectionId(connectionId);

                if (dtGame != null && dtGame.Rows.Count > 0)
                {
                    Data.Game.GameRow row = (Data.Game.GameRow)dtGame.Rows[0];

                    game.GameId = row.GameId;

                    if (!row.IsDateCompletedNull())
                        game.DateCompleted = row.DateCompleted;

                    if (!row.IsDateCreatedNull())
                        game.DateCreated = row.DateCreated;

                    if (!row.IsOwnerPlayerIdNull())
                        game.OwnerPlayerId = row.OwnerPlayerId;

                    if (!row.IsGameStateDataNull())
                        game.GameStateData = JsonConvert.DeserializeObject<GameState>(row.GameStateData);

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

        public Game GetGameByGameLobbyId(int gameLobbyId)
        {
            Game game = new Game();

            try
            {
                Data.GameTableAdapters.GameTableAdapter adapter = new Data.GameTableAdapters.GameTableAdapter();
                Data.Game.GameDataTable dtGame = adapter.GetGameByGameLobbyId(gameLobbyId);

                if (dtGame != null && dtGame.Rows.Count > 0)
                {
                    Data.Game.GameRow row = (Data.Game.GameRow)dtGame.Rows[0];

                    game.GameId = row.GameId;

                    if (!row.IsDateCompletedNull())
                        game.DateCompleted = row.DateCompleted;

                    if (!row.IsDateCreatedNull())
                        game.DateCreated = row.DateCreated;

                    if (!row.IsOwnerPlayerIdNull())
                        game.OwnerPlayerId = row.OwnerPlayerId;

                    if (!row.IsGameStateDataNull())
                        game.GameStateData = JsonConvert.DeserializeObject<GameState>(row.GameStateData);

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

                    if (!row.IsOwnerPlayerIdNull())
                        game.OwnerPlayerId = row.OwnerPlayerId;

                    if (!row.IsGameStateDataNull())
                        game.GameStateData = JsonConvert.DeserializeObject<GameState>(row.GameStateData);

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

        public HandHistory[] GetHandHistoryByGameId(int gameId)
        {
            // history list
            List<HandHistory> handHistoryList = new List<HandHistory>();

            try
            {
                // adapters
                Data.GameTableAdapters.HandHistoryTableAdapter adapter = new Data.GameTableAdapters.HandHistoryTableAdapter();
                Data.Game.HandHistoryDataTable dtHistory = adapter.GetHandHistoryByGameId(gameId);

                // history exists and not empty
                if (dtHistory != null && dtHistory.Rows.Count > 0)
                {
                    for(int i = 0; i < dtHistory.Rows.Count; i++) {
                        // read row
                        Data.Game.HandHistoryRow row = (Data.Game.HandHistoryRow)dtHistory.Rows[i];

                        // hand history obj
                        HandHistory history = new HandHistory();

                        history.HandHistoryId = row.HandHistoryId;
                        history.GameId = row.GameId;
                        history.DateCreated = row.DateCreated;
                        history.TrumpCard = JsonConvert.DeserializeObject<Card>(row.TrumpCard);
                        history.SuitToFollow = (Suit)row.SuitToFollow;
                        history.CardsPlayed = JsonConvert.DeserializeObject<Card[]>(row.CardsPlayed);
                        history.WinnerPlayerId = row.WinnerPlayerId;
                        history.Round = row.Round;

                        // add to list
                        handHistoryList.Add(history);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return handHistoryList.ToArray();
        }

        public HandHistory GetHandHistoryById(int handHistoryId)
        {
            HandHistory history = new HandHistory();

            try
            {
                Data.GameTableAdapters.HandHistoryTableAdapter adapter = new Data.GameTableAdapters.HandHistoryTableAdapter();
                Data.Game.HandHistoryDataTable dtHistory = adapter.GetHandHistoryByGameId(handHistoryId);

                if (dtHistory != null && dtHistory.Rows.Count > 0)
                {
                    Data.Game.HandHistoryRow row = (Data.Game.HandHistoryRow)dtHistory.Rows[0];

                    history.HandHistoryId = row.HandHistoryId;
                    history.GameId = row.GameId;
                    history.DateCreated = row.DateCreated;
                    history.TrumpCard = JsonConvert.DeserializeObject<Card>(row.TrumpCard);
                    history.SuitToFollow = (Suit)row.SuitToFollow;
                    history.CardsPlayed = JsonConvert.DeserializeObject<Card[]>(row.CardsPlayed);
                    history.WinnerPlayerId = row.WinnerPlayerId;
                    history.Round = row.Round;
                }
            }
            catch (Exception ex)
            {
                // error handling
                LogError(ex);
            }

            return history;
        }

        public HandHistory GetLastHandHistoryByGameId(int gameId)
        {
            HandHistory history = new HandHistory();

            try
            {
                Data.GameTableAdapters.HandHistoryTableAdapter adapter = new Data.GameTableAdapters.HandHistoryTableAdapter();
                Data.Game.HandHistoryDataTable dtHistory = adapter.GetLastHandHistoryByGameId(gameId);

                if (dtHistory != null && dtHistory.Rows.Count > 0)
                {
                    Data.Game.HandHistoryRow row = (Data.Game.HandHistoryRow)dtHistory.Rows[0];

                    history.HandHistoryId = row.HandHistoryId;
                    history.GameId = row.GameId;
                    history.DateCreated = row.DateCreated;
                    history.TrumpCard = JsonConvert.DeserializeObject<Card>(row.TrumpCard);
                    history.SuitToFollow = (Suit)row.SuitToFollow;
                    history.CardsPlayed = JsonConvert.DeserializeObject<Card[]>(row.CardsPlayed);
                    history.WinnerPlayerId = row.WinnerPlayerId;
                    history.Round = row.Round;
                }
            }
            catch (Exception ex)
            {
                // error handling
                LogError(ex);
            }

            return history;
        }

        public Game GetLatestGameByPlayerId(int playerId)
        {
            Game game = new Game();

            try
            {
                Data.GameTableAdapters.GameTableAdapter adapter = new Data.GameTableAdapters.GameTableAdapter();
                Data.Game.GameDataTable dtGame = adapter.GetLatestGameByPlayerId(playerId);

                if (dtGame != null && dtGame.Rows.Count > 0)
                {
                    Data.Game.GameRow row = (Data.Game.GameRow)dtGame.Rows[0];

                    game.GameId = row.GameId;

                    if (!row.IsDateCompletedNull())
                        game.DateCompleted = row.DateCompleted;

                    if (!row.IsDateCreatedNull())
                        game.DateCreated = row.DateCreated;

                    if (!row.IsOwnerPlayerIdNull())
                        game.OwnerPlayerId = row.OwnerPlayerId;

                    if (!row.IsGameStateDataNull())
                        game.GameStateData = JsonConvert.DeserializeObject<GameState>(row.GameStateData);

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

        public Player GetPlayerByConnectionId(string connectionId)
        {
            Player player = new Player();

            try
            {
                Data.SessionTableAdapters.PlayerTableAdapter adapter = new Data.SessionTableAdapters.PlayerTableAdapter();
                Data.Session.PlayerDataTable dtPlayer = adapter.GetPlayerByConnectionId(connectionId);

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

        public Player[] ListPlayersByGameId(int gameId)
        {
            List<Player> players = new List<Player>();

            try
            {
                Data.SessionTableAdapters.PlayerTableAdapter adapter = new Data.SessionTableAdapters.PlayerTableAdapter();
                Data.Session.PlayerDataTable dtPlayer = adapter.ListPlayersByGameId(gameId);

                if (dtPlayer != null && dtPlayer.Rows.Count > 0)
                {
                    for (int i = 0; i < dtPlayer.Rows.Count; i++)
                    {
                        Data.Session.PlayerRow row = (Data.Session.PlayerRow)dtPlayer.Rows[i];

                        Player player = new Player();

                        if (!row.IsNameNull())
                            player.Name = row.Name;

                        if (!row.IsPictureURLNull())
                            player.PictureURL = row.PictureURL;

                        player.PlayerId = row.PlayerId;

                        if (!row.IsUserIdNull())
                            player.UserId = row.UserId;

                        player.ConnectionId = row.ConnectionId;

                        switch (row.ConnectionState)
                        {
                            case "CONNECTED":
                                player.ConnectionState = ConnectionState.CONNECTED;
                                break;
                            case "DISCONNECTED":
                                player.ConnectionState = ConnectionState.DISCONNECTED;
                                break;
                            case "INACTIVE":
                                player.ConnectionState = ConnectionState.INACTIVE;
                                break;
                        }

                        // add to list
                        players.Add(player);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return players.ToArray();
        }

        public Player[] ListPlayersByGameLobbyId(int gameLobbyId)
        {
            List<Player> players = new List<Player>();

            try
            {
                Data.SessionTableAdapters.PlayerTableAdapter adapter = new Data.SessionTableAdapters.PlayerTableAdapter();
                Data.Session.PlayerDataTable dtPlayer = adapter.ListPlayersByGameLobbyId(gameLobbyId);

                if (dtPlayer != null && dtPlayer.Rows.Count > 0)
                {
                    for (int i = 0; i < dtPlayer.Rows.Count; i++)
                    {
                        Data.Session.PlayerRow row = (Data.Session.PlayerRow)dtPlayer.Rows[i];

                        Player player = new Player();

                        if (!row.IsNameNull())
                            player.Name = row.Name;

                        if (!row.IsPictureURLNull())
                            player.PictureURL = row.PictureURL;

                        player.PlayerId = row.PlayerId;

                        if (!row.IsUserIdNull())
                            player.UserId = row.UserId;

                        player.ConnectionId = row.ConnectionId;

                        switch (row.ConnectionState)
                        {
                            case "CONNECTED":
                                player.ConnectionState = ConnectionState.CONNECTED;
                                break;
                            case "DISCONNECTED":
                                player.ConnectionState = ConnectionState.DISCONNECTED;
                                break;
                            case "INACTIVE":
                                player.ConnectionState = ConnectionState.INACTIVE;
                                break;
                        }

                        // add to list
                        players.Add(player);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return players.ToArray();
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

                Data.Session.SessionDataTable dtSession = adapter.Login(username.ToLower().Trim(), password, ipAddress);

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

        public HandHistory UpdateHandHistory(int handHistoryId, int gameId, Card trumpCard, Suit suitToFollow, Card[] cardsPlayed, int winnerPlayerId, int round)
        {
            HandHistory history = new HandHistory();

            try
            {

                string trumpCardData = JsonConvert.SerializeObject(trumpCard);
                string cardsPlayedData = JsonConvert.SerializeObject(cardsPlayed);

                Data.GameTableAdapters.HandHistoryTableAdapter adapter = new Data.GameTableAdapters.HandHistoryTableAdapter();
                Data.Game.HandHistoryDataTable dtHistory = adapter.UpdateHandHistory(handHistoryId, gameId, trumpCardData, (int)suitToFollow, cardsPlayedData, winnerPlayerId, round);

                if (dtHistory != null && dtHistory.Rows.Count > 0)
                {
                    Data.Game.HandHistoryRow row = (Data.Game.HandHistoryRow)dtHistory.Rows[0];

                    history.HandHistoryId = row.HandHistoryId;
                    history.GameId = row.GameId;
                    history.DateCreated = row.DateCreated;
                    history.TrumpCard = JsonConvert.DeserializeObject<Card>(row.TrumpCard);
                    history.SuitToFollow = (Suit) row.SuitToFollow;
                    history.CardsPlayed = JsonConvert.DeserializeObject<Card[]>(row.CardsPlayed);
                    history.WinnerPlayerId = row.WinnerPlayerId;
                    history.Round = row.Round;
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

        public void DeletePlayerFromGame(int playerId, int gameId, string connectionId)
        {
            try
            {
                Data.GameTableAdapters.GamePlayersTableAdapter adapter = new Data.GameTableAdapters.GamePlayersTableAdapter();
                adapter.DeletePlayerFromGame(gameId, playerId, connectionId);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
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

        public GameLobby GetGameLobbyByConnectionId(string connectionId)
        {
            GameLobby gameLobby = new GameLobby();

            try
            {
                Data.GameTableAdapters.GameLobbyTableAdapter adapter = new Data.GameTableAdapters.GameLobbyTableAdapter();
                Data.Game.GameLobbyDataTable dtGameLobby = adapter.GetGameLobbyByConnectionId(connectionId);

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

                    if (!row.IsNumPlayersInLobbyNull())
                        gameLobby.NumPlayersInLobby = row.NumPlayersInLobby;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return gameLobby;
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

                    if (!row.IsNumPlayersInLobbyNull())
                        gameLobby.NumPlayersInLobby = row.NumPlayersInLobby;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return gameLobby;
        }

        public GameLobbyPlayers GetGameLobbyPlayersByConnectionId(string connectionId)
        {
            GameLobbyPlayers glp = new GameLobbyPlayers();

            try
            {
                Data.GameTableAdapters.GameLobbyPlayersTableAdapter adapter = new Data.GameTableAdapters.GameLobbyPlayersTableAdapter();
                Data.Game.GameLobbyPlayersDataTable dtGameLobbyPlayers = adapter.GetGameLobbyPlayersByConnectionId(connectionId);

                if (dtGameLobbyPlayers != null && dtGameLobbyPlayers.Rows.Count > 0)
                {
                    Data.Game.GameLobbyPlayersRow row = (Data.Game.GameLobbyPlayersRow)dtGameLobbyPlayers.Rows[0];

                    glp.ConnectionId = row.ConnectionId;
                    glp.DateCreated = row.DateCreated;
                    glp.DateLastActive = row.DateLastActive;
                    glp.GameLobbyId = row.GameLobbyId;
                    glp.GameLobbyPlayersId = row.GameLobbyPlayersId;
                    glp.PlayerId = row.PlayerId;

                    switch (row.ConnectionState)
                    {
                        case "DISCONNECTED":
                            glp.ConnectionState = ConnectionState.DISCONNECTED;
                            break;
                        case "CONNECTED":
                            glp.ConnectionState = ConnectionState.CONNECTED;
                            break;
                        case "INACTIVE":
                            glp.ConnectionState = ConnectionState.INACTIVE;
                            break;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return glp;
        }

        public GameLobbyPlayers GetGameLobbyPlayersByGameLobbyIdAndPlayerId(int gameLobbyId, int playerId)
        {
            GameLobbyPlayers glp = new GameLobbyPlayers();

            try
            {
                Data.GameTableAdapters.GameLobbyPlayersTableAdapter adapter = new Data.GameTableAdapters.GameLobbyPlayersTableAdapter();
                Data.Game.GameLobbyPlayersDataTable dtGameLobbyPlayers = adapter.GetGameLobbyPlayersByGameLobbyIdAndPlayerId(gameLobbyId, playerId);

                if (dtGameLobbyPlayers != null && dtGameLobbyPlayers.Rows.Count > 0)
                {
                    Data.Game.GameLobbyPlayersRow row = (Data.Game.GameLobbyPlayersRow)dtGameLobbyPlayers.Rows[0];

                    glp.ConnectionId = row.ConnectionId;
                    glp.DateCreated = row.DateCreated;
                    glp.DateLastActive = row.DateLastActive;
                    glp.GameLobbyId = row.GameLobbyId;
                    glp.GameLobbyPlayersId = row.GameLobbyPlayersId;
                    glp.PlayerId = row.PlayerId;

                    switch (row.ConnectionState)
                    {
                        case "DISCONNECTED":
                            glp.ConnectionState = ConnectionState.DISCONNECTED;
                            break;
                        case "CONNECTED":
                            glp.ConnectionState = ConnectionState.CONNECTED;
                            break;
                        case "INACTIVE":
                            glp.ConnectionState = ConnectionState.INACTIVE;
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return glp;
        }

        public GamePlayers GetGamePlayersByConnectionId(string connectionId)
        {
            GamePlayers gp = new GamePlayers();

            try
            {
                Data.GameTableAdapters.GamePlayersTableAdapter adapter = new Data.GameTableAdapters.GamePlayersTableAdapter();
                Data.Game.GamePlayersDataTable dtGamePlayers = adapter.GetGamePlayersByConnectionId(connectionId);

                if (dtGamePlayers != null && dtGamePlayers.Rows.Count > 0)
                {
                    Data.Game.GamePlayersRow row = (Data.Game.GamePlayersRow)dtGamePlayers.Rows[0];

                    gp.ConnectionId = row.ConnectionId;
                    gp.DateLastActive = row.DateLastActive;
                    gp.GameId = row.GameId;
                    gp.GamePlayersId = row.GamePlayersId;
                    gp.PlayerId = row.PlayerId;

                    switch (row.ConnectionState)
                    {
                        case "DISCONNECTED":
                            gp.ConnectionState = ConnectionState.DISCONNECTED;
                            break;
                        case "CONNECTED":
                            gp.ConnectionState = ConnectionState.CONNECTED;
                            break;
                        case "INACTIVE":
                            gp.ConnectionState = ConnectionState.INACTIVE;
                            break;
                    }
                }


            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return gp;
        }

        public GamePlayers GetGamePlayersByGameIdAndPlayerId(int gameId, int playerId)
        {
            GamePlayers gp = new GamePlayers();

            try
            {
                Data.GameTableAdapters.GamePlayersTableAdapter adapter = new Data.GameTableAdapters.GamePlayersTableAdapter();
                Data.Game.GamePlayersDataTable dtGamePlayers = adapter.GetGamePlayersByGameIdAndPlayerId(gameId, playerId);

                if (dtGamePlayers != null && dtGamePlayers.Rows.Count > 0)
                {
                    Data.Game.GamePlayersRow row = (Data.Game.GamePlayersRow)dtGamePlayers.Rows[0];

                    gp.ConnectionId = row.ConnectionId;
                    gp.DateLastActive = row.DateLastActive;
                    gp.GameId = row.GameId;
                    gp.GamePlayersId = row.GamePlayersId;
                    gp.PlayerId = row.PlayerId;

                    switch (row.ConnectionState)
                    {
                        case "DISCONNECTED":
                            gp.ConnectionState = ConnectionState.DISCONNECTED;
                            break;
                        case "CONNECTED":
                            gp.ConnectionState = ConnectionState.CONNECTED;
                            break;
                        case "INACTIVE":
                            gp.ConnectionState = ConnectionState.INACTIVE;
                            break;
                    }
                }


            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return gp;
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

                        if(!row.IsNumPlayersInLobbyNull())
                            gameLobby.NumPlayersInLobby = row.NumPlayersInLobby;

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


        public void DeletePlayerFromGameLobby(int playerId, int gameLobbyId, string connectionId)
        {
            try
            {
                Data.GameTableAdapters.GameLobbyPlayersTableAdapter adapter = new Data.GameTableAdapters.GameLobbyPlayersTableAdapter();
                adapter.DeletePlayerFromGameLobby(playerId, gameLobbyId, connectionId);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        public GameLobbyPlayers UpdateGameLobbyPlayers(int gameLobbyId, int playerId, string connectionId, ConnectionState state)
        {
            GameLobbyPlayers lobbyPlayers = new GameLobbyPlayers();

            try
            {
                Data.GameTableAdapters.GameLobbyPlayersTableAdapter adapter = new Data.GameTableAdapters.GameLobbyPlayersTableAdapter();
                Data.Game.GameLobbyPlayersDataTable lobbyPlayersDt = adapter.UpdateGameLobbyPlayers(playerId, gameLobbyId, connectionId, state.ToString());

                if (lobbyPlayersDt != null && lobbyPlayersDt.Rows.Count > 0)
                {
                    Data.Game.GameLobbyPlayersRow row = (Data.Game.GameLobbyPlayersRow)lobbyPlayersDt.Rows[0];

                    lobbyPlayers.ConnectionId = row.ConnectionId;
                    lobbyPlayers.DateCreated = row.DateCreated;
                    lobbyPlayers.GameLobbyId = row.GameLobbyId;
                    lobbyPlayers.GameLobbyPlayersId = row.GameLobbyPlayersId;
                    lobbyPlayers.PlayerId = row.PlayerId;

                    switch (row.ConnectionState)
                    {
                        case "CONNECTED":
                            lobbyPlayers.ConnectionState = ConnectionState.CONNECTED;
                            break;
                        case "DISCONNECTED":
                            lobbyPlayers.ConnectionState = ConnectionState.DISCONNECTED;
                            break;
                        case "INACTIVE":
                            lobbyPlayers.ConnectionState = ConnectionState.INACTIVE;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return lobbyPlayers;
        }

        public GamePlayers UpdateGamePlayers(int gameId, int playerId, string connectionId, ConnectionState state)
        {
            GamePlayers gamePlayers = new GamePlayers();

            try
            {
                Data.GameTableAdapters.GamePlayersTableAdapter adapter = new Data.GameTableAdapters.GamePlayersTableAdapter();
                Data.Game.GamePlayersDataTable dtGamePlayers = adapter.UpdateGamePlayers(gameId, playerId, connectionId, state.ToString());

                if (dtGamePlayers != null && dtGamePlayers.Rows.Count > 0)
                {
                    Data.Game.GamePlayersRow row = (Data.Game.GamePlayersRow)dtGamePlayers.Rows[0];

                    gamePlayers.GameId = row.GameId;
                    gamePlayers.PlayerId = row.PlayerId;
                    gamePlayers.DateLastActive = row.DateLastActive;
                    gamePlayers.ConnectionId = row.ConnectionId;
                    
                    switch(row.ConnectionState) {
                        case "CONNECTED":
                            gamePlayers.ConnectionState = ConnectionState.CONNECTED;
                            break;
                        case "DISCONNECTED":
                            gamePlayers.ConnectionState = ConnectionState.DISCONNECTED;
                            break;
                        case "INACTIVE":
                            gamePlayers.ConnectionState = ConnectionState.INACTIVE;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return gamePlayers;
        }

        
    }
}
