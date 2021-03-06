﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="GameLobbyRoom.aspx.cs" Inherits="WizardGame.GameLobbyRoom" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- viewport settings for mobile -->
    <meta name="viewport" content="height=device-height,width=device-width,initial-scale=0.47,maximum-scale=0.85" >

    <script type="text/javascript">
        // player object
        var currentPlayer = new function () {
            this.PlayerId = 0,
            this.Name = "",
            this.PictureURL = "",
            this.UserId = 0,
            this.connectionId = ""
        };

        var ConnectionState = {
            DISCONNECTED:0,
            CONNECTED:1,
            INACTIVE:2
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

        // player list
        var playerList = Array();

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

            // start refreshing player list
            startRefreshingPlayerList();

            // stop reconnect interval
            stopReconnecting();
        };

        // Start the connection
        $.connection.hub.start().done(onConnectionInit);

        $.connection.hub.reconnecting(function () {
            // broadcast
            appendChatMessage("Server", "Attempting to reconnect to game lobby.");

            // stop refreshing player list
            stopRefreshingPlayerList();

            // update flag
            isConnected = false;
        });

        $.connection.hub.reconnected(function () {
            // broadcast
            appendChatMessage("Server", "Reconnected to game lobby.");
            
            // stop reconnect interval
            stopReconnecting();

            // tell server we are joining the lobby
            joinGameLobby(currentPlayer.PlayerId, groupNameId);

            // update flag
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

            // update flag
            isConnected = false;

            // start reconnecting
            startReconnecting();
        });

        // get reference to hub
        var hub = $.connection.gameLobbyHub;

        /*******************************************
         * functions that are called by the server *
         *******************************************/

        // playerJoinedLobby
        hub.client.playerJoinedLobby = function (playerId, playerName, connectionId, gameId) {
            // game has started
            if(gameId != null && gameId > 0) {
                // redirect player to game
                window.location = "Play.aspx?GameId=" + gameId;
            }
            else {
                // chat message player joined
                appendChatMessage(playerName, "Joined the game lobby.")

                // add player to list
                if(!isPlayerInList(playerName))
                    $(".player-list").append("<li class='list-group-item' id='player-" + playerId + "'>" + playerName + " <span class=\"glyphicon glyphicon-ok-sign\"></li>");

                // resume refreshing player list
                startRefreshingPlayerList();
            }
        };

        // playerLeftLobby
        hub.client.playerLeftLobby = function playerLeftLobby(playerId, playerName) {
            // chat message player left lobby
            appendChatMessage(playerName, "Left the game lobby.");

            // remove name from player list
            $("#player-" + playerId).remove();
        };

        // playerLeftLobby
        hub.client.playerTimedOut = function playerTimedOut(playerId, playerName) {
            // chat message player left lobby
            appendChatMessage(playerName, "Timed out.");
        };

        // playerReconnected
        hub.client.playerReconnected = function playerReconnected(playerId, playerName, connectionId, gameId) {
            // game has started
            if(gameId != null && gameId > 0) {
                // redirect player to game
                window.location = "Play.aspx?GameId=" + gameId;
            }
            else {
                // chat message player joined
                appendChatMessage(playerName, "Reconnect to game lobby.")

                if(!isPlayerInList(playerName))
                    $(".player-list").append("<li class='list-group-item' id='player-" + playerId + "'>" + playerName + " <span class=\"glyphicon glyphicon-ok-sign\"></li>");

                // resume refreshing player list
                startRefreshingPlayerList();
            }
        };

        // receiveChatMessage
        hub.client.receiveChatMessage = function receiveChatMessage(playerName, message) {
            // append to chat window
            appendChatMessage(playerName, message);
        };

        // get list of players (status)
        hub.client.refreshPlayerList = function(_playerList) {
            // update player list
            playerList = _playerList;

            // redraw player list
            drawPlayerList();
        };

        // gameStarted
        hub.client.gameStarted = function gameStarted(gameData) {
            appendChatMessage("Server", "Game will start in 3 seconds");

            // redirect to game room
            setTimeout(function() {
                window.location = 'Play.aspx?gameId=' + gameData.GameId;
            }, 3000);
        }

        // gameCancelled
        hub.client.gameCancelled = function gameCancelled() {
            appendChatMessage("Server", "Game cancelled by host");

            // redirect to home page in 3 seconds
            setTimeout(function() { 
                window.location = 'Home.aspx'; 
            }, 3000);
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

        var playerListInterval = 0;

        function startRefreshingPlayerList() {
            // clear existing interval
            clearInterval(playerListInterval);

            // start new interval
            playerListInterval = setInterval(refreshPlayerList, 3000);
        }

        function stopRefreshingPlayerList() {
            // clear existing interval
            clearInterval(playerListInterval);
        }

        function refreshPlayerList() {
            if(isConnected) {
                hub.server.refreshPlayerList(gameLobbyId, groupNameId);
            }
        }

        function drawPlayerList() {
            $playerList = $(".player-list");

            // clear list
            $playerList.html('');

            // playerlist exists
            if(playerList != null) {
                // loop through each player
                for(var i = 0; i < playerList.length; i++) {
                    // player ref
                    var player = playerList[i];

                    var connectionStateHtml = "";
                    
                    switch(player.ConnectionState)
                    {
                        case ConnectionState.DISCONNECTED:
                            connectionStateHtml = " <span class='glyphicon glyphicon-remove-circle'></span>";
                            break;
                        case ConnectionState.INACTIVE:
                            connectionStateHtml = " <span class='glyphicon glyphicon-question-sign'></span>";
                            break;
                        case ConnectionState.CONNECTED:
                            connectionStateHtml = " <span class='glyphicon glyphicon-ok-sign'></span>";
                            break;
                        default:
                            break;
                    }

                    // append list group item
                    var listItemHtml = "<li class=\"list-group-item\" id=\"" + player.PlayerId + "\">" + player.Name + connectionStateHtml + "</li>";

                    $playerList.append(listItemHtml);
                }

                // get total # of players
                var totalPlayers = playerList.length;

                if(totalPlayers > 2) {
                    $("#btnStartGame").removeAttr("disabled");
                }
                else {
                    $("#btnStartGame").attr("disabled", "disabled");
                }

                // update player count
                $(".total-players").html(totalPlayers.toString() + " / " + maxPlayers);
            }
        }

        var reconnectInterval = 0;

        function startReconnecting() {
            // clear any existing reconnect interval
            clearInterval(reconnectInterval);

            // start reconnecting interval
            reconnectInterval = setInterval(waitForReconnect, 5000);
        }

        function stopReconnecting() {
            // clear any existing reconnect interval
            clearInterval(reconnectInterval);
        }

        function waitForReconnect() {
            if(!isConnected) {
                // Start the connection
                $.connection.hub.start().done(onConnectionInit);
            } 
        }

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
            hub.server.sendChatMessage(currentPlayer.Name, message, groupNameId);
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

        
        function isPlayerInList(playerName) {
            var player = $(".player-list li:contains('" + playerName + "')");

            if (player != null && player.length > 0) {
                logMessage("-- " + playerName + " is already in list --");

                return true;
            }

            logMessage("-- " + playerName + " is not in list --");
            return false; 
        };

        function sendKeepAlive() {
            if(isConnected) {
                // send keep-alive
                hub.server.keepAlive(currentPlayer.PlayerId, gameLobbyId, groupNameId);
            }
        };

        function startGame() {
            // disable start button
            $("#btnStartGame").attr("disabled", "disabled");
            $("#btnCancelGame").attr("disabled", "disabled");

            if(isConnected) {
                if(playerList.length > 2) {
                    hub.server.startGame(gameLobbyId, groupNameId);
                }
                else {
                    alert("Not enough players to start the game");
                }
            } 

            return false;
        };

        function cancelGame() {
            if(isConnected) {
                hub.server.cancelGame(gameLobbyId, groupNameId);
            }

            return false;
        };

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
        <div class="row">
            <div class="panel panel-default">
                <div class="panel-heading">
                    <span runat="server" id="GameLobbyTitle"></span>
                    <span class="pull-right">
                        Players: <span class="total-players"><%=Players.Length %></span>
                    </span>
                </div>
                <div class="panel-body">
                    <div class="col-xs-9">
                        <div class="form-group">
                            <textarea id="txtChatWindow" name="txtChatWindow" class="form-control" rows="12" readonly></textarea>
                            <style type="text/css">
                                #txtChatWindow {
                                    width: 100%;
                                    resize: none;
                                    background-color: transparent;
                                }
                            </style>
                        </div>
                        <div>
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
                    <div class="col-xs-3">
                        <ul class="list-group player-list">
                            <%= ListGameLobbyPlayersHtml()%>
                        </ul>
                    </div>
                    <div class="clearfix"></div>
                </div>
                <div class="panel-footer">
                    <span class="pull-right">
                        <% 
                        // check if player is game host
                        if (IsGameHost)
                        {
                        %>
                        <input type="button" id="btnStartGame" class="btn btn-primary" onclick="return startGame(); return false;" value="Start game" disabled />
                        <input type="button" id="btnCancelGame" class="btn btn-danger" onclick="return cancelGame(); return false;" value="Cancel game" />
                        <% 
                        }
                        else
                        {
                        %>
                        <input type="button" id="btnQuitGame" class="btn btn-primary" onclick="return leaveGameLobby(); return false;" value="Quit game" />
                        <% 
                        }
                        %>
                    </span>
                    <div class="clearfix"></div>
                </div>
                <style type="text/css">
                    .panel-footer {
                        padding: 10px 15px;
                        background-color: #E5E5E5;
                        border-top: 1px solid #A8A8A8;
                        border-bottom-right-radius: 3px;
                        border-bottom-left-radius: 3px;
                    }
                </style>
            </div>
        </div>
    </div>
</asp:Content>
