"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();

connection.start()
    .then(() => {
        PlaceAlreadyPlayedMoves(movesToLoad); 
        // Wenn playerOne immer Rot ist, dann heisst das, dass PlayerOne immer bei ungeraden Zahlen drann ist.
        connection.invoke("RegisterGameInStaticProperty", playerOneID, playerTwoID, gameId)
    }
);

connection.on("NotificateGameEnd", function (winnerId) {
    console.log(`Gratuliere ${winnerId}!! Du hast gewonnen!`);
    disableButton("btnColRed");
    disableButton("btnColYellow");
    var audio = document.getElementById("winSound");
    audio.play();
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

function PlaceAlreadyPlayedMoves(movesToLoad) {
    event.preventDefault();
    let colDepth = {};

    colDepth['1'] = 6;
    colDepth['2'] = 6;
    colDepth['3'] = 6;
    colDepth['4'] = 6;
    colDepth['5'] = 6;
    colDepth['6'] = 6;
    colDepth['7'] = 6;

    var moveNr = 1;
    
    movesToLoad.forEach(m => {
        var column = m['column'];
        var key = `${column}`;
        
        if (moveNr % 2 == 0) {
            var selectedCell = document.getElementById(`${column}${colDepth[key]}`);
            if (selectedCell != null) {
                selectedCell.style.backgroundColor = "yellow";
                array[column -1][(colDepth[key]) -1] = 1; // update virtual board
                colDepth[key] = colDepth[key] - 1;
                moveNr++;
                return;
            }
        } else {
            var selectedCell = document.getElementById(`${column}${colDepth[key]}`);
            if (selectedCell != null) {
                selectedCell.style.backgroundColor = "red";
                array[column -1][(colDepth[key]) -1] = 1; // update virtual board
                colDepth[key] = colDepth[key] - 1;
                moveNr++;
                return;
            }
        }
    });

    if (moveNr % 2 != 0) {
        activateButton("btnColRed");
        disableButton("btnColYellow");
    } else  {
        activateButton("btnColYellow");
        disableButton("btnColRed");
    }
}

disableButton("btnColYellow"); // Red startet immer

connection.on("AnimatePlayerMove", async (column, playerId) => {
    event.preventDefault();
    var playerIdOne = document.getElementById("playerIdOne").value;
    var playerIdTwo = document.getElementById("playerIdTwo").value;
    var endRow = await PlaceChipInArray(column)
    if (endRow == "full") {
        alert("Row is already full. Please select another Row.")
        if (playerIdOne == playerId) {
            activateButton("btnColRed")
        }
        else {
            activateButton("btnColYellow")
        }
        
    }
    else if (playerIdOne == playerId) {
        animate(column, endRow, "red")
    }
    else if (playerIdTwo == playerId) {
        animate(column, endRow, "yellow")
    }
});


window.onload = function () {
    var YellowBtn = document.getElementById("btnColYellow");
    YellowBtn.onclick = function () {
        event.preventDefault();
        disableButton("btnColYellow");
        var arr = $('form').serializeArray();
        var dataObj = {};

        $(arr).each(function (i, field) {
            dataObj[field.name] = field.value;
        });

        var playerIdOne = dataObj['userIdTwo'];
        var boardIdOne = dataObj['boardIdTwo'];
        var columnYellow = dataObj['colNumberYel'];

        connection.invoke("SendPlayerMove", playerIdOne, boardIdOne, columnYellow);
    }

    var RedBtn = document.getElementById("btnColRed");

    RedBtn.onclick = function () {
        event.preventDefault();
        disableButton("btnColRed");
        var arr = $('form').serializeArray();
        var dataObj = {};

        $(arr).each(function (i, field) {
            dataObj[field.name] = field.value;
        });

        var playerIdTwo = dataObj['userIdOne'];
        var boardIdTwo = dataObj['boardIdOne'];
        var columnRed = dataObj['colNumberRed'];

        connection.invoke("SendPlayerMove", playerIdTwo, boardIdTwo, columnRed);
    }
}

async function animate(column, endRow, color) {
    var audio = document.getElementById("beep");
    var audioEndPos = document.getElementById("beepEnd");
    if (color == "yellow") {
        disableButton("btnColYellow");
        for (let row = 1; row <= endRow; row++) {
            var selectedCell = document.getElementById(`${column}${row}`);
            if (selectedCell != null) {
                if (row == endRow) {
                    selectedCell.style.backgroundColor = "yellow";
                    audioEndPos.play();                    
                    activateButton("btnColRed");
                    return;
                }
                selectedCell.classList.add('blinkYellow');
                audio.play();
                await new Promise(resolve => setTimeout(resolve, 1000));
                selectedCell.classList.remove('blinkYellow'); 
            }
        }
    }
    else if (color == "red") {
        disableButton("btnColRed");
        for (let row = 1; row <= endRow; row++) {
            var selectedCell = document.getElementById(`${column}${row}`);
            if (selectedCell != null) {
                if (row == endRow) {
                    selectedCell.style.backgroundColor = "red";
                    audioEndPos.play();                   
                    activateButton("btnColYellow");
                    return;
                }
                selectedCell.classList.add('blinkRed');
                audio.play();
                await new Promise(resolve => setTimeout(resolve, 1000));
                selectedCell.classList.remove('blinkRed');
            }
        }
    }
}

function disableButton(btnId) {
    var button = document.getElementById(btnId);
    button.disabled = true;
}

function activateButton(btnId) {
    var button = document.getElementById(btnId);
    button.disabled = false;
}

//virtual board to check if a chip is already placed
var array = [
    [0, 0, 0, 0, 0, 0],
    [0, 0, 0, 0, 0, 0],
    [0, 0, 0, 0, 0, 0],
    [0, 0, 0, 0, 0, 0],
    [0, 0, 0, 0, 0, 0],
    [0, 0, 0, 0, 0, 0],
    [0, 0, 0, 0, 0, 0],
];

async function PlaceChipInArray(column) {
    var endRow = await GetEndRow(column);
    if (endRow == "full") {
        return "full"
    }
    array[column - 1][endRow - 1] = 1
    return endRow
}

async function GetEndRow(column) {
    for (var i = 6; i >= 0; i--) {
        if (i == 0) {
            return "full"
        }
        else if (array[column - 1][i - 1] == 0) { // column and row of the board starts at 1, array starts at 0, so we subtract - 1 
            return i
        }
    }
}

window.addEventListener('beforeunload', (event) => {
    // Cancel the event as stated by the standard.
    event.preventDefault();
    // Chrome requires returnValue to be set.
    event.returnValue = '';
});