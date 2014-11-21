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
    public partial class HostGame : System.Web.UI.Page
    {
        public Session UserSession = null;
        public Player PlayerData = null;

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
            // get user session
            UserSession = Functions.GetSessionFromCookie();
        }

        protected void btnHostGame_Click(object sender, EventArgs e)
        {
            // service
            WizardService wizWS = new WizardService();

            // player list
            Player[] playerList = wizWS.ListPlayersByUserId(UserSession.UserId);

            // validate
            if (playerList != null && playerList.Length > 0)
            {
                // get first player in list
                PlayerData = playerList[0];

                // get post vars
                string strGameName = txtGameName.Value;
                int maxPlayers = Convert.ToInt32(selectMaxPlayers.Value);
                string strPassword = txtPassword.Value;

                // create new game lobby
                GameLobby gameLobby = wizWS.UpdateGameLobby(0, PlayerData.PlayerId, strGameName, maxPlayers, "", strPassword, false);

                // update session -- attach playerId
                UserSession = wizWS.UpdateSession(UserSession.Secret, UserSession.UserId, PlayerData.PlayerId, UserSession.ConnectionId);

                // validate
                if (gameLobby != null && gameLobby.GameLobbyId > 0)
                {
                    Response.Redirect("~/GameLobbyRoom.aspx?gameLobbyId=" + gameLobby.GameLobbyId);
                }
                else
                {
                    MessageBox.Visible = true;
                    MessageBoxText.InnerHtml = "<strong>Error</strong>: Game lobby could not be created";
                }
            }
        }
    }
}