"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/boardEvEHub").build();

let colDepth = {};

colDepth['1'] = 6;
colDepth['2'] = 6;
colDepth['3'] = 6;
colDepth['4'] = 6;
colDepth['5'] = 6;
colDepth['6'] = 6;
colDepth['7'] = 6;


connection.start().then(function () {
    var robotOne = document.getElementById("labelRobotOne");

    var robotOneName = robotOne.textContent;
    connection.invoke("MakeFirstMove", robotOneName);
});

connection.on("AnimateMove", function (robotName, columnNr, gameId, color) {
    // wie finde ich heraus wer playeROne ist?
    animate(columnNr, colDepth[columnNr], color)
});





function animate(column, endRow, color) {
    if (color == "yellow") {
        for (let row = 1; row <= endRow; row++) {
            var selectedCell = document.getElementById(`${column}${row}`);
            if (selectedCell != null) {
                if (row == endRow) {
                    selectedCell.style.backgroundColor = "yellow";
                }
            }
        }
    }
    else if (color == "red") {
        for (let row = 1; row <= endRow; row++) {
            var selectedCell = document.getElementById(`${column}${row}`);
            if (selectedCell != null) {
                if (row == endRow) {
                    selectedCell.style.backgroundColor = "red";
                }
            }
        }
    }
}


// Sobald spiel anfängtr
/**
 * 
 * 1. RobotOne Fängt an
 * -   Methode wird per MQTT an Roboter versendet.
 * - Dieser führt zug aus
 * - und 
 * 
 * 
 * 
 */

