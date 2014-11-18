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