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
    public partial class Default : System.Web.UI.Page
    {
        // service
        WizardService wizWS = new WizardService();

        protected void Page_Load(object sender, EventArgs e)
        {
            // is valid session
            bool isValidSession = Functions.IsValidSession();

            if (isValidSession)
            {
                // redirect to home page
                Response.Redirect("~/Home.aspx");
            }
            else
            {
                // is post
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
                        // perform facebook login
                        var session = wizWS.FacebookLogin(strFacebookEmail, strFacebookUserId);

                        // validate session
                        ValidateSession(session, remember);
                    }
                }
            }
        }

        // Perform login
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            // get post vars
            string username = txtUsername.Value;
            string password = txtPassword.Value;
            bool remember = cbRemember.Checked;

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
    }
}