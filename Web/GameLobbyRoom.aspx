<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="GameLobbyRoom.aspx.cs" Inherits="WizardGame.GameLobbyRoom" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        // player object
        var currentPlayer = new function () {
            this.PlayerId = 0,
            this.Name = "",
            this.PictureURL = "",
            this.UserId = 0,
            this.connectionId = ""
        };

        // playerList array
        var playerList = Array();

        // server group id
        var groupNameId = '<%=GameLobby.GroupNameId%>';

        // game lobby id
        var gameLobbyId = '<%=GameLobby.GameLobbyId%>';

        // is connected to server
        var isConnected = false;

        // max players
        var maxPlayers = <%=GameLobby.MaxPlayers%>;

        // connected players
        var totalPlayers = 0;

        var keepAliveInterval = 0;

        currentPlayer.PlayerId = '<%= PlayerData.PlayerId %>';
        currentPlayer.Name = '<%= PlayerData.Name %>';
        currentPlayer.PictureURL = '<%= PlayerData.PictureURL %>';
        currentPlayer.UserId = '<%= PlayerData.UserId %>';

        // initialize connection
        function onConnectionInit() {

            // update connection flag
            isConnected = true;

            // tell server we are joining the lobby
            joinGameLobby(currentPlayer.PlayerId, groupNameId);

            // append chat message
            appendChatMessage("Server", "Connected to game lobby!");

            // setup keep-alive
            keepAliveInterval = setInterval(function () {
                keepAlive();
            }, 30000);
        };

        // Start the connection
        $.connection.hub.start().done(onConnectionInit);



        // get reference to hub
        var hub = $.connection.gameLobbyHub;

        /*******************************************
         * functions that are called by the server *
         *******************************************/

        $.connection.hub.disconnected(function () {
            
        });

        $.connection.hub.reconnecting(function () {
            appendChatMessage("Server", "Attempting to reconnect to game lobby.");

            isConnected = false;
        });

        $.connection.hub.reconnected(function () {
            appendChatMessage("Server", "Reconnected to game lobby.");

            // tell server we are joining the lobby
            joinGameLobby(currentPlayer.PlayerId, groupNameId);

            isConnected = true;
        });

        $.connection.hub.disconnected(function () {
            // has error
            if ($.connection.hub.lastError) { 
                appendChatMessage("Server", $.connection.hub.lastError.message);
            }
            else {
                appendChatMessage("Server", "You have been disconnected from game lobby.");
            }

            isConnected = false;
        });

        // playerJoinedLobby
        hub.client.playerJoinedLobby = function (playerId, playerName, connectionId) {
            // log message
            logMessage("-- " + playerName + " has joined the game lobby --");

            // chat message player joined
            appendChatMessage(playerName, "Joined the game lobby.")

            // add player to list
            if(!isPlayerInList(playerName))
                $(".player-list").append("<li class='list-group-item' id='player-" + playerId + "'>" + playerName + "</li>");

            // update player count
            updatePlayerCount();
        };

        // playerLeftLobby
        hub.client.playerLeftLobby = function playerLeftLobby(playerId, playerName) {
            // log message
            logMessage("-- " + playerName + " has left the game lobby --");
            
            // chat message player left lobby
            appendChatMessage(playerName, "Left the game lobby.");

            // remove name from player list
            $("#player-" + playerId).remove();

            // update player count
            updatePlayerCount();
        };

        // playerLeftLobby
        hub.client.playerTimedOut = function playerTimedOut(playerId, playerName) {
            // log message
            logMessage("-- " + playerName + " has timed out--");
            
            // chat message player left lobby
            appendChatMessage(playerName, "Timed out.");
        };

        // playerReconnected
        hub.client.playerReconnected = function playerReconnected(playerId, playerName, connectionId) {
            // log message
            logMessage("-- " + playerName + " has reconnected--");
            
            // chat message player left lobby
            appendChatMessage(playerName, "Reconnected.");
        };

        // receiveChatMessage
        hub.client.receiveChatMessage = function receiveChatMessage(playerName, message) {
            // log message
            logMessage("-- message received from: " + playerName + " --");

            // append to chat window
            appendChatMessage(playerName, message);
        }

        // gameStarted
        hub.client.gameStarted = function gameStarted(gameData) {
            console.log(gameData);

            logMessage("-- game started by host --");

            appendChatMessage("Server", "Game will start in 3 seconds");

            // redirect to game room
            setTimeout(function() {
                window.location = 'Play.aspx?gameId=' + gameData.GameId;
            }, "3000");
        }

        // gameCancelled
        hub.client.gameCancelled = function gameCancelled() {
            logMessage("-- game cancelled by host --");

            appendChatMessage("Server", "Game cancelled by host");

            // redirect to home page in 3 seconds
            setTimeout(function() { window.location = 'Home.aspx'; }, "3000");
        }

        // logMessage
        hub.client.logMessage = function logMessage(message) {
            // append to log window
            logMessage(message);
        };

        // logMessage
        hub.client.ping = function ping() {
            // append to log window
            logMessage("-- keep-alive received from server --");
        };

        /*******************************************
         * functions that are called by the client *
         *******************************************/

        function joinGameLobby(playerId, groupNameId) {
            // call joinGameLobby on server
            hub.server.joinGameLobby(playerId, gameLobbyId, groupNameId)
                .done(function () {
                    logMessage("-- joinGameLobby executed on server --");
                })
                .fail(function (msg) {
                    logMessage("-- " + msg + " --");
                });
        };

        function leaveGameLobby() {
            // call leaveGameLobby on server
            hub.server.leaveGameLobby(gameLobbyId, currentPlayer.PlayerId, currentPlayer.Name, groupNameId)
                .done(function () {
                    logMessage("-- leaveGameLobby executed on server --");

                    // redirect to home page
                    window.location = 'Home.aspx';
                })
                .fail(function (msg) {
                    logMessage("-- " + msg + " --");
                });
        };

        function sendChatMessage() {
            // get chat message box
            var $chatbox = $("#txtChatMessage");

            // get msg
            var message = $chatbox.val();

            // clear chat box
            $chatbox.val('');

            // send to server
            hub.server.sendChatMessage(currentPlayer.Name, message, groupNameId)
                .done(function () {
                    logMessage("-- sendChatMessage executed on server --");
                })
                .fail(function (error) {
                    logMessage("-- " + msg + " --");
                });
        };

        function clearChatWindow() {
            var $chatwindow = $("#txtChatWindow");

            // clear chat window
            $chatwindow.val('');
        };

        function appendChatMessage(playerName, message) {
            var oldMessages = $("#txtChatWindow").val();
            var time = new Date();
            var timeStr = time.getHours() + ":" + time.getMinutes() + ":" + time.getSeconds();
            var chatStr = "[" + timeStr + "] " + playerName + ": " + message + "\n";

            // append new message
            $("#txtChatWindow").val(oldMessages + chatStr);

            // scroll to bottom
            var psconsole = $('#txtChatWindow');

            if(psconsole.length)
                psconsole.scrollTop(psconsole[0].scrollHeight - psconsole.height());
        };

        function updatePlayerCount() {
            // reset count
            totalPlayers = 0;

            // count items in player list
            $(".player-list li").each(function (i, e) {
                totalPlayers++;
            });

            // update player count
            $(".total-players").html(totalPlayers.toString() + " / " + maxPlayers);

            // has min players connected
            if(totalPlayers > 2) {
                $("#btnStartGame").removeAttr("disabled");
            }
        }

        function isPlayerInList(playerName) {
            var player = $(".player-list li:contains('" + playerName + "')");

            if (player != null && player.length > 0) {
                logMessage("-- " + playerName + " is already in list --");

                return true;
            }

            logMessage("-- " + playerName + " is not in list --");
            return false; 
        };

        function keepAlive() {
            if (isConnected) {
                hub.server.ping()
                    .done(function () {
                        logMessage("-- keep-alive request sent to server --");
                    });
            }
        }

        function startGame() {
            // disable start button
            $("#btnStartGame").attr("disabled", "disabled");
            $("#btnCancelGame").attr("disabled", "disabled");

            if(isConnected) {
                if(totalPlayers > 2) {
                    hub.server.startGame(gameLobbyId, groupNameId)
                        .done(function() {
                            logMessage("-- start game sent to server --");
                        })
                        .fail(function (msg) {
                            logMessage("-- " + msg + " --");
                        });
                }
                else {
                    alert("Not enough players to start the game");
                }
            } 

            return false;
        }

        function cancelGame() {
            if(isConnected) {
                hub.server.cancelGame(gameLobbyId, groupNameId).
                    done(function() {
                        // log msg
                        logMessage("-- cancel game sent to server --");
                    })
                    .fail(function (msg) {
                        // log error
                        logMessage("-- " + msg + " --");
                    });
            }

            return false;
        }

        /******************************************
         * functions that are called on page load *
         ******************************************/

        $(document).ready(function () {
            // display connecting message
            appendChatMessage("Server", "Connecting to game lobby... Please wait");
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="page-header" style="margin-top: 14px;">
            <h1 runat="server" id="GameLobbyTitle">Title</h1>
        </div>
        <div class="col-sm-8">
            <div class="panel panel-default">
                <div class="panel-heading">
                    <strong>Chat window</strong>
                </div>
                <div class="panel-body">
                    <div class="form-group">
                        <textarea id="txtChatWindow" name="txtChatWindow" class="form-control" rows="6" readonly></textarea>
                        <style type="text/css">
                            #txtChatWindow {
                                width: 100%;
                                border: none;
                                resize: none;
                                background-color: transparent;
                            }
                        </style>
                    </div>
                    <div class="form-group" style="margin-bottom: 0px;">
                        <div class="input-group">
                            <input type="text" id="txtChatMessage" name="txtChatMessage" class="form-control" placeholder="Send a message to other players" />
                            <span class="input-group-btn">
                                <input type="button" id="btnClearChat" name="btnClearChat" class="btn btn-default" value="Clear" onclick="clearChatWindow(); return false;" />
                                <input type="button" id="btnSendChat" name="btnSendChat" class="btn btn-primary" value="Send" onclick="sendChatMessage(); return false;" />
                                <script>
                                    // bind enter event to chat text box
                                    $("#txtChatMessage").bind("keypress", function (event) {
                                        // enter key pressed
                                        if (event.keyCode == 13) {
                                            sendChatMessage();

                                            return false;
                                        }
                                    });
                                </script>
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="col-sm-4">
            <div class="panel panel-default">
                <div class="panel-heading">
                    <strong>Players</strong>
                </div>
                <ul class="list-group player-list">
                    <%= ListGameLobbyPlayersHtml()%>
                </ul>
                <div class="panel-footer">
                    Connected: <span class="total-players"><%=Players.Length %></span>
                </div>
            </div>
            <div class="form-group">
                <% 
                    // check if player is game host
                    if (IsGameHost)
                    {
                        // player is the game host
                %>
                <input type="button" id="btnStartGame" class="btn btn-lg btn-primary btn-block" onclick="return startGame(); return false;" value="Start game" disabled />
                <input type="button" id="btnCancelGame" class="btn btn-lg btn-default btn-block" onclick="return cancelGame(); return false;" value="Cancel game" />
                <% 
                    }
                    else
                    {
                        // player is not the host 
                %>
                <input type="button" id="btnQuitGame" class="btn btn-lg btn-primary btn-block" onclick="return leaveGameLobby(); return false;" value="Quit game" />
                <% 
                    }
                %>
            </div>
        </div>
    </div>
</asp:Content>
