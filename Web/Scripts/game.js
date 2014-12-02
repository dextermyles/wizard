// log to console
function logMessage(message) {
    var $logWindow = $("#logwindow");

    if ($logWindow == null) {
        console.log(message);

        return;
    }
        
    // append message
    $logWindow.val($logWindow.val() + message + "\n");

    // scroll to bottom
    if ($logWindow.length)
        $logWindow.scrollTop($logWindow[0].scrollHeight - $logWindow.height());
}