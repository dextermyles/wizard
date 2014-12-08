delete from GameLobby WHERE GameLobby.GameLobbyId IN (SELECT GameLobbyId FROM Game WHERE DateCompleted IS NULL);
delete from GameLobbyPlayers WHERE GameLobbyPlayers.GameLobbyId IN (SELECT GameLobbyId FROM Game WHERE DateCompleted IS NULL);
delete from GamePlayers WHERE GamePlayers.GameId IN (SELECT GameId FROM Game WHERE DateCompleted IS NULL);
delete from Game WHERE Game.DateCompleted IS NULL;
