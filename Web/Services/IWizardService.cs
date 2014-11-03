using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using Wizard.Helpers;

namespace Wizard.Services
{
    [ServiceContract]
    public interface IWizardService
    {
        [OperationContract]
        Card[] GenerateDeck();

        [OperationContract]
        Player UpdatePlayer(int playerId, string name, string pictureUrl);

        [OperationContract]
        Game UpdateGame(int gameId, int ownerPlayerId, DateTime? dateCompleted, int numPlayers, int maxHands, int intialDealerPosition, string scoreData);
    }
}
