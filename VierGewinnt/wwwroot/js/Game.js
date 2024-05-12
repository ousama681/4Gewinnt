"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();
connection.start();

connection.on("AnimatePlayerMove", (column, playerId) => {
    event.preventDefault();

    // Hier die Logik einbauen um f�r den Spieler der dran ist die Buttons zu enablen.
    console.log("Spielzug auf Spalte: " + column);
});


window.onload = function () {
            var YellowBtn = document.getElementById("btnColYellow");
            YellowBtn.onclick = function () {
                event.preventDefault();

                var arr = $('form').serializeArray();
                var dataObj = {};

                $(arr).each(function(i, field){
                    dataObj[field.name] = field.value;
                });

                var playerIdOne = dataObj['userIdOne'];
                var boardIdOne = dataObj['boardIdOne'];
                var columnYellow = dataObj['colNumberYel'];

                connection.invoke("SendPlayerMove", playerIdOne, boardIdOne, columnYellow);
            }

    var RedBtn = document.getElementById("btnColRed");

            RedBtn.onclick = function () {
                event.preventDefault();

                var arr = $('form').serializeArray();
                var dataObj = {};

                $(arr).each(function (i, field) {
                    dataObj[field.name] = field.value;
                });

                var playerIdTwo = dataObj['userIdTwo'];
                var boardIdTwo = dataObj['boardIdTwo'];
                var columnRed = dataObj['colNumberRed'];

                connection.invoke("SendPlayerMove", playerIdTwo, boardIdTwo, columnRed);
            }
}