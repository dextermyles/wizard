using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WizardGame.WizardService;

namespace WizardGame
{
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            // service
            WizardServiceClient wizWS = new WizardServiceClient();

            // get post vars
            string strUsername = txtUsername.Value;
            string strEmailAddress = txtEmailAddress.Value;
            string strPassword = txtPassword.Value;

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