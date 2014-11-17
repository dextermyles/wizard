using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WizardGame.Helpers
{
    public class ScoreData
    {
        PlayerScore[] playerScores = null;

        public void AddPlayer(int playerId)
        {
            playerScores.ToList<PlayerScore>().Add(
                new PlayerScore { 
                    PlayerId = playerId 
                }
            );
        }

        public void UpdatePlayerScore(int playerId, int score)
        {
            PlayerScore ps = playerScores.Where(p => p.PlayerId == playerId).FirstOrDefault();

            if(ps != null)
                ps.Score = score;
        }
    }

    public class PlayerScore
    {
        public int PlayerId = 0;
        public int Score = 0;
    }
}