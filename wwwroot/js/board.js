function placeYellowStone(inputId) {
    var colNumber = document.getElementById(inputId).value;
    var selectedColumn = document.querySelector('.cell[data-colnr="' + colNumber + '"]');
    if (selectedColumn) {
        selectedColumn.classList.add('blinkYellow');
        setTimeout(function () {
            selectedColumn.classList.remove('blinkYellow');
        }, 5000); // Blink duration in milliseconds
    }
    return false; // Prevent form submission
}

function placeRedStone(inputId) {
    var colNumber = document.getElementById(inputId).value;
    var selectedColumn = document.querySelector('.cell[data-colnr="' + colNumber + '"]');
    if (selectedColumn) {
        selectedColumn.classList.add('blinkRed');
        setTimeout(function () {
            selectedColumn.classList.remove('blinkRed');
        }, 5000); // Blink duration in milliseconds
    }
    return false; // Prevent form submission
}

// To be tested: ChatGpt
$(function () {
    var hub = $.connection.myHub;

    // Start the SignalR connection
    $.connection.hub.start().done(function () {
        $('#placeYellowStone').submit(function (event) {
            event.preventDefault();
            var columnNumber = $('#colNumberYellow').val();

            // TO DO: Implement function to Hub
            hub.server.placeStone(columnNumber);
        });
    });

    // Define a client-side method to handle the response from the server
    hub.client.stonePlaced = function (columnNumber) {
        // Update the UI here with the received columnNumber
        console.log('Stone placed in column:', columnNumber);
    };
});