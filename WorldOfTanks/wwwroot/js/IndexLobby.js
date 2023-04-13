var connection;
connection = new signalR.HubConnectionBuilder().withUrl("/LobbyHub").build();
connection.start().then();

connection.on("refreshPageIndexLobby", function () {
    location.reload(); return false;
});
connection.on("redirectToHome", function (message) {
    var baseURL = window.location.origin;
    var url = new URL(`/Home/index`, baseURL);
    url.searchParams.set(`message`, `${message}`);
    window.location.href = url;
});

var playerName = document.getElementById(`playerName`);
var messageBlock = document.getElementById(`message`);
var block;
var url;


function addUrlParamName(blockId) {
    block = document.getElementById(`${blockId}`);
    url = new URL(`${block.href}`);
    url.searchParams.set(`PlayerName`, `${playerName.value}`);
    block.href = url;
}

function validateInput() {
    if (playerName.value == "") {
        messageBlock.innerHTML = `Please enter your nickname`;
        Swal.fire(
            'Error!',
            'Please enter your nickname',
            'error'
        )
        
        return false
    }
    return true
}
