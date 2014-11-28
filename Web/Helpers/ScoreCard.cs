using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WizardGame.Helpers
{
    public class ScoreCard
    {
        List<PlayerScore> playerScores = null;

        public ScoreCard()
        {
            playerScores = new List<PlayerScore>();
        }

        public void AddPlayerScore(int playerId = 0, int round = 0, int bid = 0, int tricks = 0)
        {
            // get score from bid/tricks
            int score = CalculateScore(bid, tricks);

            // add score to list
            playerScores.Add(new PlayerScore
            {
                PlayerId = playerId,
                Round = round,
                Bid = bid,
                Tricks = tricks,
                Score = score
            });
        }

        public void UpdatePlayerScore(int playerId = 0, int round = 0, int bid = 0, int tricks = 0)
        {
            // get score record
            PlayerScore ps = playerScores.Where(p => 
                p.PlayerId == playerId && p.Round == round
            ).FirstOrDefault();

            // validate
            if (ps != null && ps.PlayerId > 0)
            {
                // update score values
                ps.Bid = bid;
                ps.Tricks = tricks;
                ps.Score = CalculateScore(bid, tricks);
            }   
        }

        public int CalculateScore(int bid, int tricks)
        {
            int bidDifference = Math.Abs(bid - tricks);
            int score = 0;

            // calculate score
            if (bidDifference == 0)
            {
                // base score
                score = 20;

                // bonus
                score = score + (tricks * 10);
            }
            else
            {
                score = -(bidDifference * 10);
            }

            return score;
        }

        public PlayerScore[] PlayerScores()
        {
            return playerScores.ToArray();
        }
    }

    public class PlayerScore
    {
        public int PlayerId = 0;
        public int Round = 0;
        public int Bid = 0;
        public int Tricks = 0;
        public int Score = 0;
    }
}