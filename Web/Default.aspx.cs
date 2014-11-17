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
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        // Perform login
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            // get post vars
            string username = txtUsername.Value;
            string password = txtPassword.Value;
            bool remember = cbRemember.Checked;

            WizardService wizWS = new WizardService();

            var session = wizWS.Login(username, password, Functions.GetUserIPAddress());

            // validate login
            if (session != null && !string.IsNullOrEmpty(session.Secret))
            {
                // valid
                // redirect
                Response.Redirect("~/Home.aspx", true);
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