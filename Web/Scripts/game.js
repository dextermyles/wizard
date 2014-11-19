

// Start the connection
$.connection.hub.start().done(on_connect);

var hub = $.connection.gameSessionHub;

function on_connect() {
    logMessage("-- connected to server: " + $.connection.hub.id + " --");
}

function logMessage(message) {
    var $logWindow = $("#logwindow");

    // append message
    $logWindow.val($logWindow.val() + message + "\n");
}