using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WizardGame.Helpers
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

        public Card[] Cards()
        {
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
}