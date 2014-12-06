﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Play.aspx.cs" Inherits="WizardGame.Play" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        // playertracker
        var numPlayersConnected = parseInt('<%= Players.Length %>');
        var numPlayersExpected = parseInt('<%= Players.Length %>');

        // current player object
        var currentPlayer = new function () {
            this.PlayerId = 0,
            this.Name = "",
            this.PictureURL = "",
            this.UserId = 0,
            this.ConnectionId = "",
            this.IsTurn = false,
            this.IsDealer = false,
            this.IsLastToAct = false,
            this.Score = 0;
        };

        // game state status enum
        var gameStateStatus = {
            DealInProgress: 0,
            BiddingInProgress: 1,
            RoundInProgress: 2,
            Setup: 3,
            Finished:4,
            TurnEnded: 5,
            SelectTrump: 6,
            RoundEnded: 7
        };

        // card suits
        var suit = {
            None: -1,
            Spades: 0,
            Hearts: 1,
            Clubs: 2,
            Diamonds: 3,
            Fluff: 4,
            Wizard: 5
        };

        // get suit name by value/id
        function getSuitName(suitId) {
            switch(suitId) {
                case suit.None:
                    return "None";
                case suit.Spades:
                    return "Spades";
                case suit.Hearts:
                    return "Hearts";
                case suit.Clubs:
                    return "Clubs";
                case suit.Diamonds:
                    return "Diamonds";
                case suit.Fluff:
                    return "Fluff";
                case suit.Wizard:
                    return "Wizard";
                default:
                    return "Unknown";
            }
        };

        // get card image path
        function getCardImagePath(_suit, _value) {
            if(_suit == null)
                return "deck_cover.png";

            var suitName = getSuitName(_suit);
            var value = _value;
            var imageFileName = '';

            if(_suit == suit.Wizard)
                imageFileName = "wizard.png";
            else if(_suit == suit.Fluff)
                imageFileName = "fluff.png";
            else
                imageFileName  = suitName.toLowerCase() + "_" + value + ".png";

            return "/Assets/Cards/" + imageFileName;
        };

        // keep track of last game state
        var lastGameState = null;

        // playerList array
        var playerList = Array();

        // playerCards array
        var playerCards = Array();

        // server group id
        var groupNameId = '<%=Game.GroupNameId%>';

        // game lobby id
        var gameLobbyId = '<%=Game.GameLobbyId%>';

        // game id
        var gameId = '<%=Game.GameId%>';

        // is connected to server
        var isConnected = false;

        // max players
        var maxPlayers = <%=GameLobby.MaxPlayers%>;

        // connected players
        var totalPlayers = 0;

        // keep alive interval
        var keepAliveInterval = 0;

        // is deal animation in progress
        var isDealing = false;

        // player just connected
        var pageJustLoaded = true;

        currentPlayer.PlayerId = '<%= PlayerData.PlayerId %>';
        currentPlayer.Name = '<%= PlayerData.Name %>';
        currentPlayer.PictureURL = '<%= PlayerData.PictureURL %>';
        currentPlayer.UserId = '<%= PlayerData.UserId %>';

        // initialize connection
        function onConnectionInit() {
            // hide offline message
            $(".offline-message").fadeOut("fast");

            // update connection flag
            isConnected = true;

            // tell server we are joining the lobby
            joinGame(currentPlayer.PlayerId, groupNameId, false);

            // append chat message
            appendChatMessage("Server", "Connected to game lobby!");

            // get updated player list every 10 seconds
            keepAliveInterval = setInterval(function () {
                sendKeepAlive();
            }, 30000);
        };

        // Start the connection
        $.connection.hub.start().done(onConnectionInit);

        $.connection.hub.reconnecting(function () {
            // broadcast msg
            appendChatMessage("Server", "Attempting to reconnect to game lobby.");

            isConnected = false;
        });

        $.connection.hub.reconnected(function () {
            // broadcast msg
            appendChatMessage("Server", "Reconnected to game lobby.");

            // tell server we are joining the lobby
            joinGame(currentPlayer.PlayerId, groupNameId, true);

            // hide offline message
            $(".offline-message").fadeOut("fast");

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

            // show offline message
            $(".offline-message").fadeIn("fast");

            isConnected = false;
        });

        // get reference to hub
        var hub = $.connection.gameHub;

        /*******************************************
         * functions that are called by the server *
         *******************************************/

        // playerJoinedLobby
        hub.client.playerJoinedGame = function (_player, numPlayersInGame) {
            // broadcast
            appendChatMessage(_player.Name, "Joined the game lobby.")

            // update num connected players
            numPlayersConnected = parseInt(numPlayersInGame);

            console.log('players present: ' + numPlayersConnected + ' / ' + numPlayersExpected);

            // all players present
            if(numPlayersConnected == numPlayersExpected) {
                // broadcast
                appendChatMessage("Server","All players present. Resuming game!");

                // resume game
                resumeGame();
            }
        };

        // player reconnected
        hub.client.playerReconnected = function playerReconnected(_player, numPlayersInGame) {
            // broadcast
            appendChatMessage("Server", _player.Name + " reconnected.");

            // update num connected players
            numPlayersConnected = parseInt(numPlayersInGame);

            console.log('players present: ' + numPlayersConnected + ' / ' + numPlayersExpected);

            // all players present
            if(numPlayersConnected == numPlayersExpected) {
                // broadcast
                appendChatMessage("Server","All players present. Resuming game!");

                // resume game
                resumeGame();
            }
        };

        hub.client.playerQuit = function playerQuit(_player, numPlayersInGame, forcedQuit) {
            // get num of connect players
            numPlayersConnected = parseInt(numPlayersInGame);

            // broadcast
            appendChatMessage("Server", _player.Name + " quit the game.");

            console.log('players present: ' + numPlayersConnected + ' / ' + numPlayersExpected);

            // pause game if missing players
            if(numPlayersConnected < numPlayersExpected) {
                // broadcast
                appendChatMessage("Server","Not all players are connected. Pausing game!");

                // pause game
                pauseGame();
            }
        };

        hub.client.playerTimedOut = function playerTimedOut(_player, numPlayersInGame) {
            // get num of connect players
            numPlayersConnected = parseInt(numPlayersInGame);

            // broadcast
            appendChatMessage("Server", _player.Name + " timed out.");

            console.log('players present: ' + numPlayersConnected + ' / ' + numPlayersExpected);

            // pause game if missing players
            if(numPlayersConnected < numPlayersExpected) {
                // broadcast
                appendChatMessage("Server","Not all players are connected. Pausing game!");

                // pause game
                pauseGame();
            }
        };

        // receiveChatMessage
        hub.client.receiveChatMessage = function receiveChatMessage(playerName, message) {
            // append to chat window
            appendChatMessage(playerName, message);
        };

        // receivePlayerList
        hub.client.receivePlayerList = function receivePlayerList(players) {
            // update player list
            updatePlayerList(players);
        };

        // receiveGameData
        hub.client.receiveGameData = function receiveGameData(gameData, isReconnect, numPlayersInGame) {
            // get num of connect players
            numPlayersConnected = parseInt(numPlayersInGame);

            // update game data
            processGameData(gameData);    

            // player reconnected
            if(isReconnect) {
                // start turn
                startTurn();
            }

            // pause game if missing players
            if(numPlayersConnected < numPlayersExpected) {
                // broadcast
                appendChatMessage("Server","Not all players are connected. Pausing game!");

                // pause game
                pauseGame();
            }

            // pause game if missing players
            if(numPlayersConnected == numPlayersExpected) {
                // broadcast
                appendChatMessage("Server","All players present. Resuming game!");

                // pause game
                resumeGame();
            }
        };

        // receiveBid
        hub.client.receiveBid = function receiveBid(_player, bid, gameData) {
            // update game state
            updateGameState(gameData.GameStateData);

            // get player div
            var $playerDiv = getPlayerDivByPlayerId(_player.PlayerId);

            // show tool tip
            showToolTip($playerDiv, _player.Name + " bid " + bid);

            // broadcast message
            appendChatMessage("Server", _player.Name + " bid " + bid);

            // delay start
            setTimeout(function() {
                // process game data
                processGameData(gameData);

                // start turn
                startTurn();
            }, 2000);
        };

        // trumpUpdated
        hub.client.trumpUpdated = function trumpUpdated(_player, newTrumpCard, gameData) {
            // update game state
            updateGameState(gameData.GameStateData);

            // broadcast to chat
            appendChatMessage("Server", _player.Name + " has made " + getSuitName(lastGameState.SuitToFollow) + " trump!");

            // update trump
            updateTrump();

            var $playerDiv = getPlayerDivByPlayerId(_player.PlayerId);

            // show tool tip
            showToolTip($playerDiv, _player.Name + " made trump " + getSuitName(lastGameState.SuitToFollow));

            // delay start
            setTimeout(function() {
                // update game data
                processGameData(gameData);

                // start turn
                startTurn();
            }, 2000);
        };

        // cardPlayedFailed
        hub.client.cardPlayedFailed = function(_cardName, gameData) {
            // broadcast
            appendChatMessage("Server", "Failed to play card: " + _cardName);

            // update game state
            updateGameState(gameData.gameState);

            // start turn
            setTimeout(function() {
                // start turn
                startTurn();
            }, 2000);
        };

        // cardPlayed
        hub.client.cardPlayed = function(_card, _player, isTurnEnded, _playerWinner, isRoundOver, previousRoundScoreArray, gameData) {
            // update game state
            updateGameState(gameData.GameStateData);

            // round is not over
            if(isRoundOver) {
                // clear player cards
                $(".player-cards").html('');
            }
            else {
                // re-draw player cards
                drawPlayerCards();
            }

            // if score history passed
            if(previousRoundScoreArray != null)
            {
                // update total scores
                for(var i = 0; i < previousRoundScoreArray.length;i++) {
                    // player score ref
                    var playerScore = previousRoundScoreArray[i];

                    // player ref
                    var player = getPlayerById(playerScore.PlayerId);

                    // broadcast
                    appendChatMessage("Server", player.Name + " scored " + playerScore.Score + " points");
                }
            }

            // animate card played
            var $playerDiv = getPlayerDivByPlayerId(_player.PlayerId);
            var playerPosition = $playerDiv.offset();
            var cardPlayedFilename = getCardImagePath(_card.Suit, _card.Value);

            // get game board position
            var $cardsPlayedDiv = $(".cards-played-container");
            var targetLeft = ($cardsPlayedDiv.offset().left + ($cardsPlayedDiv.width() / 2));
            var targetTop = ($cardsPlayedDiv.offset().top);

            // spawn card
            var cardPlayedHtml = "<a><img id='card-played' src='" + cardPlayedFilename + "' style='position: absolute; left:" + playerPosition.left + "px; top:" + playerPosition.top + "px;' class='card' /></a>";
            
            // append new card
            $("body").append(cardPlayedHtml);  
            
            // play audio
            $(".soundCardPlayed").trigger('play');

            // animate to card pile + remove card
            $("#card-played")
                .animate({
                    left: targetLeft + 'px',
                    top: targetTop + 'px',
                    opacity: 0.5
                }, 
                500, 
                function() {
                    // remove initial spawned card
                    $("#card-played").remove();

                    // append card played to container
                    $cardsPlayedDiv.append(cardPlayedHtml);

                    // update css
                    $("#card-played").css({
                        'position': 'inherit'
                    });

                    // animate pile if we have a winner
                    if(_playerWinner != null) {
                        // winner player div
                        var $playerWinnerDiv = getPlayerDivByPlayerId(_playerWinner.PlayerId);
                        var playerWinnerPosition = $playerWinnerDiv.offset();

                        // delay card pile animation
                        setTimeout(function() {
                            // animate card pile to winner
                            $(".cards-played-container .card").each(function(index) {
                                // card data
                                var $card = $(this);

                                // get card position
                                var cardPosition = {
                                    left: $card.offset().left,
                                    top: $card.offset().top
                                };

                                // update card css
                                $card.css({
                                    position: 'absolute',
                                    left: cardPosition.left + 'px',
                                    top: cardPosition.top + 'px'
                                });

                                $card.animate({
                                    left: playerWinnerPosition.left + 'px',
                                    top: playerWinnerPosition.top + 'px'
                                }, 1000, function() {
                                    $(this).remove();
                                });
                            });
                        }, 500); 
                    }

                    // round ended
                    if(isRoundOver) {
                        // delay turn start
                        setTimeout(function() {
                            // update game data
                            processGameData(gameData);

                            // deal cards
                            dealCards(lastGameState.Round); 
                        }, 3000); 
                    }
                    else {
                        // delay turn start
                        setTimeout(function() {
                            // update game data
                            processGameData(gameData);

                            // draw cards played
                            drawCardsPlayed();

                            // update player cards
                            drawPlayerCards();

                            // announce next players turn to act
                            startTurn();
                        }, 1500);
                    }
                });
        };

        // gameEnded
        hub.client.gameEnded = function (_player, gameData) {
            // broadcast win
            appendChatMessage("Server", "Game has ended. " + _player.Name + " won with " + _player.Score + " points!");

            // update winner scores
            $(".winner-points").html(_player.Score);
            $(".winner-name").html(_player.Name);

            // update profile picture
            if(_player.PictureURL.indexOf('http') > -1) {
                $(".winner-profile-picture").attr("src", _player.PictureURL);
            }
            
            // update ui
            processGameData(gameData);

            // update final scores table
            drawFinalScores();

            // show game ended modal
            $("#gameEndedModal").modal('show');
        };

        // gameCancelled
        hub.client.gameCancelled = function() {
            // alert cancelled
            alert('game cancelled by game host');

            // game cancelled
            window.location = "Home.aspx";
        }

        /*******************************************
         * functions that are called by the client *
         *******************************************/

        function quitGame() {
            if(isConnected) {
                // quit game
                hub.server.quitGame(gameId, currentPlayer.PlayerId);
            }

            // redirect
            window.location = 'Home.aspx';

            return false;
        };

        function cancelGame() {
            if(isConnected) {
                // cancel game
                hub.server.cancelGame(gameId, currentPlayer.PlayerId);
            }

            // redirect
            window.location = 'Home.aspx';

            return false;
        }

        function reconnect() {
            // reconnect to server
            $.connection.hub.start().done(onConnectionInit);
        };

        function joinGame(playerId, groupNameId, reconnected) {
            // call joinGameLobby on server
            hub.server.joinGame(playerId, gameId, groupNameId, reconnected)
                .done(function () {
                    logMessage("-- joinGame executed on server --");
                })
                .fail(function (msg) {
                    logMessage("-- " + msg + " --");
                });
        };

        function sendChatMessage() {
            logMessage("-- calling sendChatMessage on server --");

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
                .fail(function (msg) {
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

        function showToolTip(target, message) {
            // destroy any existing tooltips
            target.tooltip('destroy');

            // configure tooltip
            target.tooltip({
                title: message,
                placement: 'top',
                trigger: 'manual'
            });

            // show tool tip
            target.tooltip('show');

            // dont overlap modal
            target.css('z-index', 0);

            // destroy it after delay
            setTimeout(function() {
                target.tooltip('destroy');
            }, 3000);
        };

        function updatePlayerList(players) {
            console.log(players);
        };

        function getPlayerById(_playerId) {
            if(lastGameState == null)
                return null;

            var player = null;

            for(var y = 0; y < lastGameState.Players.length;y++) {
                current_player = lastGameState.Players[y];

                if(current_player.PlayerId == _playerId) {
                    player = current_player;

                    break;
                }
            }

            return player;
        };

        function getListOfPlayersInGame() {
            if (isConnected) {
                hub.server.listPlayersInGame(gameId, groupNameId);
            }
        };

        function sendKeepAlive() {
            if(isConnected) {
                // send keep-alive
                hub.server.keepAlive(currentPlayer.PlayerId, gameId, groupNameId);
            }
        };

        function getTotalBids() {
            var score_total = 0;

            for(var i = 0; i < lastGameState.Players.length; i++) {
                score_total += lastGameState.Players[i].Bid;
            }

            return score_total;
        }

        function updateGameState(gameState) {
            // update game state
            lastGameState = gameState;

            // players ref
            var players = lastGameState.Players;

            // update current Player
            if(lastGameState != null) {
                // update UI
                for(i = 0; i < players.length; i++) {
                    // player container
                    $playerDiv = $("#position-" + (i+1));

                    // player object
                    var player = players[i];

                    // update currentPlayer object
                    if(player.PlayerId == currentPlayer.PlayerId) {
                        // update current player object
                        currentPlayer = player;
                    }
                }
            }
        }

        function drawCardsPlayed() {
            if(lastGameState != null) {
                // draw cards played in middle
                var $cardsPlayed = $(".cards-played");

                // clear cards on table
                $cardsPlayed.html('');

                if(lastGameState.CardsPlayed != null && lastGameState.CardsPlayed.length > 0) {
                    for(i = 0; i < lastGameState.CardsPlayed.length; i++) {
                        var card = lastGameState.CardsPlayed[i];
                        var suitName = getSuitName(card.Suit);
                        var value = card.Value;
                        var ownerPlayerId = card.OwnerPlayerId;
                        var imageFileName = getCardImagePath(card.Suit, card.Value);

                        $cardsPlayed.append("<a id=\"" + card.Id + "\" suit=\"" + card.Suit + "\" value=\"" + card.Value + "\"><img src=\"" + imageFileName + "\" class='card' /></a>");
                    }
                }
            }
        };

        function updateTrump() {
            // update trump
            if(lastGameState.TrumpCard != null) {
                // determine trump
                if(lastGameState.TrumpCard.Suit == suit.Fluff) {
                    $(".trump").html("None");
                }
                else if(lastGameState.TrumpCard.Suit == suit.Wizard) {
                    // update trump value
                    $(".trump").html("Being chosen");
                }
                else {
                    // update trump value
                    $(".trump").html(getSuitName(lastGameState.TrumpCard.Suit)); 
                } 

                // update trump graphic
                updateTrumpCardGraphic(lastGameState.TrumpCard.Suit, lastGameState.TrumpCard.Value);
            }
            else {
                updateTrumpCardGraphic(lastGameState.SuitToFollow, 0);

                // update trump value
                $(".trump").html(getSuitName(lastGameState.SuitToFollow)); 
            }    
        };

        function updateEmptySeats(numPlayers) {
            if(numPlayers == null || isNaN(numPlayers))
                numPlayers = 0;

            // update un-used player containers
            for(i = numPlayers; i < 6; i++) {
                $playerDiv = $("#position-" + (i + 1)); 
                $playerDiv.find(".player-name").html('Empty seat');
                $playerDiv.css('opacity', '0.15');
                $playerDiv.children().css('opacity', '0.15');
            };
        };

        function processGameData(gameData) {
            // update game state
            updateGameState(gameData.GameStateData);

            // get vars
            var players = lastGameState.Players;
            var round = lastGameState.Round;
            var status = lastGameState.Status;
            var cardsPlayed = lastGameState.CardsPlayed;
            var dealerPositionIndex = lastGameState.DealerPositionIndex;
            var playerTurnIndex = lastGameState.PlayerTurnIndex;
            var lastToActIndex = lastGameState.LastToActIndex;
            var dateGameEnded = lastGameState.DateGameEnded;
            var i = 0;
            var numPlayers = 0;

            // update local variables
            playerList = players;

            // update total rounds
            var total_rounds = (60 / players.length);

            if(total_rounds > 15)
                total_rounds = 15; // capped at 15

            // upodate round #
            $(".round-info").html("Round: " + round + " of " + total_rounds);

            var total_bids = getTotalBids();
            var over_under = "";
            var bid_diff = Math.abs(total_bids - round);

            if(total_bids > round)
                over_under = "Overbid";
            else if(total_bids < round)
                over_under = "Underbid";
            else
                over_under = "Even";

            var bid_desc = "Bidding: " + over_under;

            if(total_bids > round || total_bids < round) {
                bid_desc += " by " + bid_diff; 
            }

            // update bid info
            $(".bid-info").html("- " + bid_desc);

            // update UI
            for(i = 0; i < players.length; i++) {
                // num players
                numPlayers++;

                // player container
                var $playerDiv = $("#position-" + (i+1));

                // player object
                var player = players[i];

                // update player name
                var playerName = player.Name;

                if(playerName.length > 12)
                    playerName = playerName.substring(0,11);

                $playerDiv.find(".player-name").html(playerName);

                // update profile pic
                if(player.PictureURL != null && (player.PictureURL.indexOf("http") > -1)) {
                    $playerDiv.find(".profile-pic").attr("src", player.PictureURL);
                }

                // update stats
                $playerDiv.find(".player-score").html(player.Score + " points");
                $playerDiv.find(".tricks-bid").html(player.Bid);
                $playerDiv.find(".tricks-taken").html(player.TricksTaken);

                // remove any existing classes
                $playerDiv.find(".player-name").removeClass("active dealer");

                if(player.IsDealer) {
                    $playerDiv.find(".player-name").addClass("dealer");
                }

                // add special label for players turn (except when current players turn)
                if(player.IsTurn) {
                    // change background of player name when its their turn
                    $playerDiv.find(".player-name").addClass("active");

                    // construct announcement message
                    var message = '';

                    if(lastGameState.Status == gameStateStatus.BiddingInProgress) {
                        message = player.Name + "'s turn to bid"; 
                    }
                    else if(lastGameState.Status == gameStateStatus.RoundInProgress) {
                        message = player.Name + "'s turn to play a card";
                    }
                    else if(lastGameState.Status == gameStateStatus.SelectTrump) {
                        message = player.Name + "'s turn to choose trump";
                    }
                    else {
                        message = player.Name + "'s turn!";
                    }

                    // only show tool tip for other players (not current player)
                    if(player.PlayerId != currentPlayer.PlayerId) {
                        // announce via tool tip
                        showToolTip($playerDiv, message);
                    }

                    // announce to chat window
                    appendChatMessage("Server", message);
                }  
            } 

            // update trump
            updateTrump();
            
            // check if first hand
            if(pageJustLoaded) {
                // update flag
                pageJustLoaded = false;

                // update cards played on table
                drawCardsPlayed();

                // update player cards
                drawPlayerCards();

                // update empty seats
                updateEmptySeats(numPlayers);

                // start turn
                startTurn();
            } 
        };

        function drawFinalScores() {
            // generate table html
            var scores_html = "<tr>";

            for(var i = 0; i < lastGameState.Players.length; i++) {
                scores_html += "<th>" + lastGameState.Players[i].Name + "</th>";
            }

            scores_html += "</tr>";
            scores_html += "<tr>";

            for(var i = 0; i < lastGameState.Players.length; i++) {
                scores_html += "<td>" + lastGameState.Players[i].Score + "</td>";
            }

            scores_html += "</tr>";

            // update table html
            $(".final-scores-table").html(scores_html);
        }

        function drawPlayerBidsHtml() {
            // draw player bids
            var playerBidsHtml = "<tr class=''>";
            
            // round
            playerBidsHtml += "<th class='text-center'>Round</th>";

            // player names
            for(var i = 0; i < lastGameState.Players.length; i ++) {
                var is_player_class = '';
                var player = lastGameState.Players[i];
                
                if(currentPlayer.PlayerId == player.PlayerId) {
                    is_player_class = 'danger';
                }

                if(player.IsDealer) {
                    playerBidsHtml += "<th class='text-center " + is_player_class + "'>" + player.Name + " (" + player.Score + ")</th>";
                }
                else {
                    playerBidsHtml += "<th class='text-center " + is_player_class + "'>" + player.Name + " (" + player.Score + ")</th>";
                }
                
            }
            
            playerBidsHtml += "</tr>";
            playerBidsHtml += "<tr>";
            
            // round
            playerBidsHtml += "<td class='text-center'><strong>" + lastGameState.Round + "</strong></td>";

            // player bids
            for(var i = 0; i < lastGameState.Players.length; i ++) {
                var is_player_class = '';

                if(currentPlayer.PlayerId == lastGameState.Players[i].PlayerId) {
                    is_player_class = 'danger';
                }

                playerBidsHtml += "<td class='text-center " + is_player_class + "'>" + lastGameState.Players[i].TricksTaken + "/" + lastGameState.Players[i].Bid + "</td>";
            }
  
            playerBidsHtml += "</tr>";

            return playerBidsHtml;
        }

        var isSelectingBid = false;

        // select bid
        function selectBid(round) {
            // validate
            if(!currentPlayer.IsTurn) {
                logMessage("-- you must wait your turn --");

                return false;
            }

            // check if window open already
            if(isSelectingBid)
                return;

            // play audio
            $(".soundStartTurn").trigger('play');

            // update flag
            isSelectingBid = true;

            // draw player bids
            var playerBidsHtml = drawPlayerBidsHtml();

            $(".player-bids").html(''); //clear player bids
            $(".player-bids").append(playerBidsHtml);

            // update total bids
            var total_bids = getTotalBids();

            $(".total-bids").html(total_bids);
            $(".round-number").html(lastGameState.Round);

            // generate bid # buttons
            $playerBid = $(".player-bid");

            // erase buttons
            $playerBid.html('');

            // add new button based on round #
            for(var i = 0; i <= lastGameState.Round; i++) {
                $playerBid.append("<a onclick=\"verifyBid(" + i + ");\" class=\"btn btn-lg btn-default\" style=\"width: 54px !important; margin:1px 1px;\">" + i + "</a>");
            }

            // show bid box
            $('#selectBidModal').modal('show');
        };

        var isSelectingCard = false;

        // select card to play
        function selectCard() {
            // validate
            if(!currentPlayer.IsTurn) {
                logMessage("-- you must wait your turn --");

                return false;
            }

            // check if already selecting card
            if(isSelectingCard)
                return;

            // play audio
            $(".soundStartTurn").trigger('play');

            // update flag
            isSelectingCard = true;

            // draw player bids
            var playerBidsHtml = drawPlayerBidsHtml();

            $(".player-bids").html(''); //clear player bids
            $(".player-bids").append(playerBidsHtml);

            // update total bids
            var total_bids = getTotalBids();

            $(".total-bids").html(total_bids);

            // first to act
            if(lastGameState.CardsPlayed == null) {
                $(".modal-cards-played").hide();
            }
            else {
                $(".modal-cards-played").show();
            }

            // show select card box
            $('#selectCardModal').modal('show');
        };

        var isSelectingTrump = false;

        function selectTrump() {
            // validate
            if(!currentPlayer.IsDealer) {
                logMessage("-- only the dealer can choose trump --");

                return;
            }
                
            if(lastGameState.Status != gameStateStatus.SelectTrump) {
                logMessage("-- you cant select trump right now --");

                return;
            }

            // check if already selecting trump
            if(isSelectingTrump)
                return;

            // play audio
            $(".soundStartTurn").trigger('play');

            // update trump
            isSelectingTrump = true;

            // show select trump modal
            $('#selectTrumpModal').modal('show');
        };

        function verifyBid(bidValue) {
            // validate
            if(!currentPlayer.IsTurn) {
                logMessage("-- you must wait your turn --");

                return false;
            }

            if(lastGameState.Status != gameStateStatus.BiddingInProgress) {
                logMessage("-- you cant play a card right now --");

                return;
            }

            if(bidValue != null && bidValue != NaN) {
                // hide modal
                $('#selectBidModal').modal('hide');

                // update flag
                isSelectingBid = false;

                // send msg to server
                if(isConnected) {
                    logMessage("-- enter bid --");

                    hub.server.enterBid(gameId, currentPlayer.PlayerId, bidValue, groupNameId)
                        .done(function() {
                            logMessage("-- enter bid executed on server --");
                        });
                }
            } 
        };

        function verifySelectedCard(selectedCard) {
            // validate
            if(!currentPlayer.IsTurn) {
                logMessage("-- you must wait your turn --");

                return;
            }

            if(lastGameState.Status != gameStateStatus.RoundInProgress) {
                logMessage("-- you cant play a card right now --");

                return;
            }

            var suitToFollow = lastGameState.SuitToFollow;
            var $card = $(selectedCard);
            var cardId = $card.attr("id");   
            var cardSuit = parseInt($card.attr("suit"));
            var cardValue = parseInt($card.attr("value"));

            // not player fluff or wizard
            if(cardSuit != suit.Fluff && cardSuit != suit.Wizard) {
                // not first to act
                if(lastGameState.CardsPlayed != null && lastGameState.CardsPlayed.length > 0) {
                    // if wizard is led, then allow any card to be played
                    if(lastGameState.CardsPlayed[0].Suit != suit.Wizard) {
                        // alert player to follow suit
                        if(suitToFollow != null && cardSuit != suitToFollow) {
                            // check that player can follow suit
                            for(var i = 0; i < currentPlayer.Cards.length; i++) {
                                if(currentPlayer.Cards[i].Suit == suitToFollow) {
                                    alert('You have to follow suit! Picked: ' + getSuitName(cardSuit) + ' - must be: ' + getSuitName(suitToFollow));

                                    return;
                                }
                            } 
                        }
                    }
                }
            }
            
            // hide modal
            $('#selectCardModal').modal('hide');

            // update flag
            isSelectingCard = false;

            logMessage("-- selected card: " + cardValue + " of " + getSuitName(cardSuit) + " (" + cardSuit + ")");

            // send data to server
            var cardObject = {
                Id: cardId,
                OwnerPlayerId: currentPlayer.PlayerId,
                Suit: cardSuit,
                Value: cardValue
            };

            if(isConnected) {
                hub.server.playCard(gameId, currentPlayer.PlayerId, cardObject, groupNameId);
            }
        };

        
        function verifySelectedTrump(suitId) {
            // validate
            if(!currentPlayer.IsDealer) {
                return;
            }

            if(!lastGameState.Status == gameStateStatus.SelectTrump) {
                return;
            }

            // hide select trump modal
            $('#selectTrumpModal').modal('hide');

            // update flag
            isSelectingTrump = false;

            if(isConnected) {
                hub.server.setTrump(gameId, currentPlayer.PlayerId, suitId, groupNameId);
            }
        };

        function startTurn() {
            // log
            logMessage("-- start turn: " + currentPlayer.IsTurn + " --");

            // select trump
            if(lastGameState.Status == gameStateStatus.SelectTrump) {
                // select trump
                selectTrump();
            }
                // enter bid
            else if(lastGameState.Status == gameStateStatus.BiddingInProgress) {
                // select bid
                selectBid(lastGameState.Round); 
            }
                // play card
            else if(lastGameState.Status == gameStateStatus.RoundInProgress) {
                // select card to play
                selectCard();
            }
            else {
                console.log("-- cant start turn - game state unknown: " + lastGameState.Status);
            }
        };

        function updateTrumpCardGraphic(_suit, _value) {
            // get file path
            var trumpFileName = getCardImagePath(_suit, _value);

            // update trump card graphic
            $(".trump-card").children("img").attr("src", trumpFileName);
        };

        function getPlayerDivByPlayerId(_playerId) {
            if(lastGameState != null) {
                for(var i = 0; i < lastGameState.Players.length; i++) {
                    if(lastGameState.Players[i].PlayerId == _playerId) {
                        return $("#position-" + (i + 1));
                    }
                }
            }
        }

        // num cards to deal
        var numCardsToDeal = 0;
        var dealtCardIndex = 0;
        var targetPlayerIndex = 0;

        function dealCards() {
            // update dealing flag
            isDealing = true;

            // reset indexes
            dealtCardIndex = 0;
            targetPlayerIndex = lastGameState.DealerPositionIndex + 1;

            // get num remaining cards to deal
            numCardsToDeal = (lastGameState.Round * lastGameState.Players.length);

            // deal card every 1/4th second
            dealRemainingCards();
        };

        function dealRemainingCards() {
            // dealer data
            var dealerIndex = lastGameState.DealerPositionIndex;
            var $dealerDiv = $(".cards-played-container");
            var dealerPosition = $dealerDiv.offset();

            // offset position
            dealerPosition.left = (dealerPosition.left + ($dealerDiv.width() / 2) - 32);
            dealerPosition.top = (dealerPosition.top);

            // validate
            if(numCardsToDeal < 0)
                numCardsToDeal = 0;

            // no cards left to deal - flip trump card and animate
            if(numCardsToDeal == 0) {
                // trump card data
                var $trumpDiv = $(".trump-card img");
                var trumpPosition = $trumpDiv.offset();
                var trumpCard = lastGameState.TrumpCard;

                var trumpFileName = "";
                
                if(trumpCard == null) {
                    trumpFileName = getCardImagePath(lastGameState.SuitToFollow, 0);
                }
                else {
                    trumpFileName = getCardImagePath(trumpCard.Suit, trumpCard.Value);
                }

                // spawn trump card at dealer position
                $("body").append("<img id='deal-card-trump' src='" + trumpFileName + "' class='deal-card card' style='position: absolute; left:" + dealerPosition.left + "px; top:" + dealerPosition.top + "px;' />");

                // animate trump card
                $("#deal-card-trump")
                    .animate({
                        'left': trumpPosition.left + 'px',
                        'top': trumpPosition.top + 'px',
                        'opacity': '0'
                    }, 1000, function() {
                        // remove spawned card
                        $(this).remove();

                        if(trumpCard == null) {
                            // trump was chosen, use generic suit card
                            updateTrumpCardGraphic(lastGameState.SuitToFollow, 0);
                        }
                        else {
                            // trump was decided by the turn
                            updateTrumpCardGraphic(trumpCard.Suit, trumpCard.Value);
                        }
                        
                        // disable dealing flag
                        isDealing = false;

                        // draw player cards
                        drawPlayerCards();

                        // delay turn start
                        setTimeout(function() {
                            // start turn
                            startTurn();
                        }, 2000);
                    });

                return;
            }

            // validate target player index
            if(targetPlayerIndex > (lastGameState.Players.length - 1))
                targetPlayerIndex = 0;

            // target player data
            var $targetPlayerDiv = $("#position-" + (targetPlayerIndex + 1));
            var targetPosition = $targetPlayerDiv.offset();

            // offset position
            targetPosition.left = ((targetPosition.left - 32) + ($targetPlayerDiv.width() / 2));
            targetPosition.top = (targetPosition.top + ($targetPlayerDiv.height() / 2));

            // spawn card at dealer location
            $("body").append("<img id='deal-card-" + dealtCardIndex + "' src='/Assets/Cards/deck_cover.png' class='deal-card' style='position: absolute; left:" + dealerPosition.left + "px; top:" + dealerPosition.top + "px;' />");
             
            // get deal speed based on rounds
            var animation_speed = (lastGameState.Round > 4) ? 230 : 250;

            // 12 rounds and up, increase deal speed
            if(lastGameState.Round > 8)
                animation_speed -= 15;

            // 12 rounds and up, increase deal speed
            if(lastGameState.Round > 11)
                animation_speed -= 18;

            // 16 rounds and up, increase deal speed
            if(lastGameState.Round > 15)
                animation_speed -= 20;


            // animate dealt card
            $("#deal-card-" + dealtCardIndex)
                .animate({
                    'left': targetPosition.left + 'px',
                    'top': targetPosition.top + 'px',
                    'opacity':'0.7'
                }, animation_speed, function() {
                    // remove card
                    $(this).remove();

                    // amimation comete - deal next card
                    dealRemainingCards();
                });

            // indexes
            targetPlayerIndex++;
            dealtCardIndex++;
            numCardsToDeal--;
        };  
        
        function drawPlayerCards() {
            // draw player cards
            var $playerCards = $(".player-cards");

            // clear existing cards
            $playerCards.html('');

            console.log("clearing player cards in hand");

            // is player able to follow suit
            var canPlayerFollowSuit = false;
            var player;
            var i;
            var card;

            // player is not first to act - check if we can follow suit
            if(lastGameState.SuitToFollow != suit.None 
                && lastGameState.SuitToFollow != suit.Wizard
                && lastGameState.CardsPlayed != null 
                && lastGameState.CardsPlayed.length > 0) {
                // loop through player cards
                for(i = 0; i < currentPlayer.Cards.length; i++) {
                    if(lastGameState.SuitToFollow != null) {
                        // card ref
                        card = currentPlayer.Cards[i];
                    
                        // check if card can follow suit
                        if(card.Suit == lastGameState.SuitToFollow){
                            canPlayerFollowSuit = true;

                            break;
                        }
                    }
                }
            }
            
            // loop through cards
            for(i = 0; i < currentPlayer.Cards.length; i++) {
                card = currentPlayer.Cards[i];

                var suitName = getSuitName(card.Suit);
                var value = card.Value;
                var ownerPlayerId = card.OwnerPlayerId;
                var imageFileName = getCardImagePath(card.Suit, card.Value);

                var style = "";
                var fullCardName = "";
                var cardValueName = "";

                if(card.Value == 11) {
                    cardValueName = "Jack";
                }
                else if(card.Value == 12) {
                    cardValueName = "Queen";
                }
                else if(card.Value == 13) {
                    cardValueName = "King";
                }
                else if(card.Value == 14) {
                    cardValueName = "Ace";
                }
                else {
                    cardValueName = card.Value;
                }

                if(card.Suit == suit.Wizard) {
                    fullCardName = "Wizard";
                }
                else if(card.Suit == suit.Fluff) {
                    fullCardName = "Fluff";
                }
                else {
                    fullCardName = cardValueName + " of " + getSuitName(card.Suit);
                }

                var suitToFollow = lastGameState.SuitToFollow;
                var is_not_playable_class = '';
                var is_playable = false;

                // player is able to follow suit from previous check
                if(canPlayerFollowSuit) {
                    // wizards and fluffs are playable
                    if(card.Suit == suit.Wizard || card.Suit == suit.Fluff) {
                        is_playable = true;
                    }
                    else {
                        // found playable card
                        if(card.Suit == lastGameState.SuitToFollow) {
                            is_playable = true;
                        }
                    }  
                }
                else {
                    // player cant follow suit - card is playable
                    is_playable = true;
                }        
                
                if(i > 0) {
                    style = "style='margin-left: -48px'";
                }

                if(is_playable) {
                    // card is playable
                    $playerCards.append("<a onclick='verifySelectedCard(this);' id='" + card.Id + "' suit='" + card.Suit + "' value='" + card.Value + "' " + style + " title='" + fullCardName + "'><img src=\"" + imageFileName + "\" class=\"card\" /></a>");
                }
                else {
                    // card is not playable
                    $playerCards.append("<a title='" + fullCardName + "' class='unplayable'" + style + "><img src=\"" + imageFileName + "\" class=\"card\" /></a>");
                }
            }

            if(!canPlayerFollowSuit) {
                // if player cant follow suit, all cards are playable
                $playerCards.find("a").removeClass("unplayable");
            }

            // remove onclick attr
            $("card-holder .player-cards").removeAttr("onclick");
        };

        function pauseGame() {
            $("#gamePausedModal").modal('show');
        };

        function resumeGame() {
            $("#gamePausedModal").modal('hide');
        };
    </script>
    <!--[if gte IE 9]>
      <style type="text/css">
        .gradient {
           filter: none;
        }
      </style>
    <![endif]-->
</asp:Content>
<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <h1 class="game-info">
            <span class="round-info"></span>
            <span class="bid-info"></span>
            <span class="pull-right">Trump:
                <span class="trump label label-danger" style="top: 0px;">Loading</span>
            </span>
        </h1>
        <div class="game-board">
            <table class="game-board-table">
                <tr>
                    <td>&nbsp;</td>
                    <td>
                        <div id="position-1" class="player-container">
                            <div class="player-details">
                                <div class="player-name">Player 1</div>
                                <div class="player-tricks">
                                    <span class="tricks-taken">0</span>
                                    of
                                    <span class="tricks-bid">0</span>
                                    tricks
                                </div>
                                <div class="player-score">0 points</div>
                            </div>
                            <div class="player-profile-image">
                                <img data-src="holder.js/64x64" class="profile-pic" />
                            </div>
                        </div>
                    </td>
                    <td>
                        <div id="position-2" class="player-container">
                            <div class="player-details">
                                <div class="player-name">Player 2</div>
                                <div class="player-tricks">
                                    <span class="tricks-taken">0</span>
                                    of
                                    <span class="tricks-bid">0</span>
                                    tricks
                                </div>
                                <div class="player-score">0 points</div>
                            </div>
                            <div class="player-profile-image">
                                <img data-src="holder.js/64x64" class="profile-pic" />
                            </div>
                        </div>
                    </td>
                    <td></td>
                </tr>
                <tr>
                    <td>
                        <div id="position-6" class="player-container">
                            <div class="player-details">
                                <div class="player-name">Player 6</div>
                                <div class="player-tricks">
                                    <span class="tricks-taken">0</span>
                                    of
                                    <span class="tricks-bid">0</span>
                                    tricks
                                </div>
                                <div class="player-score">0 points</div>
                            </div>
                            <div class="player-profile-image">
                                <img data-src="holder.js/64x64" class="profile-pic" />
                            </div>
                        </div>
                    </td>
                    <td colspan="2">
                        <div class="cards-played cards-played-container">
                            <!-- place holder for cards played -->
                        </div>
                    </td>
                    <td>
                        <div id="position-3" class="player-container">
                            <div class="player-details">
                                <div class="player-name">Player 3</div>
                                <div class="player-tricks">
                                    <span class="tricks-taken">0</span>
                                    of
                                    <span class="tricks-bid">0</span>
                                    tricks
                                </div>
                                <div class="player-score">0 points</div>
                            </div>
                            <div class="player-profile-image">
                                <img data-src="holder.js/64x64" class="profile-pic" />
                            </div>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td>
                        <div id="position-5" class="player-container">
                            <div class="player-details">
                                <div class="player-name">Player 5</div>
                                <div class="player-tricks">
                                    <span class="tricks-taken">0</span>
                                    of
                                    <span class="tricks-bid">0</span>
                                    tricks
                                </div>
                                <div class="player-score">0 points</div>
                            </div>
                            <div class="player-profile-image">
                                <img data-src="holder.js/64x64" class="profile-pic" />
                            </div>
                        </div>
                    </td>
                    <td>
                        <div id="position-4" class="player-container">
                            <div class="player-details">
                                <div class="player-name">Player 4</div>
                                <div class="player-tricks">
                                    <span class="tricks-taken">0</span>
                                    of
                                    <span class="tricks-bid">0</span>
                                    tricks
                                </div>
                                <div class="player-score">0 points</div>
                            </div>
                            <div class="player-profile-image">
                                <img data-src="holder.js/64x64" class="profile-pic" />
                            </div>
                        </div>
                    </td>
                    <td>&nbsp;</td>
                </tr>
            </table>
        </div>
        <div class="card-holder row">
            <div class="player-cards col-xs-9"></div>
            <div class="trump-card-container col-xs-3 text-right">
                <a class="trump-card">
                    <img src="Assets/Cards/deck_cover.png" class="card" alt="trump card" />
                </a>
            </div>
        </div>
        <div class="clearfix"></div>
    </div>
    <hr />
    <div class="container">
        <div class="panel panel-default">
            <div class="panel-heading">Chat window</div>
            <div class="panel-body">
                <div class="form-group">
                    <textarea id="txtChatWindow" name="txtChatWindow" class="form-control" rows="8" readonly></textarea>
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
                        <input type="text" id="txtChatMessage" name="txtChatMessage" class="form-control" placeholder="Chat message" />
                        <span class="input-group-btn">
                            <!--input type="button" id="btnClearChat" name="btnClearChat" class="btn btn-default" value="Clear" onclick="clearChatWindow(); return false;" /-->
                            <input type="button" id="btnSendChat" name="btnSendChat" class="btn btn-primary" value="Send" onclick="sendChatMessage(); return false;" />
                            <script type="text/javascript">
                                $(document).ready(function() {
                                    // bind enter event to chat text box
                                    $("#txtChatMessage").bind("keypress", function (event) {
                                        // enter key pressed
                                        if (event.keyCode == 13) {
                                            sendChatMessage();

                                            return false;
                                        }
                                    });
                                });
                            </script>
                        </span>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <hr />
    <div class="modal" id="selectBidModal" tabindex="-1" role="dialog" aria-labelledby="selectBidModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title" id="selectBidModalLabel">Your turn to bid!
                        <span class="pull-right">Trump:
                            <span class="trump label label-danger" style="top: 0px;">Loading</span>
                        </span>
                    </h4>
                </div>
                <div class="modal-body">
                    <div class="panel panel-default" style="margin-bottom: 0px;">
                        <div class="panel-heading player-bids-title"><strong>Player bids - <span class="total-bids">0</span> of <span class="round-number">0</span></strong></div>
                        <table class="table table-responsive player-bids"></table>
                    </div>
                    <div class="modal-cards-played" style="margin-top: 10px; border: 1px solid #ccc; padding: 10px; border-radius: 5px;">
                        <div class="player-cards"></div>
                    </div>
                </div>
                <div class="modal-footer">
                    <div class="player-bid">
                        <!-- placeholder for buttons -->
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="selectCardModal" tabindex="-1" role="dialog" aria-labelledby="selectCardModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title" id="selectCardModalLabel">Your turn to play a card!
                        <span class="pull-right">Trump:
                            <span class="trump label label-danger" style="top: 0px;">Loading</span>
                        </span>
                    </h4>
                </div>
                <div class="modal-body">
                    <div>
                        <div class="panel panel-default" style="margin-bottom: 0px;">
                            <div class="panel-heading"><strong>Player bids - <span class="total-bids">0</span> of <span class="round-number">0</span></strong></div>
                            <table class="table table-responsive player-bids"></table>
                        </div>
                    </div>
                    <div class="modal-cards-played" style="margin-top: 10px; border: 1px solid #ccc; padding: 10px; border-radius: 5px;">
                        <div class="cards-played"></div>
                    </div>
                </div>
                <div class="modal-footer" style="text-align: left;">
                    <div class="player-cards" style="padding: 10px; background-color: #d8d8d8; border: 1px solid #ccc; border-radius: 5px;"></div>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="selectTrumpModal" tabindex="-1" role="dialog" aria-labelledby="selectTrumpModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title" id="selectTrumpModalLabel">Please select Trump for this round</h4>
                </div>
                <div class="modal-body">
                    <div class="player-cards well well-sm"></div>
                </div>
                <div class="modal-footer">
                    <div>
                        <a class="btn btn-default btn-lg btn-block" onclick="verifySelectedTrump(suit.Spades);">Spades</a>
                        <a class="btn btn-default btn-lg btn-block" onclick="verifySelectedTrump(suit.Clubs);">Clubs</a>
                        <a class="btn btn-default btn-lg btn-block" onclick="verifySelectedTrump(suit.Hearts);">Hearts</a>
                        <a class="btn btn-default btn-lg btn-block" onclick="verifySelectedTrump(suit.Diamonds);">Diamonds</a>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="gameEndedModal" tabindex="-1" role="dialog" aria-labelledby="gameEndedModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title" id="gameEndedModalLabel">Game has ended!</h4>
                </div>
                <div class="modal-body">
                    <div class="form-group">
                        <h3 style="margin-top: 0px;">
                            <span class="glyphicon glyphicon-thumbs-up"></span>
                            Congratulations, <span class="winner-name" style="text-transform: capitalize;">Player</span>!
                        </h3>
                        <div class="media">
                            <a class="media-left" href="#">
                                <img src="holder.js/64x64/" class="winner-profile-picture img-thumbnail">
                            </a>
                            <div class="media-body">
                                <h4 class="media-heading">
                                    <span class="winner-name" style="text-transform: capitalize;"></span>
                                    won the game with <span class="winner-points">0</span> points!
                                </h4>
                                <a href="HostGame.aspx">Start a new game</a> or look <a href="Home.aspx">for an existing game lobby</a>.
                            </div>
                        </div>
                    </div>
                    <div class="panel panel-default" style="margin-bottom: 0px;">
                        <div class="panel-heading">
                            <strong>Final scores</strong>
                        </div>
                        <table class="table table-responsive final-scores-table"></table>
                    </div>

                </div>
                <div class="modal-footer">
                    <button class="btn btn-lg btn-primary" onclick="window.location='Home.aspx'; return false;">
                        Exit game
                    </button>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="gamePausedModal" tabindex="-1" role="dialog" aria-labelledby="gamePausedModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title" id="gamePausedModalLabel">Game paused!</h4>
                </div>
                <div class="modal-body">
                    <h4 style="margin-top: 0px;">Not enough players to continue.</h4>
                    <p>Game will automatically resume once all players have connected.</p>
                </div>
                <div class="modal-footer">
                    <%
                        // game host may cancel game
                        if (Game.OwnerPlayerId == PlayerData.PlayerId)
                        {
                            // show cancel game button if game host
                            Response.Write("<button class=\"btn btn-primary\" onclick=\"cancelGame(); return false;\">Cancel game</button>");
                        }
                        else
                        {
                            // show quit button
                            Response.Write("<button class=\"btn btn-primary\" onclick=\"quitGame(); return false;\">Quit game</button>");
                        }
                    %>
                </div>
            </div>
        </div>
    </div>
    <div class="offline-message" style="display: none;">
        <input type="button" id="btnReconnect" onclick="reconnect();" value="Reconnect" class="btn btn-primary btn-block" style="height: 50%;" />
        <input type="button" id="btnQuit" onclick="quitGame();" value="Quit game" class="btn btn-default btn-block" style="height: 50%;" />
    </div>
    <script type="text/javascript">
        $(document).ready(function() {
            $(".modal").modal({
                backdrop: 'static',
                keyboard: false,
                show: false
            });

            // update offline message size
            $(".offline-message").css({
                height: $(window).height(),
                width: $(window).width()
            });

            // load game audio
            $(".gameAudio").trigger('load');
        });

        // update offline message size when window resizes
        $(window).resize(function() {
            // update offline message size
            $(".offline-message").css({
                height: $(window).height(),
                width: $(window).width()
            });
        });
    </script>
    <audio class="soundStartTurn" preload="auto" style="display: none;">
        <source src="Assets/Sounds/beep.mp3" type="audio/mpeg">
    </audio>
    <audio class="soundCardPlayed" preload="auto" style="display: none;">
        <source src="Assets/Sounds/card-played.mp3" type="audio/mpeg">
    </audio>
</asp:Content>
