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
        public bool IsBidRequired = false;
        public Card[] Cards = null;
        public Card LastCardPlayed = null;

        public void SetBid(int points)
        {
            IsBidRequired = false;
            Bid = points;
        }

        public void GiveCard(Card _card)
        {
            // get list
            List<Card> cardList = (Cards == null) ? 
                new List<Card>() : Cards.ToList();

            // assign player id
            _card.OwnerPlayerId = PlayerId;

            // add card to list
            cardList.Add(_card);

            // update cards
            Cards = cardList.ToArray();

            // clear list
            cardList = null;
        }

        public void UseCard(Card _card)
        {
            // validation
            if (Cards == null)
                return;

            // get list
            List<Card> cardList = Cards.ToList();

            // remove card
            cardList.Remove(_card);

            // update cards
            Cards = cardList.ToArray();

            // clear list
            cardList = null;
        }
    }
}