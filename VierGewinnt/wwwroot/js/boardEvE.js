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


connection.start().then(function () {
    var robotOne = document.getElementById("labelRobotOne");

    var robotOneName = robotOne.textContent;
    connection.invoke("MakeFirstMove", robotOneName);
});

connection.on("AnimateMove", function (columnNr) {
    // wie finde ich heraus wer playeROne ist?
    movecounter++;

    var color = movecounter % 2 != 0 ? "red" : "yellow";

    animate(columnNr, colDepth[columnNr], color);

    colDepth[columnNr] = colDepth[columnNr] - 1;
});


connection.on("NotificateGameEnd", function (winnerId) {
    console.log(`Gratuliere ${winnerId}!! Du hast gewonnen!`);
    showPlayerOneChallengeModal(winnerId);
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

async function showPlayerOneChallengeModal(winnerId) {
    const winner = winnerId;
    const modal = document.getElementById("gameoverModal");
    const label = document.getElementById("modalLabel");
    label.innerText = label.innerText + winner;
    const timer = document.getElementById("timer");
    let playerResponded = false;

    modal.style.display = "block";

    // When player wants to go back to lobby
    document.getElementById("confirmButton").onclick = function () {
        playerResponded = true;
        modal.style.display = "none";
        const baseUrl = "https://localhost:7102/Home/GameLobby";
        window.location.href = `${baseUrl}`;
    }
 }

    //// Timer
    //for (let i = 15; i >= 0; i--) {
    //    timer.textContent = i
    //    if (playerResponded) {
    //        return;
    //    }
    //    await new Promise(resolve => setTimeout(resolve, 1000));
    //    if (i == 0) {
    //        connection.invoke("AbortChallenge", groupId, playerName);
    //        modal.style.display = "none";

    //    }
    //}