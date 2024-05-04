"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

connection.on("ReceiveAvailableUsers", function (players) {

    for (var i = 0; i < players.length; i++) {

        var li = document.createElement("li");

        var anchor = document.createElement("a");

        var playerOne = document.getElementById("userNameLabel").textContent;
        var playerTwo = players[i];
        li.id = playerTwo;

        // Setting up URL Params
        const baseUrl = "https://localhost:7102/Game/Board";
        const params = new URLSearchParams();
        params.append("playerOne", playerOne);
        params.append("playerTwo", playerTwo);

        const gameUrl = `${baseUrl}?${params.toString()}`;

        anchor.textContent = `${playerTwo}`;
        anchor.onclick = function () {
            connection.invoke("NotificateGameStart", playerOne, playerTwo, gameUrl).catch(function (err) {
                return console.error(err.toString());
            }).catch(function (err) {
                return console.error(err.toString());
            });
        };

        li.appendChild(anchor);

        document.getElementById("playerList").appendChild(li);
    }
    console.log("Online Users wurden erfolgreich in Liste geladen.");
});

connection.on("ReceiveNewUser", function (user) {
    console.log("inside ReceiveNewUser");

        var li = document.createElement("li");

        li.id = user;

        var anchor = document.createElement("a");

        var playerOne = document.getElementById("userNameLabel").textContent;
        var playerTwo = user;

        // Setting up URL Params
    const baseUrl = "https://localhost:7102/Game/Board";
        const params = new URLSearchParams();
        params.append("playerOne", playerOne);
    params.append("playerTwo", playerTwo);
    
    const gameUrl = `${baseUrl}?${params.toString()}`;
    anchor.textContent = `${user}`;

    anchor.onclick = function () {
        connection.invoke("NotificateGameStart", playerOne, playerTwo).catch(function (err) {
            return console.error(err.toString());
        }).catch(function (err) {
            return console.error(err.toString());
        });
    };
        li.appendChild(anchor);

        document.getElementById("playerList").appendChild(li);
});

connection.start().then(function () {
    var username = document.getElementById("userNameLabel").textContent;

    connection.invoke("GetAvailableUsers").catch(function (err) {
        return console.error(err.toString());
    }).catch(function (err) {
        return console.error(err.toString());
    });


    connection.invoke("AddUser", username).catch(function (err) {
        return console.error(err.toString());
    }).catch(function (err) {
        return console.error(err.toString());
    });

    connection.invoke("SendNotification", username).catch(function (err) {
        return console.error(err.toString());
    }).catch(function (err) {
        return console.error(err.toString());
    });
});

connection.on("PlayerLeft", (userName) => {
    var liElement = document.getElementById(userName);
    liElement.remove();

    console.log(`${userName} left the lobby.`);
});

window.addEventListener("beforeunload", () => {
    var username = document.getElementById("userNameLabel").textContent;
    connection.invoke("LeaveLobby", username);
});

connection.on("NavigateToGame", (playerOne, playerTwo, gameUrl) => {
    //Möglichkeit um beide Spieler ins selbe Spiel zu lotsen
   /**
    * Erste Möglichkeit. Wir suchen in der DB nach einem Spiel in dem beide Spieler drinn sind und schauen den Flas isGame done an? Da beide Spieler nicht in zwei Spielen gleichzeitig sein können die laufen.e
    */
   var passtUrl = gameUrl;
    const baseUrl = "https://localhost:7102/Game/Board";
    const params = new URLSearchParams();
    params.append("playerOne", playerOne);
    params.append("playerTwo", playerTwo);

    window.location.href = `${baseUrl}?${params.toString()}`;
    
});