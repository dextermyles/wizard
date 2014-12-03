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
            if (Cards == null)
                return false;

            if (Cards.Length > 0)
                return true;

            return false;
        }

        public void ReceiveCard(Card _card)
        {
            // get list
            List<Card> cardList = (Cards == null) ? 
                new List<Card>() : Cards.ToList();

            // assign player id
            _card.OwnerPlayerId = PlayerId;

            // add card to list
            cardList.Add(_card);

            // update cards and sort by suit
            Cards = cardList.OrderBy(c => c.Suit).ThenBy(c => c.Value).ToArray();

            // clear list
            cardList = null;
        }

        public Card PlayCard(Card _card)
        {
            // validation
            if (Cards == null)
                return null; ;

            Card returnCard = null;

            // card list
            List<Card> cardList = Cards.ToList();

            // update last used card
            LastCardPlayed = _card;

            // remove card
            for (int i = 0; i < Cards.Length; i++)
            {
                Card card = Cards[i];

                if (card.Id == _card.Id)
                {
                    returnCard = card;

                    cardList.Remove(card);
                }    
            }

            // get array from list
            Cards = cardList.ToArray();

            // clear list
            cardList = null;

            // return played card
            return returnCard;
        }
    }
}