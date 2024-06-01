window.onload = init;

function init() {
    document.getElementById("btnColRed").onclick = nextMove;
}



let colDepth = {};

colDepth['1'] = 6;
colDepth['2'] = 6;
colDepth['3'] = 6;
colDepth['4'] = 6;
colDepth['5'] = 6;
colDepth['6'] = 6;
colDepth['7'] = 6;

var moveNr = 1;

let totalMoves = 0;

let movesLeft = [];
let movesPlayed = [];


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

function nextMove() {
    event.preventDefault();
    if (moveNr <= totalMoves) {
    var color = moveNr % 2 != 0 ? "red" : "yellow";
    var nextMove = movesLeft[moveNr-1];
    var colNr = nextMove.column;

    animate(colNr, colDepth[colNr], color);

    colDepth[colNr] = colDepth[colNr] - 1;

    moveNr++;

    if (moveNr > totalMoves) {
        console.log("Alle Moves wurden gespielt!");
    }
    }
}

function assignMoves(moves) {
    movesLeft = Array(moves.length);
    movesLeft = moves;
    totalMoves = movesLeft.length;
    console.log(movesLeft);
}
