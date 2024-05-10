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


connection.on("NavigateToGame", (playerOne, playerTwo) => {
    const baseUrl = "https://localhost:7102/Game/Board";
    const params = new URLSearchParams();
    params.append("playerOne", playerOne);
    params.append("playerTwo", playerTwo);

    window.location.href = `${baseUrl}?${params.toString()}`;
});

// JSCode END

// FUNCTIONS START

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


// THIS PART WAS IN ReceiveNewUser

//    var li = document.createElement("li");

//    li.id = user;

//    var anchor = document.createElement("a");

//    var playerOne = document.getElementById("userNameLabel").textContent;
//var playerTwo = user;

//anchor.textContent = `${user}`;

//anchor.onclick = function () {
//    event.preventDefault();
//    createBoardEntity(playerOne, playerTwo);
//    connection.invoke("NotificateGameStart", playerOne, playerTwo);


//}
//    li.appendChild(anchor);

//document.getElementById("playerList").appendChild(li);

// THIS PART WAS IN ReceiveNewUser END

// THIS PART IS FROM NavigateToGame 

//connection.on("NavigateToGame", (playerOne, playerTwo) => {
//    //Möglichkeit um beide Spieler ins selbe Spiel zu lotsen
//   /**
//    * Erste Möglichkeit. Wir suchen in der DB nach einem Spiel in dem beide Spieler drinn sind und schauen den Flas isGame done an? Da beide Spieler nicht in zwei Spielen gleichzeitig sein können die laufen.e
//    */
//    const baseUrl = "https://localhost:7102/Game/Board";
//    const params = new URLSearchParams();
//    params.append("playerOne", playerOne);
//    params.append("playerTwo", playerTwo);


//    window.location.href = `${baseUrl}?${params.toString()}`;
//});

// THIS PART IS FROM NavigateToGame END
