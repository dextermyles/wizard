// facebook init
var isFacebookLoggedIn = false;

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
    // log
    logMessage('-- retrieving facebook PictureURL --');

    // perform fbook sync/login
    FB.api('/me/picture', function (response) {
        // log
        logMessage('-- PictureURL received --');
        console.log(response);
    });
}

// Start the connection
$.connection.hub.start().done(init);

var hub = $.connection.gameSessionHub;

function init() {
    logMessage("-- connected to server: " + $.connection.hub.id + " --");
}

function logMessage(message) {
    var $logWindow = $("#logwindow");

    // append message
    $logWindow.val($logWindow.val() + message + "\n");
}