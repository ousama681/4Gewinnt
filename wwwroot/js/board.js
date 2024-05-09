
function disableButton(btnId) {
    var button = document.getElementById(btnId);
    button.disabled = true;
}

function activateButton(btnId) {
    var button = document.getElementById(btnId);
    button.disabled = false;
}

async function animationYellow(col, endRow, btnId) {
    event.preventDefault();
    disableButton(btnId);
    for (let row = 1; row <= endRow; row++) {
        var selectedCell = document.getElementById(`${col.value}${row}`);
        if (selectedCell != null) {
            selectedCell.classList.add('blinkYellow');
            await new Promise(resolve => setTimeout(resolve, 1000));
            selectedCell.classList.remove('blinkYellow');
            if (row == endRow) {
                selectedCell.style.backgroundColor = "yellow";
            }
        }
    }
    activateButton(btnId);
}

async function animationRed(col, endRow, btnId) {
    event.preventDefault();
    disableButton(btnId);
    for (let row = 1; row <= endRow; row++) {
        var selectedCell = document.getElementById(`${col.value}${row}`);
        if (selectedCell != null) {
            selectedCell.classList.add('blinkRed');
            await new Promise(resolve => setTimeout(resolve, 1000));
            selectedCell.classList.remove('blinkRed');
            if (row == endRow) {
                selectedCell.style.backgroundColor = "red";
            }
        }
    }
    activateButton(btnId);
}

//function placeRedStone(numericId, btnId) {
//    event.preventDefault();
//    disableButton(btnId);
//    var colNumber = document.getElementById(numericId).value;
//    var selectedColumn = document.querySelector('.cell[data-colnr="' + colNumber + '"]');
//    if (selectedColumn) {
//        selectedColumn.classList.add('blinkRed');
//        setTimeout(function () {
//            selectedColumn.classList.remove('blinkRed');
//            activateButton(btnId);
//        }, 5000); // Blink duration in milliseconds
//    }
//    return false; // Prevent form submission
//}

//function placeYellowStone(numericId, btnId) {
//    event.preventDefault();
//    disableButton(btnId);
//    var colNumber = document.getElementById(numericId).value;
//    var selectedColumn = document.querySelector('.cell[data-colnr="' + colNumber + '"]');
//    if (selectedColumn) {
//        selectedColumn.classList.add('blinkYellow');
//        setTimeout(function () {
//            selectedColumn.classList.remove('blinkYellow');
//            activateButton(btnId);
//        }, 5000); // Blink duration in milliseconds
//    }
//    return false; // Prevent form submission
//}