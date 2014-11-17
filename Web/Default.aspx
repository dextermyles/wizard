<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WizardGame.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script>
        // facebook status changed callback
        function statusChangeCallback(response) {
            console.log('statusChangeCallback');
            console.log(response);

            if (response.status === 'connected') {
                // Logged into your app and Facebook.
                logMessage('-- logged into app --');

                beginLogin();

            } else if (response.status === 'not_authorized') {
                // The person is logged into Facebook, but not your app.
                logMessage('user not authorized');
            } else {
                logMessage('user not logged into facebook');
            }
        }

        // facebook login attempted
        function checkLoginState() {
            FB.getLoginStatus(function (response) {
                statusChangeCallback(response);
            });
        }

        function fb_login() {
            FB.login(function (response) {
                if (response.authResponse) {
                    logMessage('Welcome!  Fetching your information.... ');
                    //console.log(response); // dump complete info
                    access_token = response.authResponse.accessToken; //get access token
                    user_id = response.authResponse.userID; //get FB UID

                    FB.api('/me', function (response) {
                        user_email = response.email; //get user email
                        // you can store this data into your database 
                        logMessage('player: ' + response.first_name + ' ' + response.last_name);
                        logMessage('userID: ' + response.id);
                        logMessage('email: ' + user_email);
                    });

                } else {
                    //user hit cancel button
                    logMessage('User cancelled login or did not fully authorize.');

                }
            }, {
                scope: 'email'
            });
        }

        function beginLogin() {
            logMessage('-- getting user details from facebook --');

            // perform fbook sync/login
            FB.api('/me', function (response) {
                logMessage('-- details retrieved --');
                console.log(response); // log js object
                logMessage('player: ' + response.first_name + ' ' + response.last_name);
                logMessage('userID: ' + response.id);
            });
        };

        // init
        window.fbAsyncInit = function () {
            FB.init({
                appId: '343664855758060',
                oauth: true,
                status: true, // check login status
                cookie: true, // enable cookies to allow the server to access the session
                xfbml: true, // parse XFBML
                version: 'v2.2'
            });

            // get login status from fbook
            FB.getLoginStatus(function (response) {
                statusChangeCallback(response);
            });
        };
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container login well well-sm" style="">
        <h3 class="form-signin-heading" style="margin-top: 0px;">Please sign in</h3>
        <div id="MessageBox" class="alert alert-danger alert-dismissible fade in" role="alert" runat="server" visible="false">
            <button type="button" class="close" data-dismiss="alert"><span aria-hidden="true">×</span><span class="sr-only">Close</span></button>
            <p id="MessageBoxText" runat="server" />
        </div>
        <label for="inputEmail" class="sr-only">Username</label>
        <input type="text" id="txtUsername" class="form-control username" placeholder="Username" required autofocus runat="server" />
        <label for="inputPassword" class="sr-only">Password</label>
        <input type="password" id="txtPassword" class="form-control password" placeholder="Password" required runat="server" />
        <div class="checkbox">
            <label>
                <input id="cbRemember" type="checkbox" value="remember-me" runat="server" />
                Remember me
            </label>
        </div>
        <asp:Button ID="btnLogin" CssClass="btn btn-lg btn-primary btn-block" Text="Sign in" runat="server" OnClick="btnLogin_Click" />
        <button id="btnFacebook" class="btn btn-lg btn-primary btn-block" onclick="fb_login(); return false;">Sign in with Facebook</button>
    </div>
    <style type="text/css">
        .login {
            width: 420px;
        }
    </style>
</asp:Content>
