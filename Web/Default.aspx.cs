using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WizardGame.WizardService;

namespace WizardGame
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // http post
            if (Page.IsPostBack)
            {
                // get post vars
                string strFacebookEmail = txtFacebookEmail.Value;
                string strFacebookUserId = txtFacebookUserId.Value;
                bool remember = cbRemember.Checked;
                bool isFacebookLogin = (txtIsFacebookLogin.Value == "1") ? true : false;

                // validate
                if (isFacebookLogin 
                    && !string.IsNullOrEmpty(strFacebookEmail) 
                    && !string.IsNullOrEmpty(strFacebookUserId))
                {
                    // service
                    WizardServiceClient wizWS = new WizardServiceClient();

                    // perform facebook login
                    var session = wizWS.FacebookLogin(strFacebookEmail, strFacebookUserId);

                    // close service
                    wizWS.Close();

                    // validate session
                    ValidateSession(session, remember);
                }
            }

            // validate existing cookie
            ValidateCookie();
        }

        // Perform login
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            // get post vars
            string username = txtUsername.Value;
            string password = txtPassword.Value;
            bool remember = cbRemember.Checked;

            // service
            WizardServiceClient wizWS = new WizardServiceClient();

            // perform login
            var session = wizWS.Login(username, password, Helpers.Functions.GetUserIPAddress());

            // validate session
            ValidateSession(session, remember);
        }

        private void ValidateSession(Session session, bool remember = false)
        {
            // validate login
            if (session != null && !string.IsNullOrEmpty(session.Secret))
            {
                // save cookies
                string secret = session.Secret;
                int sessionId = session.SessionId;
                DateTime dateCreated;

                if(session.DateCreated.HasValue)
                    dateCreated = session.DateCreated.Value;

                // cookie
                HttpCookie cookie = new HttpCookie("OfficeWizard");
                
                // expiry
                cookie.Expires = DateTime.Now.AddDays(1);
                cookie.Values.Add("secret", secret);

                // remember user
                if (remember)
                {
                    cookie.Values.Add("remember", "true");
                    cookie.Expires = DateTime.Now.AddDays(30);
                }

                // save cookie
                Response.Cookies.Add(cookie);

                // valid
                Response.Redirect("~/Home.aspx");
            }
            else
            {
                // show error box
                MessageBox.Visible = true;
                MessageBoxText.InnerHtml = "<strong>Error</strong>: Invalid username or password";
            }
        }

        private void ValidateCookie()
        {
            // check for existing cookie
            HttpCookie cookie = Request.Cookies["OfficeWizard"];

            // validate
            if (cookie != null)
            {
                // get secret from cookie
                string secret = cookie.Values["secret"];

                if (!string.IsNullOrEmpty(secret))
                {
                    // service
                    WizardServiceClient wizWS = new WizardServiceClient();

                    // validate secret
                    var session = wizWS.ValidateSession(secret);

                    // close service
                    wizWS.Close();

                    // invalid secret
                    if (session != null && !string.IsNullOrEmpty(session.Secret))
                    {
                        // validate session result
                        ValidateSession(session);
                    }
                }
            }
        }
    }
}