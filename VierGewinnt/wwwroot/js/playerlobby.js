"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/playerlobbyHub").build();

connection.on("ReceiveAvailableUsers", function (players) {

    for (var i = 0; i < players.length; i++) {
        createListelement(players[i]);
    }
});

connection.on("ReceiveNewUser", function (playerTwo) {
    createListelement(playerTwo);
});

connection.start().then(function () {
    var username = document.getElementById("userNameLabel").textContent;
    connection.invoke("GetAvailableUsers");
    connection.invoke("AddUser", username);
    connection.invoke("SendNotification", username);
});

connection.on("PlayerLeft", (userName) => {
    var liElement = document.getElementById(userName);
    liElement.remove();
});

window.addEventListener("beforeunload", () => {
    var username = document.getElementById("userNameLabel").textContent;
    connection.invoke("LeaveLobby", username);
});

connection.on("NavigateToGame", (gameId) => {
    const baseUrl = "https://localhost:7102/Game/Board";
    const params = new URLSearchParams();
    params.append("gameId", gameId);

    window.location.href = `${baseUrl}?${params.toString()}`;
});

// JSCode END

// FUNCTIONS START

// Wahrscheinlich später löschen brauchts nicht mehr.

function createBoardEntity(playeroneName, playertwoName) {
    event.preventDefault();

    $.ajax({
        url: "/Game/CreateGame",
        type: 'GET',
        data: { "playerone": playeroneName, "playertwo": playertwoName },
        success: function (result) {
            console.log("SpielErstellt");
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function createListelement(playerTwo) {
    var li = document.createElement("li");
    var anchor = document.createElement("a");
    var playerOne = document.getElementById("userNameLabel").textContent;

    li.id = playerTwo;
    anchor.textContent = `${playerTwo}`;

    anchor.onclick = function () {
        event.preventDefault();
        createBoardEntity(playerOne, playerTwo);
        connection.invoke("NotificateGameStart", playerOne, playerTwo);
    }

    li.appendChild(anchor);

    document.getElementById("playerList").appendChild(li);
}
