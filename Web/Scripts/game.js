// Start the connection
$.connection.hub.start().done(init);

var gameSessionProxy = $.connection.gameSessionHub;

function init() {
    logMessage("-- Connected to server --");
}

function logMessage(message) {
    var $logWindow = $("#logwindow");

    // append message
    $logWindow.val($logWindow.val() + message + "\n");
}