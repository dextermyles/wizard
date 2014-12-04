using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WizardGame.Helpers;
using WizardGame.Services;
using CloudinaryDotNet;

namespace WizardGame
{
    public partial class Home : System.Web.UI.Page
    {
        public Session UserSession = null;
        public Player[] UserPlayers = null;
        public User UserData = null;

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

            // update session with default player
            SetDefaultPlayer(); 
        }

        private void SetDefaultPlayer()
        {
            // check player list for a player name
            if (UserPlayers != null && UserPlayers.Length > 0)
            {
                // by default assign first player to session (will later be done via character select screen)
                wizWS.UpdateSession(UserSession.Secret, UserSession.UserId, UserPlayers[0].PlayerId, UserSession.ConnectionId);
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
            StringBuilder html = new StringBuilder();
            GameLobby[] gameLobbies = wizWS.ListAllGameLobbies(false);

            if (gameLobbies != null)
            {
                // loop through game lobbies
                for (int i = 0; i < gameLobbies.Length; i++)
                {
                    GameLobby gameLobby = gameLobbies[i];
                    Player gameHost = wizWS.GetPlayerById(gameLobby.OwnerPlayerId);

                    string hostName = (gameHost != null) ? gameHost.Name : "Error";

                    html.AppendLine("<tr>");
                    html.AppendLine("<td>" + gameLobby.Name.Trim() + "</td>");
                    html.AppendLine("<td style='text-align:center;'>" + hostName + "</td>");
                    html.AppendLine("<td style='text-align:center;'>" + gameLobby.NumPlayersInLobby + " / " + gameLobby.MaxPlayers + "</td>");

                    // show join link if player record exists
                    if (UserPlayers.Length > 0)
                    {
                        html.AppendLine("<td style='text-align:center;'><a href='GameLobbyRoom.aspx?GameLobbyId=" + gameLobby.GameLobbyId + "' class='btn btn-sm btn-success'>Join</a></td>");
                    }
                    else
                    {
                        html.AppendLine("<td style='text-align:center;'><a class='btn btn-sm btn-danger' onclick='alert(\"You must create a player before joining a game!\");'>Join</a></td>");
                    }
                    
                    html.AppendLine("</tr>");
                }
            }
            else
            {
                html.AppendLine("<tr>");
                html.AppendLine("<td colspan='4'>No game lobbies available</td>");
                html.AppendLine("</tr>");
            }

            return html.ToString();
        }

        protected void btnNewPlayer_Click(object sender, EventArgs e)
        {
            // photo file name
            string photoUrl = string.Empty;

            // use facebook photo
            string strTxtFacebookPhotoUrl = txtFacebookPhotoUrl.Value;
            bool useFacebookPhoto = cbUseFacebookPhoto.Checked;

            if (useFacebookPhoto && !string.IsNullOrEmpty(strTxtFacebookPhotoUrl))
            {
                photoUrl = strTxtFacebookPhotoUrl;
            }
            // use uploaded photo
            else
            {
                // validate content type
                string[] validContentTypes = { "image/jpeg", "image/gif", "image/bmp", "image/png" };
                bool isValidFile = false;

                foreach (string contentType in validContentTypes)
                {
                    if (contentType.Contains(PlayerPhoto.PostedFile.ContentType))
                    {
                        // valid content type
                        isValidFile = true;
                        
                        // break
                        break;
                    }
                }

                // valid upload image
                if (isValidFile)
                {
                    // get upload paths
                    string uploadPath = Server.MapPath("~/Uploads/");
                    string fileName = Path.GetFileName(PlayerPhoto.FileName);

                    // get upload bytes
                    var uploadBytes = PlayerPhoto.FileBytes;

                    // convert bytes to stream
                    Stream uploadStream = new MemoryStream(uploadBytes);

                    // send to cloudinary
                    string cloudinaryUrl = ConfigurationManager.AppSettings.Get("CLOUDINARY_URL");

                    Cloudinary cloudinary = new Cloudinary(cloudinaryUrl);

                    CloudinaryDotNet.Actions.ImageUploadParams uploadParams = new CloudinaryDotNet.Actions.ImageUploadParams()
                    {
                        File = new CloudinaryDotNet.Actions.FileDescription(fileName, uploadStream)
                    };

                    // upload results
                    CloudinaryDotNet.Actions.ImageUploadResult uploadResult = cloudinary.Upload(uploadParams);

                    string image_url = cloudinary.Api.UrlImgUp.BuildUrl(String.Format("{0}.{1}", uploadResult.PublicId, uploadResult.Format));

                    if (!string.IsNullOrEmpty(image_url))
                    {
                        photoUrl = image_url;
                    }
                }
            }

            // new player
            Player player = wizWS.UpdatePlayer(0, PlayerName.Text.Trim(), photoUrl, UserSession.UserId);

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