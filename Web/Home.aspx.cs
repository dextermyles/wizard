using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using WizardGame.WizardService;

namespace WizardGame
{
    public partial class Home : System.Web.UI.Page
    {
        public Session UserSession = null;
        public Player[] UserPlayers = null;
        public User UserData = null;

        protected override void OnLoad(EventArgs e)
        {
            // validate function
            if (!Helpers.Functions.IsValidSession())
            {
                // redirect to login page
                Response.Redirect("~/Default.aspx");
            }

            base.OnLoad(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // service
            WizardServiceClient wizWS = new WizardService.WizardServiceClient();

            // get user session info
            UserSession = Helpers.Functions.GetSessionFromCookie();

            // get list of players for current user
            UserPlayers = wizWS.ListPlayersByUserId(UserSession.UserId);

            // get user data
            UserData = wizWS.GetUserById(UserSession.UserId);

            // close service
            wizWS.Close();

            // hide facebook profile photo option
            if (string.IsNullOrEmpty(UserData.FB_UserId))
            {
                UseFacebookProfilePhoto.Visible = false;
            }

            // update page details
            UpdatePageDetails();

            // update session with default player
            SetDefaultPlayer(); 
        }

        private void SetDefaultPlayer()
        {
            // check player list for a player name
            if (UserPlayers != null && UserPlayers.Length > 0)
            {
                // service
                WizardServiceClient wizWS = new WizardService.WizardServiceClient();

                // by default assign first player to session (will later be done via character select screen)
                wizWS.UpdateSession(UserSession.Secret, UserSession.UserId, UserPlayers[0].PlayerId, UserSession.ConnectionId);

                // close service
                wizWS.Close();
            }
        }

        private void UpdatePageDetails()
        {
            // check player list for a player name
            if (UserPlayers != null && UserPlayers.Length > 0)
            {
                // update welcome title with player name
                WelcomeTitle.InnerText = "Welcome, " + UserPlayers[0].Name + "!";
            }
            else
            {
                // update welcome title with username
                if (UserData != null)
                {
                    // show username
                    if (!string.IsNullOrEmpty(UserData.Username))
                    {
                        WelcomeTitle.InnerText = "Welcome, " + UserData.Username + "!";
                    }
                }
            }
        }

        public string ListGameLobbiesHtml()
        {
            // service
            WizardServiceClient wizWS = new WizardService.WizardServiceClient();

            StringBuilder html = new StringBuilder();
            GameLobby[] gameLobbies = wizWS.ListAllGameLobbies(false);

            if (gameLobbies != null && gameLobbies.Length > 0)
            {
                for (int i = 0; i < gameLobbies.Length; i++)
                {
                    GameLobby gameLobby = gameLobbies[i];
                    Player gameHost = wizWS.GetPlayerById(gameLobby.OwnerPlayerId);

                    string hostName = (gameHost != null) ? gameHost.Name : "Error";

                    html.AppendLine("<tr>");
                    html.AppendLine("<td>" + gameLobby.Name.Trim() + "</td>");
                    html.AppendLine("<td style='text-align:center;'>" + hostName + "</td>");
                    html.AppendLine("<td style='text-align:center;'>" + gameLobby.NumPlayersInLobby + " / " + gameLobby.MaxPlayers + "</td>");
                    html.AppendLine("<td style='text-align:center;'><a href='GameLobbyRoom.aspx?GameLobbyId=" + gameLobby.GameLobbyId + "'>Join</a></td>");
                    html.AppendLine("</tr>");
                }
            }
            else
            {
                html.AppendLine("<tr>");
                html.AppendLine("<td colspan='4'>No game lobbies available</td>");
                html.AppendLine("</tr>");
            }

            // close service
            wizWS.Close();

            return html.ToString();
        }

        protected void btnNewPlayer_Click(object sender, EventArgs e)
        {
            // service
            WizardServiceClient wizWS = new WizardService.WizardServiceClient();

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

            // close service
            wizWS.Close();
        }
    }
}