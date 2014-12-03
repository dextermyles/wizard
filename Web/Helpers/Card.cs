using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WizardGame.Helpers
{
    public class Card
    {
        public string Id = string.Empty; // card id
        public Suit Suit = 0;
        public int Value = 0; // 11=JACK,12=QUEEN,13=KING,14=ACE
        public int OwnerPlayerId = 0;

        private string image = string.Empty;

        public Card()
        {
            Id = Guid.NewGuid().ToString();
        }

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
        None = -1,
        Spades = 0,
        Hearts = 1,
        Clubs = 2,
        Diamonds = 3,
        Fluff = 4,
        Wizard = 5
    }
}