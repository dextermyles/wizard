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
        public ScoreCard scoreCard = null;

        public GameState()
        {
            // new score card
            scoreCard = new ScoreCard();

            // generate new deck
            Deck = new Deck();
        }

        public void ClearAllBids()
        {
            if (Players != null && Players.Length > 0)
            {
                for (int i = 0; i < Players.Length; i++)
                {
                    Players[i].Bid = 0;
                }
            }
        }

        public void EnterBid(int playerId, int bid)
        {
            // get player object
            Player player = Players.Where(p => p.PlayerId == playerId).FirstOrDefault();

            // validate
            if (player != null && player.PlayerId > 0)
            {
                // set bid
                player.Bid = bid;

                // set flag
                player.IsTurn = false;

                // check if last player to bid
                if (PlayerTurnIndex == DealerPositionIndex)
                {
                    // done bidding, start round
                    Status = GameStateStatus.RoundInProgress;
                }

                // update player turn index
                PlayerTurnIndex++;

                if (PlayerTurnIndex > Players.Length - 1)
                    PlayerTurnIndex = 0;

                // update next player turn flag
                Players[PlayerTurnIndex].IsTurn = true;
            }  
        }

        public void StartNextRound()
        {
            // validation
            if (Players == null)
                return;

            // max rounds
            int maxRounds = (60 / Players.Length);

            // increment rounds
            Round++;

            if (Round > maxRounds)
                Round = maxRounds;

            // next dealer
            DealerPositionIndex++;

            if (DealerPositionIndex > Players.Length - 1)
                DealerPositionIndex = 0;

            // set flag
            Players[DealerPositionIndex].IsDealer = true;

            // next player turn
            PlayerTurnIndex = DealerPositionIndex + 1;

            if (PlayerTurnIndex > Players.Length - 1)
                PlayerTurnIndex = 0;

            // set flag
            Players[PlayerTurnIndex].IsTurn = true;

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
                Players[currentIndex].ReceiveCard(topCard);

                // increment index
                currentIndex++;

                // back to 0 when we reach last player
                if (currentIndex > Players.Length - 1)
                    currentIndex = 0;
            }

            // set game status
            Status = GameStateStatus.BiddingInProgress;
        }

        public void StartGame(Player[] _players)
        {
            // validate players
            if(_players == null || _players.Length < 3)
                throw new Exception("3 players minimum required to play");

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
                Players[currentIndex].ReceiveCard(topCard);

                // increment index
                currentIndex++;

                // back to 0 when we reach last player
                if (currentIndex > Players.Length - 1)
                    currentIndex = 0;
            }

            // set game status
            Status = GameStateStatus.BiddingInProgress;
        }

        public void PlayCard(Card _card)
        {
            // get list of cards played
            List<Card> cardsPlayedList = (CardsPlayed == null) ? 
                new List<Card>() : CardsPlayed.ToList();

            // add card to played list
            cardsPlayedList.Add(_card);

            // replace cards played array
            CardsPlayed = cardsPlayedList.ToArray();
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