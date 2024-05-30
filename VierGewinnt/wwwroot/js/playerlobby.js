"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/playerlobbyHub").build();


// Global variables
var connectionId;
var playerName;


// Init
connection.start().then(function () {
    var username = document.getElementById("userNameLabel").textContent;
    connection.invoke("AddUser", username);
    connection.invoke("GetAvailableUsers");
    connection.invoke("SendNotification", username);
    connection.invoke("FillRobotLobby");
});

connection.on("SetConID", function (playerOneId, playerOnename) {
    connectionId = playerOneId;
    playerName = playerOnename
});



// Load Players
connection.on("ReceiveAvailableUsers", function (players) {
    for (const [player, id] of Object.entries(players)) {
        if (player != playerName) {
            createListelement(player, id);
        }
    }
});

connection.on("ReceiveNewUser", function (playerTwo, playerTwoId) {
    createListelement(playerTwo, playerTwoId);
});

function createListelement(playerTwo, playerTwoId) {

    // Create the <li> element
    var li = document.createElement("li");
    li.className = "list-group-item d-flex justify-content-between align-items-center";

    // Create the <a> element
    var anchor = document.createElement("a");
    anchor.className = "text-decoration-none";

    // Create the <span> element
    var dotSpan = document.createElement("span");
    dotSpan.className = "dot bg-success";

    // Append the <a> element to the <li> element
    li.appendChild(anchor);

    // Append the <span> element to the <li> element
    li.appendChild(dotSpan);

    li.id = playerTwo;
    anchor.textContent = `${playerTwo}`;

    anchor.onclick = function () {
        event.preventDefault();
        console.log("ChallengePlayer invoked");
        connection.invoke("ChallengePlayer", connectionId, playerTwoId, playerName, playerTwo);
    }

    li.appendChild(anchor);
    document.getElementById("playerList").appendChild(li);
}



// Initiate Game after clicking on Opponent and both players accept Modals
connection.on("ReceiveChallenge", function (payload, playerOneId, groupId) {
    console.log("Challenge received for group: " + payload);
    showPlayerTwoChallengeModal(payload, playerOneId, groupId);
});

async function showPlayerTwoChallengeModal(payload, playerOneId, groupId) {
    const players = payload.split(',');
    const modal = document.getElementById("challengeModal");
    const label = document.getElementById("modalLabel");
    const timer = document.getElementById("timer");
    let playerResponded = false;

    label.textContent = `${players[0]} wants to play a game. Do you accept the challenge?`;
    modal.style.display = "block";

    // When playerTwo confirms
    document.getElementById("confirmButton").onclick = function () {
        playerResponded = true;
        connection.invoke("ConfirmChallenge", payload, playerOneId, groupId);
        modal.style.display = "none";
    }

    // When PlayerTwo aborts
    document.getElementById("abortButton").onclick = function () {
        playerResponded = true;
        connection.invoke("AbortChallenge", groupId, playerName);
        modal.style.display = "none";
    }

    // Timer
    for (let i = 15; i >= 0; i--) {
        timer.textContent = i
        if (playerResponded) {
            return;
        }
        await new Promise(resolve => setTimeout(resolve, 1000));
        if (i == 0) {
            connection.invoke("AbortChallenge", groupId, playerName);
            modal.style.display = "none";
        }
    }
};

connection.on("AcceptChallenge", function (payload, groupId) {
    console.log("Challenge received for group: " + payload);
    showPlayerOneChallengeModal(payload, groupId);
});

async function showPlayerOneChallengeModal(payload, groupId) {
    const players = payload.split(',');
    const modal = document.getElementById("challengeModal");
    const label = document.getElementById("modalLabel");
    const timer = document.getElementById("timer");
    let playerResponded = false;

    label.textContent = `${players[1]} is ready to start the game. Start?`;
    modal.style.display = "block";

    // When playerOne confirms
    document.getElementById("confirmButton").onclick = function () {
        playerResponded = true;
        modal.style.display = "none";
        connection.invoke("StartGame", payload)
    }
    // When playerOne aborts
    document.getElementById("abortButton").onclick = function () {    
        playerResponded = true;
        connection.invoke("AbortChallenge", groupId, playerName);
        modal.style.display = "none";       
    }

    // Timer
    for (let i = 15; i >= 0; i--) {
        timer.textContent = i
        if (playerResponded) {
            return;
        }
        await new Promise(resolve => setTimeout(resolve, 1000));
        if (i == 0) {
            connection.invoke("AbortChallenge", groupId, playerName);
            modal.style.display = "none";
        }
    }
};

connection.on("ChallengeAborted", function (player, groupId) {
    showAbortChallengeModal(player, groupId);
});

function showAbortChallengeModal(player, groupId) {
    const modal = document.getElementById("abortModal");
    modal.style.display = "block";
    // When playerOne confirms
    document.getElementById("okButton").onclick = function () {
        connection.invoke("RemoveFromGroup", groupId, connectionId)
        modal.style.display = "none";
    }
};



// Player leaves Lobby
connection.on("PlayerLeft", (userName) => {
    var liElement = document.getElementById(userName);
    liElement.remove();
});

window.addEventListener("beforeunload", () => {
    var username = document.getElementById("userNameLabel").textContent;
    connection.invoke("LeaveLobby", username);
});

connection.on("NavigateToGame", (gameId) => {
    const baseUrl = "https://localhost:7102/Game/Board";
    //const baseUrl = "https://localhost:8443/Game/Board";
    const params = new URLSearchParams();
    params.append("gameId", gameId);

    window.location.href = `${baseUrl}?${params.toString()}`;
});



// Robot Lobby

// Test method
var testButton = document.getElementById("test")
testButton.onclick = function () {
    var testText = document.getElementById("testtext").value
    event.preventDefault();
    connection.invoke("CreateRobot", testText);
}



connection.on("UpdateRobotLobby", function (robots) {
    document.getElementById("robotList").innerHTML = "";
    for (var i = 0; i < robots.length; i++) {
        createListelementRobot(robots[i]);
    }
});

connection.on("AddRobot", function (robotID) {
    connection.invoke("AddRobot", robotID)
});

function createListelementRobot(robot) {

    var playerOne = document.getElementById("userNameLabel").textContent;

    var li = document.createElement("li");
    li.className = "list-group-item d-flex justify-content-between align-items-center";
    li.id = robot;

    var anchor = document.createElement("a");
    anchor.className = "text-decoration-none";
    anchor.textContent = `${robot}`;

    var dotSpan = document.createElement("span");
    dotSpan.className = "dot bg-success";

    li.appendChild(anchor);
    li.appendChild(dotSpan);

    anchor.onclick = function () {
        event.preventDefault();
        connection.invoke("ChallengeRobot", playerOne, robot);
    }



    // Add drag and drop event listeners
    li.draggable = true;
    li.addEventListener('dragstart', handleDragStart);
    li.addEventListener('dragend', handleDragEnd);



    li.appendChild(anchor);
    document.getElementById("robotList").appendChild(li);
}

connection.on("NavigateToGameAgainstRobot", (gameId) => {
    const baseUrl = "https://localhost:7102/Game/BoardPvE";
    const params = new URLSearchParams();
    params.append("gameId", gameId);

    window.location.href = `${baseUrl}?${params.toString()}`;
});

connection.on("RobotLeft", (robot) => {
    var liElement = document.getElementById(robot);
    liElement.remove();
});



// TO DO: create groups for each robot list, so people cant start games against already dragged robots, 
// also remove onklick on already dragged items, so we dont start a game against a robot

// Drag and drop handlers
function handleDragStart(event) {
    event.dataTransfer.setData('text/plain', event.target.id);
    event.target.style.opacity = '0.5';
}

function handleDragEnd(event) {
    event.target.style.opacity = '';
}

function handleDragOver(event) {
    event.preventDefault();
    // Only allow dropping on the drop zones
    if (event.target && (event.target.id === 'selectedRobots' || event.target.id === 'robotList')) {
        event.target.classList.add('droppable');
    }
}

function handleDragLeave(event) {
    // Only remove class if leaving the drop zones
    if (event.target && (event.target.id === 'selectedRobots' || event.target.id === 'robotList')) {
        event.target.classList.remove('droppable');
    }
}

function handleDrop(event) {
    event.preventDefault();
    // Only handle drop if it's on the drop zones
    if (event.target && (event.target.id === 'selectedRobots' || event.target.id === 'robotList')) {
        const id = event.dataTransfer.getData('text/plain');
        const draggableElement = document.getElementById(id);
        const dropzone = event.target;
        dropzone.classList.remove('droppable');
        dropzone.appendChild(draggableElement);
        event.dataTransfer.clearData();
    }
}

// Set up the drop zones
const selectedRobots = document.getElementById('selectedRobots');
const robotList = document.getElementById('robotList');

selectedRobots.addEventListener('dragover', handleDragOver);
selectedRobots.addEventListener('dragleave', handleDragLeave);
selectedRobots.addEventListener('drop', handleDrop);

robotList.addEventListener('dragover', handleDragOver);
robotList.addEventListener('dragleave', handleDragLeave);
robotList.addEventListener('drop', handleDrop);