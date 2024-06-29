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

var movecounter = 0;

var nextPlayer;

var robotOneName = document.getElementById("labelRobotOne").textContent;
var robotTwoName = document.getElementById("labelRobotTwo").textContent;


connection.start().then(function () {
    //var robotOne = document.getElementById("labelRobotOne");

    //var robotOneName = robotOne.textContent;
    nextPlayer = document.getElementById("labelRobotOne").textContent;
    connection.invoke("MakeFirstMove", robotOneName);
});

connection.on("AnimateMove", function (columnNr) {
    // wie finde ich heraus wer playeROne ist?
    movecounter++;

    var color = movecounter % 2 != 0 ? "red" : "yellow";

    animate(columnNr, colDepth[columnNr], color);

    if (nextPlayer == robotOneName) {
        nextPlayer = robotTwoName;
    } else if (nextPlayer == robotTwoName) {
        nextPlayer = robotOneName;
    }

    document.getElementById("nextTurnName").textContent = "Next Turn: " + nextPlayer;

    colDepth[columnNr] = colDepth[columnNr] - 1;
});

function animate(column, endRow, color) {
    if (color == "yellow") {
        var selectedCell = document.getElementById(`${column}${endRow}`);
        if (selectedCell != null) {
            selectedCell.style.backgroundColor = "yellow";
        }
    }
    else if (color == "red") {
        var selectedCell = document.getElementById(`${column}${endRow}`);
        if (selectedCell != null) {
            selectedCell.style.backgroundColor = "red";
        }
    }
}

connection.on("NotificateGameEnd", function (winnerId) {
    console.log(`Gratuliere ${winnerId}!! Du hast gewonnen!`);
    showGameOverModal(winnerId);
});

async function showGameOverModal(winnerId) {
    const modal = document.getElementById("gameoverModal");
    const label = document.getElementById("modalLabel");
    label.innerText = label.innerText + winnerId;
    modal.style.display = "block";

    // When player wants to go back to lobby
    document.getElementById("confirmButton").onclick = function () {
        modal.style.display = "none";
        const baseUrl = "https://localhost:7102/Home/GameLobby";
        window.location.href = `${baseUrl}`;
    }
 }