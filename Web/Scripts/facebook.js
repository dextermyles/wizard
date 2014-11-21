// facebook init
var isFacebookLoggedIn = false;
var facebookPhotoUrl = "";

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

// start facebook login
function facebookLogin() {
    FB.login(function (response) {
        if (response.authResponse) {
            // log
            logMessage('-- facebook login complete --');

            // is facebook login
            $("#MainContent_txtIsFacebookLogin").val(1);

            // query api
            getFacebookLoginInfo();
        } else {
            //user hit cancel button
            logMessage('-- User cancelled login or did not fully authorize. --');
        }
    }, {
        scope: 'email'
    });
}

// check facebook login state
function checkLoginState() {
    FB.getLoginStatus(function (response) {
        statusChangeCallback(response);
    });
}

// facebook login state callback
function statusChangeCallback(response) {
    if (response.status === 'connected') {
        // Logged into your app and Facebook.
        logMessage('-- logged into app --');
        isFacebookLoggedIn = true;
    } else if (response.status === 'not_authorized') {
        // The person is logged into Facebook, but not your app.
        logMessage('-- user not authorized --');
        isFacebookLoggedIn = false;
    } else {
        logMessage('-- user not logged into facebook --');
        isFacebookLoggedIn = false;
    }
}

function getFacebookLoginInfo() {
    // validate
    if (!isFacebookLoggedIn)
        return;

    // log
    logMessage('-- retrieving facebook info --');

    // perform fbook sync/login
    FB.api('/me', function (response) {
        // log
        logMessage('-- facebook info received --');
        console.log(response);

        // update hidden fields with facebook values
        $("#MainContent_txtFacebookEmail").val(response.email);
        $("#MainContent_txtFacebookUserId").val(response.id);

        // submit login
        $("#MainForm").submit();
    });
};

function getFacebookPictureURL() {
    // validate
    if (!isFacebookLoggedIn)
        return;

    // log
    logMessage('-- retrieving facebook PictureURL --');

    // perform fbook sync/login
    FB.api('/me/picture', function (response) {
        // log
        logMessage('-- PictureURL received --');

        var picture_url = response.data.url;

        // update global value
        facebookPhotoUrl = picture_url;

        // update form element
        $("#MainContent_txtFacebookPhotoUrl").val(picture_url);
    });
}