var connection;
connection = new signalR.HubConnectionBuilder().withUrl("/LobbyHub").build();
connection.start().then();
connection.on("refreshPlayerList", function (data, playerName) {
    refreshPlayerList(data, playerName);
});
connection.on("deleteLobby", function (message) {
    var baseURL = window.location.origin;
    var url = new URL(`/Lobby/index`, baseURL);
    url.searchParams.set(`message`, `${message}`);
    window.location.href = url;
});
connection.on("gameStart", function () {
    var baseURL = window.location.origin;
    var url = new URL(`/Game/Index`, baseURL);
    window.location.href = url;
});
connection.on("redirectToHome", function (message) {
    var baseURL = window.location.origin;
    var url = new URL(`/Home/index`, baseURL);
    url.searchParams.set(`message`, `${message}`);
    window.location.href = url;
});
connection.on("addChatData", function (mes) {
    addChatData(mes);
});
connection.on("chatErrorData", function (mes) {
    chatErrorData(mes);
});


setTimeout(`getPlayerList()`, 200);

var audioMessage = new Audio(document.getElementById(`sound_message`).src);
audioMessage.volume = 0.5;
var blockChatMesInput = document.getElementById("chatMesInput");
var blockValidMessage = document.getElementById(`validMessage`);
var blockHideChat = document.getElementById("hideChat");
var blockShowChat = document.getElementById("showChat");
var blockChat = document.getElementById("chatDiv");
var chatMesList = document.getElementById(`chatMesList`).value;
var jsonChatMesList = JSON.parse(chatMesList);
if (jsonChatMesList != null) {
    jsonChatMesList.forEach(function (value) {
        addChatData(value);
    });
}
var startBtn = document.getElementById(`startBtn`);
var numberOfTeams = document.getElementById(`NumOfTeams`).value;
var readyVal;

blockChatMesInput.addEventListener(`keydown`, setKeyEnterMes);

function setKeyEnterMes(event) {
    if (event.key == `Enter`) {
        sendMessage();
    }
}

function sendMessage() {
    var message = blockChatMesInput.value;
    if (message.length > 0 && message.length < 101) {
        $.ajax({
            type: "GET",
            url: `/Lobby/SetMessageAjax`,
            data: jQuery.param({ mes: `${message}` }),
            success: function () {

            }
        });
        blockChatMesInput.value = "";
    }
}

function messageSound() {
    audioMessage.currentTime = 0;
    audioMessage.play();
}

function chatErrorData(mes) {
    blockChatMesInput.value = mes;
    blockValidMessage.innerText = `Messages can be sent once every two seconds.please wait`;
    setTimeout(() => { blockValidMessage.innerText = "_"; }, 2000);
}

function addChatData(mes) {
    messageSound();
    var mesDiv = document.createElement(`div`);
    mesDiv.className = `ps-3 text-break border`;
    mesDiv.innerText = mes;
    blockChat.appendChild(mesDiv);
    blockChat.scrollTop = blockChat.scrollHeight;
}

function hideChatik() {
    blockChat.hidden = true;
    blockHideChat.hidden = true;
    blockShowChat.hidden = false;
}

function showChatik() {
    blockChat.hidden = false;
    blockHideChat.hidden = false;
    blockShowChat.hidden = true;
}

function validReady(event) {
    if (!readyVal) {
        event.preventDefault();
        Swal.fire(
            'Error!',
            'Not all players are ready.',
            'error'
        )
    }
}

function getPlayerList() {
    $.ajax({
        type: "GET",
        url: `/Lobby/LobbyAjax`,
        success: function () {

        }
    });
}

function changeTeamReadyPlauer() {
    var playerTeam = document.getElementById(`playerTeam`);
    var playerReady = document.getElementById(`playerReady`);
    var ready = "false";
    if (playerReady.checked) {
        ready = `true`;
    }
    $.ajax({
        type: "POST",
        url: `/Lobby/LobbyAjax`,
        data: jQuery.param({ team: `${playerTeam.value}`, isReady: `${ready}` }),
        success: function () {

        }
    });
}

function refreshPlayerList(data, playerName) {
    var name;
    var playerReady = false;
    var jsonPlayerList = JSON.parse(data);
    readyVal = true;
    $(document).ready(function () {
        var html = '<table class="table table-bordered table-striped" width="100%">';
        html += '<tr>';
        $.each(jsonPlayerList[0], function (index, value) {
            html += '<th>' + index + '</th>';
        });
        html += '</tr>';
        $.each(jsonPlayerList, function (index, value) {
            html += '<tr>';
            name = value[`Name`];
            $.each(value, function (index2, value2) {
                if (index2 == `Ready` && value2 == true && name == playerName) {
                    playerReady = true;
                    readyVal = readyVal && true;
                    html += '<td width="10%">' + '<input type="checkbox" class="form-check-input" id="playerReady" onchange="changeTeamReadyPlauer()" checked />' + '</td>';
                }
                if (index2 == `Ready` && value2 == false && name == playerName) {
                    readyVal = readyVal && false;
                    html += '<td width="10%">' + '<input type="checkbox" class="form-check-input" id="playerReady" onchange="changeTeamReadyPlauer()" unchecked />' + '</td>';
                }
                if (index2 == `Ready` && value2 == true && name != playerName) {
                    readyVal = readyVal && true;
                    html += '<td width="10%">' + '<input type="checkbox" class="form-check-input" checked disabled />' + '</td>';
                }
                if (index2 == `Ready` && value2 == false && name != playerName) {
                    readyVal = readyVal && false;
                    html += '<td width="10%">' + '<input type="checkbox" class="form-check-input" unchecked disabled />' + '</td>';
                }
                if (index2 == `Team` && name == playerName){
                    var select = [];
                    html += '<td width="10%">';
                    html += '<select class="form-select" id="playerTeam" onchange="changeTeamReadyPlauer()">';
                    for (var i = 1; i <= numberOfTeams; i++) {
                        if (i == value2) {
                            select.push(`selected`);
                        }
                        else {
                            select.push(``);
                        }
                        html += `<option value="${i}" ${select[i-1]}>` + i + '</option>';
                    }
                    html += '</td>';
                }
                if (index2 == `Team` && name != playerName) {
                    var select = [];
                    html += '<td width="10%">';
                    html += '<select class="form-select" disabled>';
                    for (var i = 1; i <= numberOfTeams; i++) {
                        if (i == value2) {
                            select.push(`selected`);
                        }
                        else {
                            select.push(``);
                        }
                        html += `<option value="${i}" ${select[i-1]}>` + i + '</option>';
                    }
                    html += '</td>';
                }
                if (index2 != `Ready` && index2 != `Team`) {
                    html += '<td width="10%">' + value2 + '</td>';
                }
            });
            html += '<tr>';
        });
        html += '</table>';
        
        $(`#playerList`).html(html);
        if (playerReady) {
            var playerTeam = document.getElementById(`playerTeam`);
            if (playerTeam != null) {
                playerTeam.disabled = true;
            }
        }
    });
}


