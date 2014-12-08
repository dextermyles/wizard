using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WizardGame.Services;
using WizardGame.Helpers;

namespace WizardGame
{
    public partial class GameLobbyRoom : System.Web.UI.Page
    {
        public Session UserSession = null;
        public Player PlayerData = null;
        public User UserData = null;
        public GameLobby GameLobby = null;
        public bool IsGameHost = false;
        public Player[] Players = null;

        private WizardService wizWS = new WizardService();
        private int gameLobbyId = 0;

        protected override void OnLoad(EventArgs e)
        {
            // is valid session
            bool isValidSession = Functions.IsValidSession();

            if (!isValidSession)
            {
                // current page
                string currentRequest = Request.RawUrl;

                // set referring page
                Session["referencePage"] = currentRequest;

                // redirect to login page
                Response.Redirect("~/Default.aspx?Error=Session is not valid");
            }

            base.OnLoad(e);
        }

        private void SetDefaultPlayer()
        {
            // players attached to users account
            Player[] UserPlayers = wizWS.ListPlayersByUserId(UserSession.UserId);

            // check player list for a player name
            if (UserPlayers != null && UserPlayers.Length > 0)
            {
                // default player ref
                Player defaultPlayer = UserPlayers[0];

                // by default assign first player to session (will later be done via character select screen)
                UserSession = wizWS.UpdateSession(UserSession.Secret, UserSession.UserId, defaultPlayer.PlayerId, UserSession.ConnectionId);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // get session data
            UserSession = Functions.GetSessionFromCookie();

            // get user data
            UserData = wizWS.GetUserById(UserSession.UserId);

            // set default player to account
            SetDefaultPlayer();
            
            // GET vars
            string strGameLobbyId = (string)Request.QueryString["gameLobbyId"];

            // load game lobby
            if (!string.IsNullOrEmpty(strGameLobbyId))
            {
                int.TryParse(strGameLobbyId, out gameLobbyId);

                GameLobby = wizWS.GetGameLobbyById(gameLobbyId);
            }

            // validate
            if (GameLobby != null && GameLobby.GameLobbyId > 0)
            {
                // update page info
                GameLobbyTitle.InnerText = "Game Lobby: " + GameLobby.Name;

                // get player data
                PlayerData = wizWS.GetPlayerById(UserSession.PlayerId);

                // validate
                if (PlayerData != null && PlayerData.PlayerId > 0)
                {
                    // player is the host
                    if (GameLobby.OwnerPlayerId == PlayerData.PlayerId)
                        IsGameHost = true;
                }
                else
                {
                    // error redirect
                    Response.Redirect("~/Home.aspx?Error=No player assigned to user account");
                    Response.End();
                }

                // get lobby players
                Players = wizWS.ListPlayersByGameLobbyId(GameLobby.GameLobbyId);
            }
            else
            {
                // error redirect
                Response.Redirect("~/Home.aspx?Error=Game lobby not found");
                Response.End();
            }
        }

        public string ListGameLobbyPlayersHtml()
        {
            StringBuilder html = new StringBuilder();

            for (int i = 0; i < Players.Length; i++)
            {
                Player player = Players[i];

                string playerName = (player != null) ? player.Name : "Error";

                html.AppendLine("<li class='list-group-item' id='player-" + player.PlayerId + "'>" + playerName + "</li>");
            }

            return html.ToString();
        }

        protected void btnStartGame_Click(object sender, EventArgs e)
        {

        }

        protected void btnCancelGame_Click(object sender, EventArgs e)
        {

        }

        protected void btnQuitGame_Click(object sender, EventArgs e)
        {

        }
    }
}