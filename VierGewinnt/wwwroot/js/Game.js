"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();


connection.on("ReceiveMessage", function (message) {
    event.preventDefault();
    // Animationszug
    var column = document.getElementById("colNumberYellow").value;
    connection.invoke("Animate")
    console.log("Der Spielzug wird animiert");

    document.getElementById("robotStatus").textContent = "Der Spielzug wird animiert";
});

connection.start().then(function () {
    console.log("Wir sind im GameJS drin ak, GameHub funktioniert.")

    // Start Player is always Yellow, so we disable the Button of Red
    disableButton("btnRed")
});


var buttonYellow = document.getElementById("btnYellow");
buttonYellow.onclick = function () {
    event.preventDefault();
    var column = document.getElementById("colNumberYellow").value;
    connection.invoke("SendPlayerMove", column, "btnYellow").catch(function (err) {
        return console.error(err.toString());
    }).catch(function (err) {
        return console.error(err.toString());
    });
};

var buttonRed = document.getElementById("btnRed");
buttonRed.onclick = function () {
    event.preventDefault();
    var column = document.getElementById("colNumberRed").value;
    connection.invoke("SendPlayerMove", column, "btnRed").catch(function (err) {
        return console.error(err.toString());
    }).catch(function (err) {
        return console.error(err.toString());
    });
};

connection.on("disableButton", async function (btnId) {
    event.preventDefault();
    var button = document.getElementById(btnId);
    button.disabled = true;
})
connection.on("activateButton", async function (btnId) {
    event.preventDefault();
    var button = document.getElementById(btnId);
    button.disabled = false;
})

connection.on("blinkTopRow", async function (column, btnId) {
    event.preventDefault();
    var selectedCell = document.getElementById(`${column}1`);
    if (selectedCell != null) {
        if (btnId == "btnYellow") {
            selectedCell.classList.add('blinkYellow');
            return "yellow"
        }
        else {
            selectedCell.classList.add('blinkRed');
            return "red"
        }
    }
    return null;
})

connection.on("animate", async function (column, endRow, btnId) {
    event.preventDefault();
    if (btnId == "btnYellow") {
        for (let row = 1; row <= endRow; row++) {
            var selectedCell = document.getElementById(`${column}${row}`);
            if (selectedCell != null) {
                selectedCell.classList.add('blinkYellow');
                await new Promise(resolve => setTimeout(resolve, 1000));
                selectedCell.classList.remove('blinkYellow');
                if (row == endRow) {
                    selectedCell.style.backgroundColor = "yellow";
                }
            }
        }
    }
    else if (btnId == "btnRed") {
        for (let row = 1; row <= endRow; row++) {
            var selectedCell = document.getElementById(`${column}${row}`);
            if (selectedCell != null) {
                selectedCell.classList.add('blinkRed');
                await new Promise(resolve => setTimeout(resolve, 1000));
                selectedCell.classList.remove('blinkRed');
                if (row == endRow) {
                    selectedCell.style.backgroundColor = "red";
                }
            }
        }
    }
})