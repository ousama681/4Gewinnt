async function showGameOverModal() {
    const modal = document.getElementById("passwordSuccessModal");
    modal.style.display = "block";

    // When player wants to go back to lobby
    document.getElementById("okButton").onclick = function () {
        modal.style.display = "none";
    }
}
