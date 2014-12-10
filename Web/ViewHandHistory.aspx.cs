using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WizardGame.Services;
using WizardGame.Helpers;
using System.Text;

namespace WizardGame
{
    public partial class ViewHandHistory : System.Web.UI.Page
    {
        public Session UserSession = null;
        public Player[] UserPlayers = null;
        public User UserData = null;
        public Game GameData = null;
        public int GameId = 0;

        // service
        WizardService wizWS = new WizardService();

        protected override void OnLoad(EventArgs e)
        {
            // is valid session
            bool isValidSession = Functions.IsValidSession();

            if (!isValidSession)
            {
                // redirect to login page
                Response.Redirect("~/Default.aspx?Error=Session is not valid");
            }

            base.OnLoad(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // get user session data
            UserSession = Functions.GetSessionFromCookie();

            // get game id from query string
            string strGameId = Request.QueryString["GameId"];

            // game id exists
            if (!string.IsNullOrEmpty(strGameId))
            {
                // parse game id
                int.TryParse(strGameId, out GameId);
            }

            // game exists
            if (GameId > 0)
            {
                GameData = wizWS.GetGameById(GameId);
            }
        }

        public string HandHistoryHtml()
        {
            StringBuilder html = new StringBuilder();

            // game id is set
            if (GameId > 0)
            {
                // handhistory array
                HandHistory[] handHistory = wizWS.GetHandHistoryByGameId(GameId);
                Player[] gamePlayers = wizWS.ListPlayersByGameId(GameId);

                // hand history exists
                if (handHistory != null && handHistory.Length > 0)
                {
                    int handNum = 1;

                    // loop through hand history
                    for (int i = 0; i < handHistory.Length; i++)
                    {
                        // hand reference
                        HandHistory history = handHistory[i];
                        
                        // winning player
                        Player winningPlayer = wizWS.GetPlayerById(history.WinnerPlayerId);

                        // cards played
                        string cardsPlayed = string.Empty;

                        cardsPlayed += "<ul class='pagination'>";

                        // append cards played
                        for(int x = 0; x < history.CardsPlayed.Length; x++) {
                            // card ref
                            Card cardPlayed = history.CardsPlayed[x];

                            // card image
                            string cardImage = cardPlayed.GetImagePath();

                            // replace physical path
                            cardImage = cardImage.Replace(Request.ServerVariables["APPL_PHYSICAL_PATH"], String.Empty);

                            string ownerClass = "";

                            if (cardPlayed.OwnerPlayerId == winningPlayer.PlayerId)
                                ownerClass = "active";

                            cardsPlayed += "<li class='" + ownerClass + "'><a class='card-owner'><div>" + gamePlayers.FirstOrDefault(gp => gp.PlayerId == cardPlayed.OwnerPlayerId).Name + "</div><img src='" + cardImage + "' class='img-thumbnail' style='height: 96px;'/></a></li>";  
                        }

                        cardsPlayed += "</ul>";

                        // trump card image
                        string trumpCardImage = history.TrumpCard.GetImagePath();

                        // replace physical path
                        trumpCardImage = trumpCardImage.Replace(Request.ServerVariables["APPL_PHYSICAL_PATH"], String.Empty);

                        // suit to follow image
                        string suitToFollowImage = "/Assets/Cards/" + history.SuitToFollow.ToString() + "_0.png";

                        // fluff if no suit to follow
                        if (history.SuitToFollow == Suit.None)
                            suitToFollowImage = "/Assets/Cards/fluff.png";

                        html.AppendLine("<tr>");
                        html.AppendLine("<td class=\"text-center\">" + handNum + "</td>");
                        html.AppendLine("<td class=\"text-center\">" + history.Round + "</td>");
                        html.AppendLine("<td class=\"text-center hidden-xs\">" + cardsPlayed + "</td>");
                        html.AppendLine("<td class=\"text-center\"><img src='" + trumpCardImage + "' class='img-thumbnail' style='height: 96px;' /></td>");
                        html.AppendLine("<td class=\"text-center\"><img src='" + suitToFollowImage + "' class='img-thumbnail' style='height: 96px;' /></td>");
                        html.AppendLine("<td class=\"text-center\">" + winningPlayer.Name + "</td>");
                        html.AppendLine("</tr>");

                        // increment hand #
                        handNum++;
                    }
                }
                else
                {
                    html.AppendLine("<tr><td colspan='6'>Sorry! Hand history is not available for this game</tr>");
                }
            }

            return html.ToString();
        }
    }
}