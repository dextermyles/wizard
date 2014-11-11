using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wizard.Helpers
{
    public class Deck
    {
        private Card[] _cards = null;

        public Deck()
        {
            // generate deck
            _cards = GenerateCards();

            // shuffle 
            ShuffleDeck();
        }

        public Deck(Card[] existingCards)
        {
            // generate cards if none provided
            if (existingCards == null)
            {
                _cards = GenerateCards();

                // shuffle 
                ShuffleDeck();
            } 
        }

        public void ShuffleDeck()
        {
            // generate deck if none
            if (_cards == null)
                _cards = GenerateCards();

            // shuffle cards
            Card[] cards = _cards; // copy of cards
            Random r = new Random();

            for (int n = 0; n < _cards.Length - 1; n++)
            {
                int k = r.Next(n + 1);
                Card temp = cards[n];
                cards[n] = cards[k];
                cards[k] = temp;
            }

            // update new copy of shuffled cards
            _cards = cards;
        }

        public Card[] GenerateCards()
        {
            // 60 cards
            Card[] cards = new Card[60];
            int i = 0; // starting card index
            int y = 2; // starting card value

            // add 13 Spades to deck
            for (i = 0; i < 13; i++)
            {
                cards[i] = new Card
                {
                    Suit = Suit.Spades,
                    Value = y
                };

                y++;
            }

            // reset card value
            y = 2;

            // add 13 Hearts to deck
            for (i = 13; i < 26; i++)
            {
                // add  cards
                cards[i] = new Card
                {
                    Suit = Suit.Hearts,
                    Value = y
                };

                y++;
            }

            // reset card value
            y = 2;

            // add 13 Diamonds to deck
            for (i = 26; i < 39; i++)
            {
                cards[i] = new Card
                {
                    Suit = Suit.Diamonds,
                    Value = y
                };

                y++;
            }

            // reset card value
            y = 2;

            // add 13 Clubs to deck
            for (i = 39; i < 52; i++)
            {
                cards[i] = new Card
                {
                    Suit = Suit.Clubs,
                    Value = y
                };

                y++;
            }

            // reset card value
            y = 2;

            // add 4 Fluffs to deck
            for (i = 52; i < 56; i++)
            {
                cards[i] = new Card
                {
                    Suit = Suit.Fluff,
                    Value = 0
                };
            }

            // add 4 Wizards to deck
            for (i = 56; i < 60; i++)
            {
                cards[i] = new Card
                {
                    Suit = Suit.Wizard,
                    Value = 0
                };
            }

            // return cards
            return cards;
        }

        public Card TakeTopCard()
        {
            int index = _cards.Length - 1;

            if (index < 0)
                index = 0;

            Card selectedCard = _cards[index];

            // make sure card not null
            if (selectedCard != null)
            {
                // new temp deck
                Card[] newDeck = new Card[_cards.Length - 1];

                // copy cards except last one to new deck
                Array.Copy(_cards, 0, newDeck, 0, index);

                // update cards with new deck
                _cards = newDeck;
            }

            return selectedCard;
        }

        public Card[] Cards() {
            return _cards; 
        }

        public void SetDeck(Card[] cards)
        {
            if (cards != null && cards.Length > 0)
            {
                _cards = cards;
            }
        }
    }

    public class Card
    {
        public Suit Suit = 0;
        public int Value = 0; // 11=JACK,12=QUEEN,13=KING,14=ACE
        public int OwnerPlayerId = 0;

        private string image = string.Empty;

        public string GetImagePath()
        {
            string appRootPath = AppDomain.CurrentDomain.BaseDirectory;
            string filename = string.Empty;

            if(Suit == Suit.Fluff)
            {
                filename = "fluff.png";
            }
            else if(Suit == Suit.Wizard)
            {
                filename = "wizard.png";
            }
            else
            {
                filename = Suit.ToString() + "_" + Value.ToString() + ".png";
            }
            
            string fullPath = appRootPath +  "\\Assets\\Cards\\" + filename;

            if(System.IO.File.Exists(fullPath)) {
                return fullPath;
            }

            return string.Empty;
        }

        public override string ToString()
        {
            string s = string.Empty;

            switch (Value)
            {
                case 11:
                    s = "Jack";
                    break;
                case 12:
                    s = "Queen";
                    break;
                case 13:
                    s = "King";
                    break;
                case 14:
                    s = "Ace";
                    break;
                default:
                    s = Value.ToString();
                    break;
            }

            if (Suit == Helpers.Suit.Wizard || Suit == Helpers.Suit.Fluff)
            {
                return Suit.ToString();
            }
            else
            {
                return s + " of " + Suit.ToString();
            }  
        }
    }

    public enum Suit
    {
        Spades = 0,
        Hearts = 1,
        Clubs = 2,
        Diamonds = 3,
        Fluff = 4,
        Wizard = 5
    }
}