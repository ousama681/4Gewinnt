"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();



connection.start();

connection.on("AnimatePlayerMove", (message) => {
    event.preventDefault();
    console.log(message);
});


window.onload = function () {
    var element = document.getElementById("btnColYellow");
    element.onclick = function () {
        event.preventDefault();
        connection.invoke("SendPlayerMove", element.value);
    }

    var element = document.getElementById("btnColRed");
    element.onclick = function () {
        event.preventDefault();
        connection.invoke("SendPlayerMove", element.value);
    }
}