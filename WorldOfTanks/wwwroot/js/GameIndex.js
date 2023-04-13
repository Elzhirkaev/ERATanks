var connection;
connection = new signalR.HubConnectionBuilder().withUrl("/LobbyHub").build();
connection.start().then();

connection.on("selectTankStatus", function (message) {
    selectTankStatus(message);
});
connection.on("refreshGameData", function (dataJSONstr) {
    refreshGameData(dataJSONstr);
});
connection.on("addChatData", function (mes) {
    addChatData(mes);
});
connection.on("chatErrorData", function (mes) {
    chatErrorData(mes);
});
connection.on("refreshPage", function () {
    localStorage.setItem("sqSize", JSON.stringify(sqS));
    location.reload(); return false;
});
connection.on("redirectToHome", function (message) {
    var baseURL = window.location.origin;
    var url = new URL(`/Home/index`, baseURL);
    url.searchParams.set(`message`, `${message}`);
    window.location.href = url;
});

var blockChatMesInput = document.getElementById("chatMesInput");
var blockValidMessage = document.getElementById(`validMessage`);
var blockHideChat = document.getElementById("hideChat");
var blockShowChat = document.getElementById("showChat");
var blockChat = document.getElementById("chatDiv");
var blockMap = document.getElementById("mapBlock");
var blockBigMap = document.getElementById("bigMapBlock");
var bmBlock = document.getElementById("bmBlock");
var cursor = document.getElementById(`div-aim`);
var cursorReady = document.getElementById(`div-aimReady`);
var imgInvulnerability1 = document.getElementById(`img-invulnerability1`);
var imgInvulnerability2 = document.getElementById(`img-invulnerability2`);
var playerName = document.getElementById(`playerName`).value;
var audioIntro = new Audio(document.getElementById(`sound_intro`).src);
audioIntro.volume = 0.3;
var audioLeave = new Audio(document.getElementById(`sound_leave`).src);
audioLeave.volume = 0.3;
var audioWon = new Audio(document.getElementById(`sound_won`).src);
audioWon.volume = 0.3;
var audioDefeat = new Audio(document.getElementById(`sound_defeat`).src);
audioDefeat.volume = 0.3;
var audioMessage = new Audio(document.getElementById(`sound_message`).src);
audioMessage.volume = 0.5;
var audiomotorSound = new Audio(document.getElementById(`sound_motorSound`).src);
audiomotorSound.loop = true;
audiomotorSound.volume = 0.2;
var audiosoundOfMovement = new Audio(document.getElementById(`sound_soundOfMovement`).src);
audiosoundOfMovement.loop = true;
audiosoundOfMovement.volume = 0.2;
var audiotankDeath = new Audio(document.getElementById(`sound_tankDeath`).src);
var audiobaseDeath = new Audio(document.getElementById(`sound_baseDeath`).src);
var audioarmorShot = new Audio(document.getElementById(`sound_armorShot`).src);
var audioReload = new Audio(document.getElementById(`sound_reload`).src);
audioReload.volume = 0.5;
var audioshotOnConcrete = new Audio(document.getElementById(`sound_shotOnConcrete`).src);
audioshotOnConcrete.volume = 0.3;
var audiostandartShot = new Audio(document.getElementById(`sound_standartShot`).src);
audiostandartShot.volume = 0.2;
var sizeSq = 20;
var fRequests = 20;
var tRequests = 1000 / fRequests;
var percentages = 100;
var kp = 1;
var direction = 0;
var directionDouble = 0;
var directionTank = 0;
var directionTower = 0;
var wbtn = 0;
var sbtn = 0;
var abtn = 0;
var dbtn = 0;
var mbtn = 0;
var frSound;
var getData = true;
var flagTankMove = false;
var leaveName = [];
var lost = true;
var won = true;
var flagOnOpacity = 0;
var flagOffOpacity = 0;
var x, y, xTower, yTower, mWidth, mHeight, tankId;
var respArr = [];
var mebgArr = [];
var mecvArr = [];
var arrCoords = [];
var arrGameObj = [];
var arrVisionObj = [];
var divTT;
var cur, curReady;
var myTank;
var flagT;
var mmtarget, mctarget, dataJSON, kdirX, kdirY, deltaX, deltaY, xAFEOM, yAFEOM, divAFEOM, urlDME, xAEOM, yAEOM, divAEOM, xASOM, yASOM, divASOM, invTank;
var xATOM, yATOM, tankATOM, xAdTOM, yAdTOM, alfaTM, alfaDegTM, targetTM, imgWTM, alfaCF, alfaDegCF, targetCF, imgWCF, teamR, PJSON, PlListJSON, PlayerListJSON, fireReady;
var player = {
    name: playerName,
    team: document.getElementById(`team_${playerName}`).textContent,
}



var localStorageSqS = localStorage.getItem("sqSize");
if (localStorageSqS != `undefined`) {
    var sqS = JSON.parse(localStorageSqS);
}

if (sqS == null) {
    render();
}
else {
    localStorage.setItem("sqSize", JSON.stringify(null));
    sizeSq = sqS;
    percentages = Math.round(sizeSq / 20 * 100);
    kp = percentages / 100;
    document.getElementById("SqSizeRange").value = sizeSq;
    document.getElementById("sqSize").innerHTML = ": " + percentages + `%`;
    render();
}


respMyHide(`true`);
respEnemyHide(`true`);
var grp = document.getElementsByName(`inputRP`);
if (grp != null) {
    grp.forEach(function (item) {
        item.addEventListener(`keydown`, setKeyEnterRP);
    });
}
blockChatMesInput.addEventListener(`keydown`, setKeyEnterMes);
document.addEventListener(`keydown`, setKey);
document.addEventListener(`keyup`, unsetKey);
document.getElementById("SqSizeRange").addEventListener("change", function () {
    localStorage.setItem("sqSize", JSON.stringify(this.value));
    location.reload(); return false;
});
document.getElementById("frequencyRequestsRange").addEventListener("change", function () {
    fRequests = this.value;
    if (fRequests > 0) {
        tRequests = 1000 / fRequests;
    }
    else {
        tRequests = 99999999;
    }
    document.getElementById("frequencyRequests").innerHTML = ": " + fRequests + `p/s`;
    setData();
});
blockMap.addEventListener('mousemove', mmove);
blockMap.addEventListener('mousedown', mclick);
setData();
createCursor();

function giveRP(name) {
    var rp = Number(document.getElementById(`rpInput_${name}`).value);
    var plRP = Number(document.getElementById(`rPoints_${player.name}`).innerText);
    if (rp > 0 && rp <= plRP) {
        $.ajax({
            type: "GET",
            url: `/Game/GiveRPAjax`,
            data: jQuery.param({ rp: `${rp}`, name: `${name}` }),
            success: function () {

            }
        });
        document.getElementById(`rpInput_${name}`).value = "";
    }
}

function sendMessage() {
    var message = blockChatMesInput.value;
    if (message.length > 0 && message.length < 101) {
        $.ajax({
            type: "GET",
            url: `/Game/SetMessageAjax`,
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
    setTimeout(() => { blockValidMessage.innerText = "_";  }, 2000);
}

function addChatData(mes) {
    messageSound();
    var mesDiv = document.createElement(`div`);
    mesDiv.className = `ps-3 text-break border`;
    mesDiv.innerText = mes;
    blockChat.appendChild(mesDiv);
    blockChat.scrollTop = blockChat.scrollHeight;
}

function hideChatik(){
    blockChat.hidden = true;
    blockHideChat.hidden = true;
    blockShowChat.hidden = false;
}

function showChatik() {
    blockChat.hidden = false;
    blockHideChat.hidden = false;
    blockShowChat.hidden = true;
}

function hideshowSelectTant(flag) {
    if (flag) {
        document.getElementById(`hideSelectT`).hidden = true;
        if (!document.getElementById(`selectTank`).hidden) {
            document.getElementById(`selectTank`).hidden = true;
        }
        if (document.getElementById(`showSelectT`).hidden) {
            document.getElementById(`showSelectT`).hidden = false;
        }
    }
    else {
        document.getElementById(`showSelectT`).hidden = true;
        if (document.getElementById(`selectTank`).hidden) {
            document.getElementById(`selectTank`).hidden = false;
        }
        if (document.getElementById(`hideSelectT`).hidden) {
            document.getElementById(`hideSelectT`).hidden = false;
        }
    }
    
}

function motorSound() {
    if (myTank != null) {
        if (abtn == 1 || wbtn == 1 || dbtn == 1 || sbtn == 1) {
            audiomotorSound.pause();
            audiosoundOfMovement.play();
        }
        else {
            audiosoundOfMovement.pause();
            audiomotorSound.play();
        }
    }
    else {
        audiomotorSound.pause();
        audiosoundOfMovement.pause();
    }
}

function addOpacity(flag) {
    if (flag == 1) {
        flagOnOpacity = 1;
        mebgArr.forEach(function (item) {
            item.classList.add(`opacity-50`);
        })
    }
    else {
        flagOnOpacity = 1;
        flagOffOpacity = 2;
        mebgArr.forEach(function (item) {
            item.classList.remove(`opacity-50`);
        })
    }
}

function tankOnMove(flag) {
    if (flag != flagTankMove) {
        flagTankMove = flag;
        if (flagTankMove) {
            document.getElementById(`selectT`).hidden = true;
            document.addEventListener('mousemove', towerMove);
            bmBlock.addEventListener('mousedown', mClickFire);
            document.addEventListener('mouseup', mmbtnOFF);
            document.addEventListener('mousemove', moveCursor);
            blockMap.style.cursor = "none";
        }
        else {
            document.getElementById(`selectT`).hidden = false;
            myTank = null;
            divTT = null;
            document.removeEventListener(`mousemove`, towerMove);
            bmBlock.removeEventListener(`mousedown`, mClickFire);
            document.removeEventListener(`mouseup`, mmbtnOFF);
            document.removeEventListener('mousemove', moveCursor);
            blockMap.style.cursor = "auto";
        }
    }
    
}

function setData(flag) {
    if (flag == 1) {
        getData = !getData;
    }
    if (getData) {
        $.ajax({
            type: "GET",
            url: `/Game/SetGetDataAjax`,
            data: jQuery.param({ w: `${wbtn}`, s: `${sbtn}`, a: `${abtn}`, d: `${dbtn}`, mbtn: `${mbtn}`, directionTower: `${directionTower}` }),
            success: function () {
                setTimeout(setData, tRequests);
            }
        });
    }
}

function setDataOne() {
    $.ajax({
        type: "GET",
        url: `/Game/SetGetDataAjax`,
        data: jQuery.param({ w: `${wbtn}`, s: `${sbtn}`, a: `${abtn}`, d: `${dbtn}`, mbtn: `${mbtn}`, directionTower: `${directionTower}` }),
        success: function () {

        }
    });
    mbtn = 0;
}

function setKeyEnterRP(event) {
    if (event.key == `Enter`) {
        var tg = event.target;
        var namePlayer = tg.id.replace("rpInput_", "");
        giveRP(namePlayer);
    }
}

function setKeyEnterMes(event) {
    if (event.key == `Enter`) {
        sendMessage();
    }
}

function setKey(event) {
    if (event.key.toLowerCase() == `w` || event.key.toLowerCase() == `ц` || event.key == `ArrowUp`) {
        wbtn = 1;
        motorSound();
    }
    if (event.key.toLowerCase() == `s` || event.key.toLowerCase() == `ы` || event.key == `ArrowDown`) {
        sbtn = 1;
        motorSound();
    }
    if (event.key.toLowerCase() == `a` || event.key.toLowerCase() == `ф` || event.key == `ArrowLeft`) {
        abtn = 1;
        motorSound();
    }
    if (event.key.toLowerCase() == `d` || event.key.toLowerCase() == `в` || event.key == `ArrowRight`) {
        dbtn = 1;
        motorSound();
    }
}

function unsetKey(event) {
    if (event.key.toLowerCase() == `w` || event.key.toLowerCase() == `ц` || event.key == `ArrowUp`) {
        wbtn = 0;
        motorSound();
    }
    if (event.key.toLowerCase() == `s` || event.key.toLowerCase() == `ы` || event.key == `ArrowDown`) {
        sbtn = 0;
        motorSound();
    }
    if (event.key.toLowerCase() == `a` || event.key.toLowerCase() == `ф` || event.key == `ArrowLeft`) {
        abtn = 0;
        motorSound();
    }
    if (event.key.toLowerCase() == `d` || event.key.toLowerCase() == `в` || event.key == `ArrowRight`) {
        dbtn = 0;
        motorSound();
    }
}

function mmove(event) {
    mmtarget = blockMap.getBoundingClientRect();
    x = Math.round((event.clientX - mmtarget.left) / percentages * 100);
    y = Math.round((event.clientY - mmtarget.top) / percentages * 100);
}

function createCursor() {
    cur = document.createElement("div");
    cur.innerHTML = cursor.innerHTML;
    blockMap.appendChild(cur);
    cur.style = `width: ${20 * kp}px; height: ${20 * kp}px; top:0; left:0 ;position:absolute;padding: 0;z-index: 100;`;
    curReady = document.createElement("div");
    curReady.innerHTML = cursorReady.innerHTML;
    blockMap.appendChild(curReady);
    curReady.style = `width: ${20 * kp}px; height: ${20 * kp}px; top:0; left:0 ;position:absolute;padding: 0;z-index: 100;`;
}

function moveCursor() {
    xAdTOM = Math.round(x * kp);
    yAdTOM = Math.round(y * kp);
    fireReadyFunc();
    if (xAdTOM <= mWidth && yAdTOM <= mHeight && x >= 0 && y >= 0) {
        cur.style.transform = `translate3d(${xAdTOM - 9 * kp}px, ${yAdTOM - 9 * kp}px, 0px) rotateZ(0deg)`;
        curReady.style.transform = `translate3d(${xAdTOM - 9 * kp}px, ${yAdTOM - 9 * kp}px, 0px) rotateZ(0deg)`;
    }
}

function reloadSound() {
    if (frSound != fireReady) {
        frSound = fireReady;
        if (frSound == true) {
            audioReload.currentTime = 0;
            audioReload.play();
        }
    }
}

function fireReadyFunc() {
    if (myTank == null) {
        cur.hidden = true;
        curReady.hidden = true;
        return;
    }
    if (xAdTOM <= mWidth && yAdTOM <= mHeight && x >= 0 && y >= 0) {
        if (cur.hidden) {
            cur.hidden = false;
        }
        if (fireReady) {
            if (curReady.hidden) {
                curReady.hidden = false;
            }
        }
        else {
            if (!curReady.hidden) {
                curReady.hidden = true;
            }
        }
    }
    else {
        cur.hidden = true;
        curReady.hidden = true;
    }
}

function mclick(event) {
    mctarget = blockMap.getBoundingClientRect();
    x = Math.round((event.clientX - mctarget.left) / percentages * 100);
    y = Math.round((event.clientY - mctarget.top) / percentages * 100);
}

function mmbtnOFF() {
    mbtn = 0;
}

function selectTankStatus(message) {
    if (message == `Ok`) {
        audioIntro.play();
        setTimeout(motorSound, 4000);
    }
    if (message == "Error") {
        divTT = null;
    }
}

function refreshGameData(dataJSONstr) {
    dataJSON = JSON.parse(dataJSONstr);
    arrGameObj.forEach(function (item) {
        item.remove();
    })
    arrGameObj = [];
    fireReady = dataJSON[`PJSON`][`FireReady`];
    reloadSound();
    fireReadyFunc();
    document.getElementById(`playerHeath`).innerHTML = "HP: " + dataJSON[`PJSON`][`Health`];
    
    dataJSON[`PlayerListJSON`].forEach(function (value) {
        if (value[`Name`] == player.name) {
            if (Number(value[`RebirthPoints`]) >= 10000000) {
                if (flagOffOpacity == 0) {
                    addOpacity(2);
                    arrVisionObj = [];
                }
                document.getElementById(`playerRebirthPoints`).className = `text-success`;
                document.getElementById(`playerRebirthPoints`).innerHTML = "You have won";
                wonSound();
            }
            else {
                if (Number(value[`RebirthPoints`]) >= -10000000 && Number(value[`RebirthPoints`]) < 10000000) {
                    document.getElementById(`playerRebirthPoints`).innerHTML = "RP: " + value[`RebirthPoints`];
                }
                else {
                    document.getElementById(`playerRebirthPoints`).className = `text-danger`;
                    document.getElementById(`playerRebirthPoints`).innerHTML = "You've lost";
                    lostSound();
                }
            }
            
        }
        if (Number(value[`RebirthPoints`]) >= 10000000) {
            document.getElementById(`rPoints_${value["Name"]}`).className = `text-success`;
            document.getElementById(`rPoints_${value["Name"]}`).innerHTML = "Victory"
        }
        else {
            if (Number(value[`RebirthPoints`]) >= 0 && Number(value[`RebirthPoints`]) < 10000000) {
                document.getElementById(`rPoints_${value["Name"]}`).innerHTML = value[`RebirthPoints`];
            }
            else {
                if (Number(value[`RebirthPoints`]) < 0 && Number(value[`RebirthPoints`]) > -90000000) {
                    document.getElementById(`rPoints_${value["Name"]}`).className = `text-secondary`;
                    document.getElementById(`rPoints_${value["Name"]}`).innerHTML = "Leave";
                    leaveSound(value[`Name`]);
                }
                else {
                    document.getElementById(`rPoints_${value["Name"]}`).className = `text-danger`;
                    document.getElementById(`rPoints_${value["Name"]}`).innerHTML = "Lost";
                }
            }
        }
        document.getElementById(`kills_${value["Name"]}`).innerHTML = value[`Kills`];
        document.getElementById(`death_${value["Name"]}`).innerHTML = value[`Death`];
        document.getElementById(`damageSum_${value["Name"]}`).innerHTML = value[`DamageSum`];
        if (document.getElementById(`damageFriendly_${value["Name"]}`) != null) {
            document.getElementById(`damageFriendly_${value["Name"]}`).innerHTML = value[`FriendlyFire`];
        }
        teamR = document.getElementById(`team_${value["Name"]}`).innerText;
        addTankOnMapRefresh(value[`TankId`], value[`X`], value[`Y`], value[`Direction`], value[`DirectionTower`], value["Name"], teamR, value[`Invulnerability`]);
    });
    teamVision(dataJSON[`TeamVisionListJSON`]);
    dataJSON[`RemoveCVListJSON`].forEach(function (value) {
        deleteMapElement(value, false);
    });
    dataJSON[`PlayerShotListJSON`].forEach(function (value) {
        addShotOnMapRefresh(value[`X`], value[`Y`], value[`Direction`]);
    });
    dataJSON[`ExplosionListJSON`].forEach(function (value) {
        addExplosionOnMapRefresh(value[`X`], value[`Y`]);
    });
    dataJSON[`FireExplosionListJSON`].forEach(function (value) {
        addFireExplosionOnMapRefresh(value[`X`], value[`Y`], value[`Direction`]);
    });
}

function leaveSound(name) {
    var countLeave = 0;
    leaveName.forEach(function (item) {
        if (item == name) {
            countLeave = countLeave + 1;
        }
    })
    if (countLeave == 0) {
        leaveName.push(name);
        audioLeave.play();
    }
}

function lostSound() {
    if (lost) {
        lost = false;
        audiobaseDeath.play();
        audioDefeat.play();
        setTimeout(() => { document.getElementById(`selectT`).hidden = true; }, 200);
    }
}

function wonSound() {
    if (won) {
        won = false;
        audiobaseDeath.play();
        audioWon.play();
        setTimeout(() => { document.getElementById(`selectT`).hidden = true; }, 200);
    }
}

function teamVision(value) {
    var blockTV;
    arrVisionObj.forEach(function (item) {
        item.classList.add(`opacity-50`);
    })
    arrVisionObj = [];
    value.forEach(function (val) {
        blockTV = document.getElementById(`bgme${val}`);
        blockTV.classList.remove(`opacity-50`);
        arrVisionObj.push(blockTV);
    });
}

function addFireExplosionOnMapRefresh(xExp, yExp, direct) {
    
    if (direct > 180) {
        kdirX = (360 - direct) / 180;
    }
    else {
        kdirX = direct / 180;
    }
    if (direct <= 90) {
        kdirY = (90 - direct) / 180;
    }
    if (direct > 90 && direct <= 270) {
        kdirY = (direct - 90) / 180;
    }
    if (direct > 270) {
        kdirY = (360 - direct + 90) / 180;
    }
    audiostandartShot.currentTime = 0;
    audiostandartShot.play();
    xAFEOM = Math.round(xExp * kp);
    yAFEOM = Math.round(yExp * kp);
    deltaX = kdirX * 11 * kp;
    deltaY = kdirY * 11 * kp;;
    divAFEOM = document.createElement("div");
    divAFEOM.innerHTML = document.getElementById(`div-fire1`).innerHTML;
    blockMap.appendChild(divAFEOM);
    divAFEOM.style = `width: ${Math.round(10 * kp)}px; height: ${Math.round(10 * kp)}px; top:0; left:0 ;position:absolute;padding: 0;`;
    divAFEOM.style.transform = `translate3d(${xAFEOM - deltaX }px, ${yAFEOM - deltaY}px, 0px) rotateZ(${direct - 45}deg)`;
    arrGameObj.push(divAFEOM);
}

function deleteMapElement(value, flagDel) {
    var blockDME = document.getElementById(`${value}`);
    if (flagDel) {
        blockDME.remove();
        return;
    }
    if (blockDME == null) {
        return;
    }
    urlDME = document.getElementById(`img-explosion1`).src;
    if (value.indexOf('tank') >= 0) {
        if (blockDME.hidden) {
            blockDME.hidden = false;
        }
        audiotankDeath.currentTime = 0;
        audiotankDeath.volume = 0.4;
        audiotankDeath.play();
        blockDME.innerHTML = "";
        blockDME.style.backgroundImage = `url(${urlDME})`;
        blockDME.style.backgroundSize = `100% 100%`;
    }
    else {
        audiotankDeath.currentTime = 0;
        audiotankDeath.volume = 0.2;
        audiotankDeath.play();
        blockDME.src = urlDME;
    }
    setTimeout(() => { blockDME.remove(); }, 200);
}

function addExplosionOnMapRefresh(xExp, yExp) {
    audioshotOnConcrete.currentTime = 0;
    audioshotOnConcrete.play();
    xAEOM = Math.round(xExp * kp);
    yAEOM = Math.round(yExp * kp);
    divAEOM = document.createElement("div");
    divAEOM.innerHTML = document.getElementById(`div-explosion1`).innerHTML;
    blockMap.appendChild(divAEOM);
    divAEOM.style = `width: ${Math.round(15 * kp)}px; height: ${Math.round(15 * kp)}px; top:0; left:0 ;position:absolute;padding: 0;`;
    divAEOM.style.transform = `translate3d(${xAEOM - 7 * kp}px, ${yAEOM-7}px, 0px)`;
    arrGameObj.push(divAEOM);
}

function addShotOnMapRefresh(xShot, yShot, DirectShot) {
    xASOM = Math.round(xShot * kp);
    yASOM = Math.round(yShot * kp);
    divASOM = document.createElement("div");
    blockMap.appendChild(divASOM);
    divASOM.style = `width: ${Math.round(5 * kp)}px; height: ${Math.round(2 * kp)}px; top:0; left:0 ;position:absolute;padding: 0; background-color: #00FFFF;`;
    divASOM.style.transform = `translate3d(${xASOM - 3 * kp}px, ${yASOM}px, 0px) rotateZ(${DirectShot}deg)`;
    arrGameObj.push(divASOM);
}

function addTankOnMapRefresh(tkId, xt, yt, direct, directTower, name, team, invulnerability) {
    if (tkId == 0 && name == player.name) {
        tankOnMove(false);
        motorSound()
        return;
    }
    tankATOM = document.getElementById(`tank_${name}`);
    if (tankATOM != null) {
        if (tkId == 0) {
            tankATOM.hidden = true;
            return;
        }
        else {
            tankATOM.hidden = false;
        }
    }
    if (tkId == 0) {
        return
    }
    if (flagOnOpacity == 0) {
        addOpacity(1);
    }
    xATOM = Math.round(xt * kp);
    yATOM = Math.round(yt * kp);
    if (tankATOM == null) {
        var color;
        var divT = document.createElement("div");
        var textT = document.createElement("div");
        var imgInv1 = document.createElement("img");
        var imgInv2 = document.createElement("img");
        imgInv1.id = `imgInvulnerability1_${name}`;
        imgInv2.id = `imgInvulnerability2_${name}`;
        imgInv1.src = imgInvulnerability1.src;
        imgInv2.src = imgInvulnerability2.src;
        imgInv1.style = `position: absolute; width: ${Math.round(36 * kp)}px; height: ${Math.round(36 * kp)}px; top:${-4 * kp}px; left:${-4 * kp}px`;
        imgInv2.style = `position: absolute; width: ${Math.round(36 * kp)}px; height: ${Math.round(36 * kp)}px; top:${-4 * kp}px; left:${-4 * kp}px`;
        divT.innerHTML = document.getElementById(`tankDiv-${tkId}`).innerHTML;
        blockMap.appendChild(divT);
        divT.style = `width: ${Math.round(28 * kp)}px; height: ${Math.round(28 * kp)}px; top:0; left:0 ;position:absolute;padding: 0;`;
        divT.style.transform = `translate3d(${xATOM - 14 * kp}px, ${yATOM - 14 * kp}px, 0px) rotateZ(${direct}deg)`;
        divT.id = `tank_${name}`;
        var imgW = divT.querySelector(`#imgW-${tkId}`);
        imgW.style = `width: ${Math.round(14 * kp)}px; height: ${Math.round(39 * kp)}px; top:${-5 * kp}px; left:${7 * kp}px`;
        imgW.id = `tower_${name}`;
        imgW.style.transform = `rotateZ(${directTower + 90 - direct}deg)`;
        divT.appendChild(imgInv1);
        divT.appendChild(imgInv2);
        imgInv1.hidden = true;
        imgInv2.hidden = true;
        divT.appendChild(textT);
        if (team == player.team) {
            color = `green`;
        }
        else {
            color = `red`;
        }
        textT.style = `background-color: ${color}; position:absolute; width: ${Math.round(5 * kp)}px; height: ${Math.round(5 * kp)}px; top:${11 * kp}px; left:${11 * kp}px`;
        if (name == player.name) {
            myTank = divT;
            tankOnMove(true);
        }
    }
    else {
        var imgI1 = document.getElementById(`imgInvulnerability1_${name}`);
        var imgI2 = document.getElementById(`imgInvulnerability2_${name}`);
        tankATOM.style = `width: ${Math.round(28 * kp)}px; height: ${Math.round(28 * kp)}px; top:0; left:0 ;position:absolute;padding: 0;`;
        tankATOM.style.transform = `translate3d(${xATOM - 14 * kp}px, ${yATOM - 14 * kp}px, 0px) rotateZ(${direct}deg)`;
        var imgW = tankATOM.querySelector(`#tower_${name}`);
        imgW.style = `width: ${Math.round(14 * kp)}px; height: ${Math.round(39 * kp)}px; top:${-5 * kp}px; left:${7 * kp}px`;
        imgW.style.transform = `rotateZ(${directTower + 90 - direct}deg)`;
        if (invulnerability) {
            if (imgI1.hidden) {
                imgI1.hidden = false;
                imgI2.hidden = true;
            }
            else {
                imgI1.hidden = true;
                imgI2.hidden = false;
            }
        }
        else {
            imgI1.hidden = true;
            imgI2.hidden = true;
        }
    }
    if (name == player.name) {
        directionTower = directTower;
        directionTank = direct;
    }
}

function selectTank(tId) {
    if (divTT == null) {
        flagT = `true`;
    }
    else {
        return;
    }
    tankId = tId;
    document.getElementById("SqSizeRange").disabled = true;
    respEnemyHide(`true`);
    oppacityMapE(`true`);
    respMyHide(`false`, 25,75);
    coordsResp();
    document.removeEventListener(`mousemove`, towerMove);
    blockMap.addEventListener('mousemove', illuminationResp);
    blockMap.addEventListener('mousemove', addTankOnMap);
    blockMap.addEventListener('click', startOnResp);
}

function towerMove(event) {
    if (myTank != null) {
        targetTM = myTank.getBoundingClientRect();
        imgWTM = myTank.querySelector(`#tower_${player.name}`);
        xTower = Math.round((event.clientX - targetTM.left) / percentages * 100) - 14;
        yTower = Math.round((event.clientY - targetTM.top) / percentages * 100) - 14;
        alfaTM = Math.atan(yTower / xTower);
        alfaDegTM = Math.round(alfaTM / Math.PI * 180);
        if (xTower >= 0 && yTower < 0) {
            alfaDegTM = 360 + alfaDegTM;
        }
        if (xTower < 0 && yTower <= 0) {
            alfaDegTM = 180 + alfaDegTM;
        }
        if (xTower < 0 && yTower > 0) {
            alfaDegTM = 180 + alfaDegTM;
        }
        directionTower = alfaDegTM;
        imgWTM.style.transform = `rotateZ(${directionTower + 90-directionTank}deg)`;
    }
}

function mClickFire(event) {
    if (myTank != null) {
        mbtn = 1;
        targetCF = myTank.getBoundingClientRect();
        imgWCF = myTank.querySelector(`#tower_${player.name}`);
        xTower = Math.round((event.clientX - targetCF.left) / percentages * 100) - 14;
        yTower = Math.round((event.clientY - targetCF.top) / percentages * 100) - 14;
        alfaCF = Math.atan(yTower / xTower);
        alfaDegCF = Math.round(alfaCF / Math.PI * 180);
        if (xTower >= 0 && yTower < 0) {
            alfaDegCF = 360 + alfaDegCF;
        }
        if (xTower < 0 && yTower <= 0) {
            alfaDegCF = 180 + alfaDegCF;
        }
        if (xTower < 0 && yTower > 0) {
            alfaDegCF = 180 + alfaDegCF;
        }
        directionTower = alfaDegCF;
        imgWCF.style.transform = `rotateZ(${directionTower + 90 - directionTank}deg)`;
    }
}

function startOnResp() {
    var countI = 0;
    var xI = x - 15;
    var yI = y - 15;
    $.each(arrCoords, function (i, item) {
        if (countI == 4) {
            return false;
        }
        if (xI >= item[0] && xI <= item[0] + 19 && yI >= item[1] && yI <= item[1] + 19) {
            countI++;
        }
        if (xI + 29 >= item[0] && xI + 29 <= item[0] + 19 && yI >= item[1] && yI <= item[1] + 19) {
            countI++;
        }
        if (xI >= item[0] && xI <= item[0] + 19 && yI + 29 >= item[1] && yI + 29 <= item[1] + 19) {
            countI++;
        }
        if (xI + 29 >= item[0] && xI + 29 <= item[0] + 19 && yI + 29 >= item[1] && yI + 29 <= item[1] + 19) {
            countI++;
        }
    });
    if (countI == 4) {
        blockMap.removeEventListener(`mousemove`, illuminationResp);
        blockMap.removeEventListener(`mousemove`, addTankOnMap);
        blockMap.removeEventListener(`click`, startOnResp);
        respMyHide(`true`);
        oppacityMapE(`false`);
        document.getElementById("SqSizeRange").disabled = false;
        divTT.remove();
        $.ajax({
            type: "GET",
            url: `/Game/SelectTankAjax`,
            data: jQuery.param({ x: `${x}`, y: `${y}`, direction: `${direction}`, directionDouble: `${directionDouble}`, tankId: `${tankId}` }),
            success: function () {

            }
        });
    }
}

function addTankOnMap() {
    xAdTOM = Math.round(x * kp);
    yAdTOM = Math.round(y * kp);
    if (flagT == `true`) {
        divTT = document.createElement("div");
        divTT.innerHTML = document.getElementById(`tankDiv-${tankId}`).innerHTML;
        blockMap.appendChild(divTT);
        divTT.style = `width: ${Math.round(28 * kp)}px; height: ${Math.round(28 * kp) }px; top:0; left:0 ;position:absolute;padding: 0;`;
        var imgW = divTT.querySelector(`#imgW-${tankId}`);
        imgW.style = `width: ${Math.round(14 * kp)}px; height: ${Math.round(39 * kp) }px; top:${-5 * kp}px; left:${7 * kp}px`;
        flagT = `false`;
    }
    if (xAdTOM <= mWidth && yAdTOM <= mHeight && x >= 0 && y >= 0) {
        divTT.style.transform = `translate3d(${xAdTOM - 14 * kp}px, ${yAdTOM - 14 * kp}px, 0px) rotateZ(0deg)`;
    }
}

function illuminationResp() {
    var countI = 0;
    var xI = x - 15;
    var yI = y - 15;
    $.each(arrCoords, function (i, item) {
        if (countI == 4) {
            return false;
        }
        if (xI >= item[0] && xI <= item[0] + 19 && yI >= item[1] && yI <= item[1] + 19) {
            countI++;
        }
        if (xI + 29 >= item[0] && xI + 29 <= item[0] + 19 && yI >= item[1] && yI <= item[1] + 19) {
            countI++;
        }
        if (xI >= item[0] && xI <= item[0] + 19 && yI + 29 >= item[1] && yI + 29 <= item[1] + 19) {
            countI++;
        }
        if (xI + 29 >= item[0] && xI + 29 <= item[0] + 19 && yI + 29 >= item[1] && yI + 29 <= item[1] + 19) {
            countI++;
        }
    });
    if (countI == 4) {
        respMyHide(`false`, 75, 100);
    }
    else {
        respMyHide(`false`, 100, 75);
    }
}

function coordsResp() {
    arrCoords = [];
    var rBlock;
    var target;
    var bMap = blockMap.getBoundingClientRect();
    respArr.forEach(function (item) {
        if (item[0] == player.team) {
            rBlock = document.getElementById(`${item[1]}`)
            target = rBlock.getBoundingClientRect();
            var xr = Math.round((target.left - bMap.left) / percentages * 100);
            var yr = Math.round((target.top - bMap.top) / percentages * 100);
            arrCoords.push([xr,yr]);
        }
    });
}

function createMapElement(w, h) {
    var count = w * h;
    blockMap.innerHTML = "";
    mWidth = w * sizeSq;
    mHeight = h * sizeSq;
    blockBigMap.style.width = w * sizeSq + "px";
    blockBigMap.style.height = h * sizeSq + "px";
    blockMap.style.width = w * sizeSq + "px";
    blockMap.style.height = h * sizeSq + "px";
    for (let i = 0; i < count; i++) {
        var mapElement = document.createElement("div");
        blockMap.appendChild(mapElement);
        mapElement.style.width = sizeSq + "px";
        mapElement.style.height = sizeSq + "px";
        mapElement.id = `me${i}`;
        mapElement.className = "bg-black p-0 m-0 position-relative";
    }
}

function mapElementRedact(background, src, id = "") {
    var block;
    if (background == `True`) {
        block = document.getElementById(id.replace("bg", ""));
        block.innerHTML = "";
        var img = document.createElement("img");
        block.appendChild(img);
        img.id = id;
        img.setAttribute(`src`, `${src}`);
        img.className = "w-100 h-100 user-select-none position-absolute";
        mebgArr.push(img);
    }
    else {
        block = document.getElementById(id.replace("cv", "bg"));
        var img = document.createElement("img");
        block.after(img);
        img.id = `${id}`;
        img.setAttribute(`src`, `${src}`);
        img.className = "w-100 h-100 user-select-none position-absolute";
        mecvArr.push(img);
    }
}

function render() {
    var value, src, w, h, width, heigth, mapPointListBG, mapPointListCV, removeListCV, jsonRemoveListCV, jsonObjBG, jsonObjCV, imgBlock, chatMesList, jsonChatMesList;
    width = document.getElementById(`width`).value;
    heigth = document.getElementById(`height`).value;
    mapPointListBG = document.getElementById(`MapPointListBG`).value;
    mapPointListCV = document.getElementById(`MapPointListCV`).value;
    removeListCV = document.getElementById(`removeCVGameObjmapElements`).value;
    chatMesList = document.getElementById(`chatMesList`).value;
    if (mapPointListBG == "" || mapPointListCV == "") {
        errorRend(0);
        return;
    }
    jsonObjBG = JSON.parse(mapPointListBG);
    jsonObjCV = JSON.parse(mapPointListCV);
    jsonRemoveListCV = JSON.parse(removeListCV);
    jsonChatMesList = JSON.parse(chatMesList);
    if (invalidateRand(jsonObjBG, jsonObjCV)) {
        errorRend(1);
        return;
    }
    w = width / 20;
    h = heigth / 20;
    createMapElement(w, h);
    for (var key in jsonObjBG) {
        if (jsonObjBG[key] == "") {
            continue;
        }
        value = jsonObjBG[key];
        src = document.getElementById(`imgBG-${value}`).src;
        mapElementRedact(`True`, src, key);
    }
    for (var key in jsonObjCV) {
        if (jsonObjCV[key] == "") {
            continue;
        }
        value = jsonObjCV[key];
        imgBlock = document.getElementById(`imgCV-${value}`);
        if (imgBlock.hasAttribute("resp")) {
            respArrFunc(imgBlock.getAttribute("resp"), key);
        }
        src = imgBlock.src;
        mapElementRedact(`False`, src, key);
    }
    jsonRemoveListCV.forEach(function (value) {
        deleteMapElement(value, true);
    });
    jsonChatMesList.forEach(function (value) {
        addChatData(value);
    });
}

function respArrFunc(respTeam, key) {
    
    respArr.push([respTeam, key]);
}

function respEnemyHide(flag) {
    var rBlock
    if (flag == `true`) {
        respArr.forEach(function (item, i) {
            if (item[0] != player.team) {
                rBlock = document.getElementById(`${item[1]}`);
                rBlock.style.visibility = `hidden`;
            }
        });
    }
    else {
        respArr.forEach(function (item, i) {
            if (item[0] != player.team) {
                rBlock = document.getElementById(`${item[1]}`);
                rBlock.style.visibility = 'visible';
            }
        });
    }
}

function respMyHide(flag, blvl = 0, alvl = 0) {
    var rBlock
    if (flag == `true`) {
        respArr.forEach(function (item, i) {
            if (item[0] == player.team) {
                rBlock = document.getElementById(`${item[1]}`);
                rBlock.style.visibility = `hidden`;
            }
        });
    }
    else {
        if (blvl == 0 && alvl == 0) {
            respArr.forEach(function (item, i) {
                if (item[0] == player.team) {
                    rBlock = document.getElementById(`${item[1]}`);
                    rBlock.style.visibility = 'visible';
                    rBlock.classList.remove(`opacity-25`)
                    rBlock.classList.add(`border`);
                }
            });
        }
        else {
            respArr.forEach(function (item, i) {
                if (item[0] == player.team) {
                    rBlock = document.getElementById(`${item[1]}`);
                    rBlock.style.visibility = 'visible';
                    rBlock.classList.remove(`opacity-${blvl}`)
                    rBlock.classList.add(`border`);
                    rBlock.classList.add(`opacity-${alvl}`);
                }
            });
        }
    }
}

function oppacityMapE(flag) {
    if (flag == `true`) {
        mebgArr.forEach(function (item) {
            item.classList.add(`opacity-25`)
        })
        mecvArr.forEach(function (item) {
            item.classList.add(`opacity-25`)
        })
    }
    else {
        mebgArr.forEach(function (item) {
            item.classList.remove(`opacity-25`)
        })
        mecvArr.forEach(function (item) {
            item.classList.remove(`opacity-25`)
        })
    }
}

function invalidateRand(jsonObjBG, jsonObjCV) {
    for (var key in jsonObjBG) {
        if (jsonObjBG[key] == "") {
            continue;
        }
        if (document.getElementById(`imgBG-${jsonObjBG[key]}`) == null) {
            return true
        }
    }
    for (var key in jsonObjCV) {
        if (jsonObjCV[key] == "") {
            continue;
        }
        if (document.getElementById(`imgCV-${jsonObjCV[key]}`) == null) {
            return true
        }
    }
    return false
}

function errorRend(er) {
    if (er == 0) {
        Swal.fire(
            'Error!',
            'An error occurred when building the map. The array of map elements is missing.',
            'error'
        )
    }
    if(er==1) {
        Swal.fire(
            'Error!',
            'An error occurred when building the map. The required map element is missing.',
            'error'
        )
    }
}
