using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace WizardGame.Helpers
{
    public class GameState
    {
        public int GameId = 0;
        public int Round = 0;
        public int DealerPositionIndex = 0;
        public int PlayerTurnIndex = 0;
        public Player[] Players = null;
        public Card[] CardsPlayed = null;
        public GameStateStatus Status = GameStateStatus.Setup;
        public Deck Deck = null;

        public void StartGame(Player[] _players)
        {
            // validate players
            if(_players == null || _players.Length < 3)
                throw new Exception("3 players minimum required to play");

            // generate new deck
            Deck = new Deck();

            // update players
            Players = _players;

            // determine random dealer position
            Random random = new Random();

            // assign dealer index
            DealerPositionIndex = random.Next(0, Players.Length - 1);
            
            // set flag
            Players[DealerPositionIndex].IsDealer = true;

            // get player turn index
            PlayerTurnIndex = DealerPositionIndex + 1;

            if (PlayerTurnIndex > Players.Length - 1)
                PlayerTurnIndex = 0;

            // set flag
            Players[PlayerTurnIndex].IsTurn = true;

            // set round #
            Round = 1;

            // set game status
            Status = GameStateStatus.DealInProgress;

            // get number of cards to deal
            int numCardsToDeal = (Round * Players.Length);

            // current index
            int currentIndex = PlayerTurnIndex;

            // deal cards
            for (int i = 0; i < numCardsToDeal; i++)
            {
                // take top card from deck
                Card topCard = Deck.TakeTopCard();

                // give to player
                Players[currentIndex].GiveCard(topCard);

                // increment index
                currentIndex++;

                // back to 0 when we reach last player
                if (currentIndex > Players.Length - 1)
                    currentIndex = 0;
            }

            // set game status
            Status = GameStateStatus.BiddingInProgress;
        }

        public void StartTurn()
        {

        }
    }

    public enum GameStateStatus
    {
        DealInProgress = 0,
        BiddingInProgress = 1,
        RoundInProgress = 2,
        Setup = 3,
        Finished = 4
    }
}