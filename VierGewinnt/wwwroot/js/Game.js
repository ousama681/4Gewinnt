"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();

connection.start()
    .then(() => {
        PlaceAlreadyPlayedMoves(movesToLoad);
    }
);

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
                colDepth[key] = colDepth[key] - 1;
                moveNr++;
                return;
            }
        } else {
            var selectedCell = document.getElementById(`${column}${colDepth[key]}`);
            if (selectedCell != null) {
                selectedCell.style.backgroundColor = "red";
                colDepth[key] = colDepth[key] - 1;
                moveNr++;
                return;
            }
        }    
    });
}


disableButton("btnColYellow"); // Gelb startet immer

connection.on("AnimatePlayerMove", async (column, playerId) => {
    event.preventDefault();
    var playerIdOne = document.getElementById("playerIdOne").value;
    var playerIdTwo = document.getElementById("playerIdTwo").value;
    var endRow = await PlaceChipInArray(column)

    if (endRow != "full" && playerIdOne == playerId) {
        animate(column, endRow, "yellow")
    }
    else if (endRow != "full" && playerIdTwo == playerId) {
        animate(column, endRow, "red")
    }
    
    // Hier die Logik einbauen um f�r den Spieler der dran ist die Buttons zu enablen.
    console.log("Spielzug auf Spalte: " + column);
});


window.onload = function () {
    var YellowBtn = document.getElementById("btnColYellow");
    YellowBtn.onclick = function () {
        event.preventDefault();

        var arr = $('form').serializeArray();
        var dataObj = {};

        $(arr).each(function (i, field) {
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

async function animate(column, endRow, color) {
    if (color == "yellow") {
        for (let row = 1; row <= endRow; row++) {
            var selectedCell = document.getElementById(`${column}${row}`);
            if (selectedCell != null) {
                if (row == endRow) {
                    selectedCell.style.backgroundColor = "yellow";
                    disableButton("btnColYellow");
                    activateButton("btnColRed");
                    return;
                }
                selectedCell.classList.add('blinkYellow');
                await new Promise(resolve => setTimeout(resolve, 1000));
                selectedCell.classList.remove('blinkYellow');
 
            }
        }
    }
    else if (color == "red") {
        for (let row = 1; row <= endRow; row++) {
            var selectedCell = document.getElementById(`${column}${row}`);
            if (selectedCell != null) {
                if (row == endRow) {
                    selectedCell.style.backgroundColor = "red";
                    disableButton("btnColRed");
                    activateButton("btnColYellow");
                    return;
                }
                selectedCell.classList.add('blinkRed');
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