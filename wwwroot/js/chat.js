"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

console.log("outside of Connection Methods");

var url = "";
var firstVisit = true;

//Disable the send button until connection is established.
/*document.getElementById("sendButton").disabled = true;*/


connection.on("ReceiveAvailableUsers", function (players) {
    console.log("inside receiveAvailableUsers");
    console.log("Online Users werden in Liste geladen.");

    for (var i = 0; i < players.length; i++) {

        var li = document.createElement("li");
        
        li.id = playerTwo;

        var anchor = document.createElement("a");

        var playerOne = document.getElementById("userNameLabel").textContent;
        var playerTwo = players[i];

        // Setting up URL Params
        const baseUrl = "https://localhost:7102/";
        const params = new URLSearchParams();
        params.append("playerOne", playerOne);
        params.append("playerTwo", playerTwo);
    
        anchor.href = `${baseUrl}?${params.toString()}`;
        anchor.textContent = `${playerTwo}`;

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
        const baseUrl = "https://localhost:7102/";
        const params = new URLSearchParams();
        params.append("playerOne", playerOne);
        params.append("playerTwo", playerTwo);
    
        anchor.href = `${baseUrl}?${params.toString()}`;
        anchor.textContent = `${user}`;

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



// Hier verl�sst de rUser die Verbindung und benachrigtigt alle anderen, damit er aus der Liste verschwindet

//connection.stop().then(function () {
//    var username = document.getElementById("userNameLabel").textContent;

//    connection.invoke("NotificateRemovePlayer", username).catch(function (err) {
//        return console.error(err.toString());
//    }).catch(function (err) {
//        return console.error(err.toString());
//    });
//});


//connection.on("PrepareRemovePlayer", function () {
//    console.log("user m�chte lobby verlassen");

//    var username = document.getElementById("userNameLabel").textContent;

//    connection.invoke("NotificateRemovePlayer", username).catch(function (err) {
//        return console.error(err.toString());
//    }).catch(function (err) {
//        return console.error(err.toString());
//    });
//    console.log("user von lobby entfernen und notification an andere schicken");
//});

//window.onbeforeunload = function () {
//    var newUrl = window.location.href.split('?')[0]; 
//    var username = document.getElementById("userNameLabel").textContent;

//    connection.invoke("NotificateRemovePlayer", username).catch(function (err) {
//        return console.error(err.toString());
//    }).catch(function (err) {
//        return console.error(err.toString());
//    });

//    console.log("User " + username + " left GameLobby URL");
//}


//window.onload = function () {
//    url = window.location.href.split('?')[0];
//    firstVisit = false;
//}
//connection.on("ReceivePlayerRemoval", function (user) {
//    var li = document.createElement("li");
//    li.textContent = user;
//    document.getElementById("playerList").removeChild(li);
//});

// When the page is about to unload, leave the lobby
