using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using WizardGame.Helpers;

namespace WizardGame.Services
{
    [ServiceContract]
    public interface IWizardService
    {
        // Business Logic
        [OperationContract]
        Card[] GenerateDeck();

        [OperationContract]
        bool EmailExists(string emailAddress);

        [OperationContract]
        bool UsernameExists(string username);

        [OperationContract]
        Session Login(string username, string password, string ipAddress);

        [OperationContract]
        Session FacebookLogin(string fb_email, string fb_userId);

        [OperationContract]
        NewUserResult NewUser(string username, string password, string emailAddress, string ipAddress, bool active = true);

        void LogError(Exception ex);

        [OperationContract]
        Session ValidateSession(string secret, string ipAddress);

        // Gets
        [OperationContract]
        Game GetGameById(int gameId);

        [OperationContract]
        GameHistory GetGameHistoryByGameId(int gameId);

        [OperationContract]
        GameHistory GetGameHistoryById(int gameHistoryId);

        [OperationContract]
        HandHistory GetHandHistoryById(int handHistoryId);

        [OperationContract]
        HandHistory GetLastHandHistoryByGameId(int gameId);

        [OperationContract]
        Player GetPlayerById(int playerId);

        [OperationContract]
        Player GetPlayerByName(string name);

        [OperationContract]
        User GetUserById(int userId);

        [OperationContract]
        User GetUserByUsername(string username);

        // Lists
        Player[] ListPlayersByUserId(int userId);

        // Updates
        [OperationContract]
        Game UpdateGame(int gameId, int ownerPlayerId, DateTime? dateCompleted, int numPlayers, int maxHands, int intialDealerPosition, string scoreData);

        [OperationContract]
        GameHistory UpdateGameHistory(int gameHistoryId, int gameId, int playerId, int score, int won);

        [OperationContract]
        HandHistory UpdateHandHistory(int handHistoryId, int gameId, string deckData, string playerData, string trump);

        [OperationContract]
        Player UpdatePlayer(int playerId, string name, string pictureUrl, int userId);

        [OperationContract]
        Session UpdateSession(string secret, int userId, int playerId, string ipAddress);

        [OperationContract]
        User UpdateUser(int userId, string username, string password, string emailAddress, bool active, string fb_userId);
    }
}
