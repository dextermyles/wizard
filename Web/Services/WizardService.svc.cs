using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Wizard.Helpers;
using Wizard.Services;

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


        public Player UpdatePlayer(int playerId, string name, string pictureUrl)
        {
            try
            {
                // get db results
                Data.PlayerTableAdapters.PlayerTableAdapter playerApapter = new Data.PlayerTableAdapters.PlayerTableAdapter();
                Data.Player.PlayerDataTable dtPlayer = playerApapter.UpdatePlayer(playerId, name, pictureUrl);
                Data.Player.PlayerRow row = (Data.Player.PlayerRow) dtPlayer.Rows[0];

                if (row != null)
                {
                    // new player
                    Player player = new Player();

                    // update object with db results
                    player.PlayerId = row.PlayerId;
                    player.Name = row.Name;
                    player.PictureURL = row.PictureURL;

                    return player;
                }
            }
            catch (Exception)
            {
                // error handling
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
            catch (Exception)
            {
                // error handling
            }

            return null;
        }
    }
}
