<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WizardGame.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script>
        // facebook status changed callback
        function statusChangeCallback(response) {
            if (response.status === 'connected') {
                // Logged into your app and Facebook.
                logMessage('-- logged into app --');
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
                    // log
                    logMessage('-- facebook login complete --');

                    // query api
                    beginLogin();

                } else {
                    //user hit cancel button
                    logMessage('User cancelled login or did not fully authorize.');
                }
            }, {
                scope: 'email'
            });
        }

        function beginLogin() {
            // log
            logMessage('-- querying facebook api --');

            // perform fbook sync/login
            FB.api('/me', function (response) {
                // log
                logMessage('-- details retrieved --');
                console.log(response);

                // update hidden fields with facebook values
                $("#MainContent_txtFacebookEmail").val(response.email);
                $("#MainContent_txtFacebookUserId").val(response.id);

                // submit login
                $("#MainForm").submit();
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
        <input type="hidden" id="txtFacebookEmail" name="txtFacebookEmail" runat="server" />
        <input type="hidden" id="txtFacebookUserId" name="txtFacebookUserId" runat="server" />
    </div>
    <style type="text/css">
        .login {
            width: 420px;
        }
    </style>
</asp:Content>
