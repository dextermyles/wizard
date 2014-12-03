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
    public partial class Register : System.Web.UI.Page
    {
        // service
        WizardService wizWS = new WizardService();

        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            // get post vars
            string strUsername = Functions.CleanInput(txtUsername.Value);
            string strEmailAddress = Functions.CleanInput(txtEmailAddress.Value);
            string strPassword = Functions.CleanInput(txtPassword.Value);

            // validate username
            if (strUsername != txtUsername.Value)
            {
                // display error
                MessageBox.Visible = true;
                MessageBoxText.InnerHtml = "<strong>Error</strong>: Username contains invalid characters. They have been removed.";

                txtUsername.Value = strUsername;

                return;
            }

            // validate email
            if (strEmailAddress != txtEmailAddress.Value)
            {
                // display error
                MessageBox.Visible = true;
                MessageBoxText.InnerHtml = "<strong>Error</strong>: Email contains invalid characters. They have been removed.";

                txtEmailAddress.Value = strEmailAddress;

                return;
            }

            // validate password
            if (strPassword != txtPassword.Value)
            {
                // display error
                MessageBox.Visible = true;
                MessageBoxText.InnerHtml = "<strong>Error</strong>: Password contains invalid characters.";

                return;
            }

            // create new user
            var createResult = wizWS.NewUser(strUsername, strPassword, strEmailAddress, true);

            // validate
            if (createResult != null && createResult.Result)
            {
                // success
                if (!string.IsNullOrEmpty(createResult.Secret))
                {
                    // create new cookie
                    HttpCookie cookie = new HttpCookie("OfficeWizard");

                    // expiry date
                    cookie.Expires = DateTime.Now.AddDays(1);

                    // session secret
                    cookie.Values.Add("secret", createResult.Secret);

                    // save cookie
                    Response.Cookies.Add(cookie);

                    // redirect to home page
                    Response.Redirect("~/Home.aspx");
                }
                else
                {
                    // display error
                    MessageBox.Visible = true;
                    MessageBoxText.InnerHtml = "<strong>Error</strong>: An unknown error has occured";
                }
            }
            else
            {
                // get resource error msg
                string errorMsg = (string)GetGlobalResourceObject("Language", createResult.Message);

                if (string.IsNullOrEmpty(errorMsg))
                    errorMsg = createResult.Message;

                // display error
                MessageBox.Visible = true;
                MessageBoxText.InnerHtml = "<strong>Error</strong>: " + errorMsg;
            }
        }
    }
}