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
    public partial class Play : System.Web.UI.Page
    {
        public Session UserSession = null;
        public Player PlayerData = null;
        public User UserData = null;
        public Game Game = null;
        public GameLobby GameLobby = null;
        public bool IsGameHost = false;
        public Player[] Players = null;

        private WizardService wizWS = new WizardService();
        private int gameId = 0;

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
            // get session data
            UserSession = Functions.GetSessionFromCookie();

            // get user data
            UserData = wizWS.GetUserById(UserSession.UserId);

            // GET vars
            string strGameId = (string)Request.QueryString["gameId"];

            // load game
            if (!string.IsNullOrEmpty(strGameId))
            {
                // parse gameId
                int.TryParse(strGameId, out gameId);

                // get game data
                Game = wizWS.GetGameById(gameId);

                // validation
                if (Game != null && Game.GameId > 0)
                {
                    // get player data
                    Players = Game.GameStateData.Players;

                    // check that player is part of game
                    var playerTest = Players.Where(p=>p.PlayerId == UserSession.PlayerId).FirstOrDefault();
                    bool isPlayerValid = false;

                    // validate
                    if (playerTest.PlayerId > 0)
                        isPlayerValid = true;

                    if (!isPlayerValid)
                    {
                        Response.Redirect("~/Home.aspx?Error=Player does not belong to game");
                    }

                    // get game lobby data
                    GameLobby = wizWS.GetGameLobbyById(Game.GameLobbyId);

                    // get player data
                    PlayerData = wizWS.GetPlayerById(UserSession.PlayerId);
                }
                else
                {
                    Response.Redirect("~/Home.aspx?Error=Invalid game data");
                }
            }
        }
    }
}