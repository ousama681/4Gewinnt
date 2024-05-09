"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();


connection.on("ReceiveMessage", function (message) {
    // Animationsz�g

    console.log("Der Spielzug wird animiert");

    document.getElementById("robotStatus").textContent = "Der Spielzug wird animiert";





});

connection.start().then(function () {
    console.log("Wir sind im GameJS drin ak, GameHub funktioniert.")
});



var button = document.getElementById("btnYellow");
button.onclick = function () {
    event.preventDefault();
    var column = document.getElementById("colNumberYellow").value;
    connection.invoke("SendPlayerMove", column).catch(function (err) {
        return console.error(err.toString());
    }).catch(function (err) {
        return console.error(err.toString());
    });
};