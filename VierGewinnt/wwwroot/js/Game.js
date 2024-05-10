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


// Diese MEthode nimmt schickt den nächsten Zug an Clients für die Animation.
connection.on("SendPlayerMove", (message) => {
    
    console.log(message);
});


// Erlaubt dem berechtigten Spieler den nächsten Zug zu machen. (UI wird freigegeben)
connection.on("AllowNextMove", (message) => {
    console.log(message);
});


//var button = document.getElementById("btnYellow");
//button.onclick = function () {
//    event.preventDefault();
//    var column = document.getElementById("colNumberYellow").value;

//    // auf Board.cshtl haben wir GameViewModel

//    // Wir können hier die ID des Boards als Param mitgeben, Player ID wäre auch im GameviewModle drin, ZugNr können wir noch im GameViewmodel adden.


//    connection.invoke("SendPlayerMove", column).catch(function (err) {
//        return console.error(err.toString());
//    }).catch(function (err) {
//        return console.error(err.toString());
//    });
//};