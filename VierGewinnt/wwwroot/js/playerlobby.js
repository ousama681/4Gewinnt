"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/playerlobbyHub").build();

connection.start().then(function () {
    var username = document.getElementById("userNameLabel").textContent;
    connection.invoke("GetAvailableUsers");
    connection.invoke("AddUser", username);
    connection.invoke("SendNotification", username);
    connection.invoke("FillRobotLobby");
});

connection.on("ReceiveAvailableUsers", function (players) {

    for (var i = 0; i < players.length; i++) {
        createListelement(players[i]);
    }
});

connection.on("ReceiveNewUser", function (playerTwo) {
    createListelement(playerTwo);
});

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

function createListelement(playerTwo) {

    var playerOne = document.getElementById("userNameLabel").textContent;

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
/*        createBoardEntity(playerOne, playerTwo);*/
        connection.invoke("ChallengePlayer", playerOne, playerTwo);
    }

    li.appendChild(anchor);

    document.getElementById("playerList").appendChild(li);
}

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