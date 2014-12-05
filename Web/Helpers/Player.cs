using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace WizardGame.Helpers
{
    public class Player
    {
        public int PlayerId = 0;
        public string Name = string.Empty;
        public string PictureURL = string.Empty;
        public int UserId = 0;
        public int Score = 0;
        public int Bid = 0;
        public int TricksTaken = 0;
        public bool IsDealer = false;
        public bool IsTurn = false;
        public bool IsLastToAct = false;
        public bool Won = false;
        public Card[] Cards = null;
        public Card LastCardPlayed = null;
        public string ConnectionId = string.Empty;
        public ConnectionState ConnectionState = ConnectionState.DISCONNECTED;

        public bool HasCards()
        {
            // player has cards
            if (Cards != null && Cards.Length > 0)
                return true;

            // player has no cards
            return false;
        }

        public void ReceiveCard(Card _card)
        {
            // get list
            List<Card> cardList = (Cards == null) ? 
                new List<Card>() : Cards.ToList();

            // update owner id
            _card.OwnerPlayerId = PlayerId;

            // add card to list
            cardList.Add(_card);

            // update cards and sort by suit
            Cards = cardList.OrderBy(c => c.Suit).ThenBy(c => c.Value).ToArray();

            // clear list
            cardList.Clear();
            cardList = null;
        }

        public Card PlayCard(Card _card)
        {
            // validation
            if (Cards == null)
                return null;

            // card list
            List<Card> cardList = Cards.ToList();

            // loop through cards
            for (int i = 0; i < Cards.Length; i++)
            {
                // card ref
                Card card = Cards[i];

                // card exists in players hand
                if (card.Id == _card.Id)
                {
                    // update last used card
                    LastCardPlayed = card;

                    // remove card from players hand
                    cardList.Remove(card);

                    break;
                }    
            }

            // get array from list
            Cards = cardList.ToArray();

            // clear list
            cardList.Clear();
            cardList = null;

            // return played card
            return LastCardPlayed;
        }
    }
}