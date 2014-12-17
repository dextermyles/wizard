using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WizardGame.Services;
using WizardGame.Helpers;

namespace WizardGame
{
    public partial class Leaderboard : System.Web.UI.Page
    {
        public Session UserSession = null;

        // service
        WizardService wizWS = new WizardService();

        // handle session validation
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
        }

        public string LeaderboardHtml()
        {
            System.Text.StringBuilder html = new System.Text.StringBuilder();

            // get leaderboard players
            Player[] leaderboardPlayers = wizWS.ListLeaderboardPlayers();

            if (leaderboardPlayers != null && leaderboardPlayers.Length > 0)
            {
                foreach (Player player in leaderboardPlayers)
                {
                    string imgHtml = "";

                    if (!string.IsNullOrEmpty(player.PictureURL))
                    {
                        imgHtml = "<img src='" + player.PictureURL + "' class='img-thumbnail' style='width:64px; height: 64px;' /> ";
                    }
                    else
                    {
                        imgHtml = "<img data-src='holder.js/64x64' class='img-thumbnail' /> ";
                    }

                    html.AppendLine("<tr>");
                    html.AppendLine("<td style='vertical-align: middle'>" + imgHtml + player.Name + "</td>");
                    html.AppendLine("<td class='text-center' style='font-size: 16px; font-weight: bold'>" + player.NumWins + "</td>");
                    html.AppendLine("</tr>");
                }
            }
            else
            {
                html.Append("<tr><td colspan='2'>No matches have been recorded</td></tr>");
            }

            return html.ToString();
        }
    }
}