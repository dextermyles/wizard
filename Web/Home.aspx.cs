using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WizardGame.Helpers;
using WizardGame.Services;

namespace WizardGame
{
    public partial class Home : System.Web.UI.Page
    {
        public Session UserSession = null;
        public Player[] UserPlayers = null;
        public User UserData = null;

        private WizardService wizWS = new WizardService();

        protected override void OnLoad(EventArgs e)
        {
            // validate function
            if (!Functions.IsValidSession())
            {
                // redirect to login page
                Response.Redirect("~/Default.aspx");
            }

            base.OnLoad(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // get user session info
            UserSession = Functions.GetSessionFromCookie();

            // get list of players for current user
            UserPlayers = wizWS.ListPlayersByUserId(UserSession.UserId);

            // get user data
            UserData = wizWS.GetUserById(UserSession.UserId);

            // hide facebook profile photo option
            if (string.IsNullOrEmpty(UserData.FB_UserId))
            {
                UseFacebookProfilePhoto.Visible = false;
            }

            // update page details
            UpdatePageDetails();
        }

        private void UpdatePageDetails()
        {
            // update welcome text with player name
            if (UserData != null)
            {
                // show username
                if (!string.IsNullOrEmpty(UserData.Username))
                {
                    WelcomeTitle.InnerText = "Welcome, " + UserData.Username + "!";
                }
                else
                {
                    // check player list for a player name
                    if (UserPlayers != null && UserPlayers.Length > 0)
                    {
                        // update welcome title
                        WelcomeTitle.InnerText = "Welcome, " + UserPlayers[0].Name + "!";

                        // by default assign first player to session (will later be done via character select screen)
                        wizWS.UpdateSession(UserSession.Secret, UserSession.UserId, UserPlayers[0].PlayerId, UserSession.ConnectionId);
                    }
                }
            }
        }

        protected void btnNewPlayer_Click(object sender, EventArgs e)
        {
            // new player
            Player player = wizWS.UpdatePlayer(0, PlayerName.Text, PlayerPhoto.FileName, UserSession.UserId);

            // validate
            if(player != null && player.PlayerId > 0) 
            {
                // update session
                UserSession = wizWS.UpdateSession(UserSession.Secret, UserSession.UserId, player.PlayerId, UserSession.ConnectionId);

                if (UserSession != null)
                {
                    // update list of players for user
                    UserPlayers = wizWS.ListPlayersByUserId(UserSession.UserId);
                }
            }
        }
    }
}