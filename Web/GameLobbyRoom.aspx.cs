using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WizardGame.WizardService;

namespace WizardGame
{
    public partial class GameLobbyRoom : System.Web.UI.Page
    {
        public Session UserSession = null;
        public Player PlayerData = null;
        public User UserData = null;
        public GameLobby GameLobby = null;
        public bool IsGameHost = false;
        public GameLobbyPlayers[] LobbyPlayers = null;

        private WizardServiceClient wizWS = new WizardServiceClient();
        private int gameLobbyId = 0;

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
            // get session data
            UserSession = Helpers.Functions.GetSessionFromCookie();

            // get user data
            UserData = wizWS.GetUserById(UserSession.UserId);

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
                LobbyPlayers = wizWS.ListGameLobbyPlayers(GameLobby.GameLobbyId);
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

            for (int i = 0; i < LobbyPlayers.Length; i++)
            {
                GameLobbyPlayers lobbyPlayer = LobbyPlayers[i];
                Player player = wizWS.GetPlayerById(lobbyPlayer.PlayerId);

                string playerName = (player != null) ? player.Name : "Error";

                html.AppendLine("<li class='list-group-item' id='player-" + playerName + "'>" + playerName + "</li>");
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