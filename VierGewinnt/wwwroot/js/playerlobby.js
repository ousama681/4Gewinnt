"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/playerlobbyHub").build();


// Global variables
var connectionId;
var playerName;


// Init
connection.start().then(function () {
    var username = document.getElementById("userNameLabel").textContent;
    connection.invoke("GetAvailableUsers");
    connection.invoke("AddUser", username);
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
        createListelement(player, id);
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



// Initiate Game after clicking on Opponent and accepting Modals
connection.on("ReceiveChallenge", function (payload, playerOneId) {
    console.log("Challenge received for group: " + payload);
    showModal(payload, playerOneId);
});

function showModal(payload, playerOneId) {
    // Display modal code here
    const modal = document.getElementById("challengeModal");
    modal.style.display = "block";
    // When the player confirms
    document.getElementById("confirmButton").onclick = function () {
        connection.invoke("ConfirmChallenge", payload, playerOneId);
        modal.style.display = "none";
    }
}

connection.on("AcceptChallenge", function (payload) {
    console.log("Challenge received for group: " + payload);
    showModal2(payload);
});

function showModal2(payload) {
    const modal = document.getElementById("challengeModal");
    modal.style.display = "block";
    // When the player confirms
    document.getElementById("confirmButton").onclick = function () {
        modal.style.display = "none";
        connection.invoke("StartGame", payload)
    }
}

function startGame(groupId) {
    console.log("Game starting for group: " + payload);
    connection.invoke("StartGame", payload)
    
}



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

    var anchor = document.createElement("a");
    anchor.className = "text-decoration-none";

    var dotSpan = document.createElement("span");
    dotSpan.className = "dot bg-success";

    li.appendChild(anchor);
    li.appendChild(dotSpan);

    li.id = robot;
    anchor.textContent = `${robot}`;

    anchor.onclick = function () {
        event.preventDefault();
        connection.invoke("ChallengeRobot", playerOne, robot);
    }

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

//Robot vs Robot

// ändern zu drag and drop statt click

//var robotList = document.getElementById("robotList");
//var selectedRobots = document.getElementById("selectedRobots");

//robotList.addEventListener("click", function (event) {
//    var selectedRobot = event.target;
//    var selectedRobotsCount = selectedRobots.children.length;
//    if (selectedRobotsCount < 2 && selectedRobot.tagName === "LI") {
//        // Clone the selected robot and append it to the selectedRobots list
//        var clonedRobot = selectedRobot.cloneNode(true);
//        selectedRobots.appendChild(clonedRobot);
//    }
//});