var connection = new signalR.HubConnectionBuilder().withUrl("/indexHub").build();



connection.start();


connection.on("ReceiveRobotFeedback", function (feedbackText) {
    var inputField = document.getElementById("robotResponse");

    inputField.innerText = feedbackText;
});
