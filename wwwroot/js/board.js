
function disableButton(btnId) {
    var button = document.getElementById(btnId);
    button.disabled = true;
}

function activateButton(btnId) {
    var button = document.getElementById(btnId);
    button.disabled = false;
}


async function BlinkTopRow(column, btnId) {
    var selectedCell = document.getElementById(`${column.value}1`);
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
}

async function StopBlinkTopRow(column, color) {
    var selectedCell = document.getElementById(`${column.value}1`);
    if (selectedCell != null) {
        if (color == "yellow") {
            selectedCell.classList.remove('blinkYellow');
        }
        else {
            selectedCell.classList.remove('blinkRed');
        }
    }
}

async function animate(column, endRow, color) {
    if (color == "yellow") {
        for (let row = 1; row <= endRow; row++) {
            var selectedCell = document.getElementById(`${column.value}${row}`);
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
    else if (color == "red") {
        for (let row = 1; row <= endRow; row++) {
            var selectedCell = document.getElementById(`${column.value}${row}`);
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
}
