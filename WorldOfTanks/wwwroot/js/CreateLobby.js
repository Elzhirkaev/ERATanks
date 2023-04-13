var blockMap = document.getElementById("mapBlock");
var blockLobbyMapId = document.getElementById(`GameLobby_MapId`);
var blockLobbyNumOfTeams = document.getElementById(`GameLobby_NumOfTeams`);
var blockMessageError = document.getElementById(`messageError`);
var sizeSq = 20;

blockLobbyMapId.value = 0;

function validMap(event) {
    if (blockLobbyMapId.value <= 0) {
        event.preventDefault();
        blockMessageError.innerHTML = `ERROR : Select a map`;
    }
}

function createMapElement(w, h) {
    var count = w * h;
    blockMap.innerHTML = "";
    blockMap.style.width = w * sizeSq + "px";
    blockMap.style.height = h * sizeSq + "px";
    for (let i = 0; i < count; i++) {
        var mapElement = document.createElement("div");
        blockMap.appendChild(mapElement);
        mapElement.style.width = sizeSq + "px";
        mapElement.style.height = sizeSq + "px";
        mapElement.id = `me${i}`;
        mapElement.className = "p-0 m-0 position-relative";
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
    }
    else {
        block = document.getElementById(id.replace("cv", "bg"));
        var img = document.createElement("img");
        block.after(img);
        img.id = `${id}`;
        img.setAttribute(`src`, `${src}`);
        img.className = "w-100 h-100 user-select-none position-absolute";
    }
}

function render(id,numOT) {
    var value, src, w, h, width, heigth, mapPointListBG, mapPointListCV, jsonObjBG, jsonObjCV;
    width = document.getElementById(`width_${id}`).value;
    heigth = document.getElementById(`height_${id}`).value;
    mapPointListBG = document.getElementById(`MapPointListBG_${id}`).value;
    mapPointListCV = document.getElementById(`MapPointListCV_${id}`).value;
    blockLobbyMapId.value = id;
    blockLobbyNumOfTeams.value = numOT;
    if (mapPointListBG == "" || mapPointListCV == "") {
        errorRend(0);
        return;
    }
    jsonObjBG = JSON.parse(mapPointListBG);
    jsonObjCV = JSON.parse(mapPointListCV);
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
        src = document.getElementById(`imgCV-${value}`).src;
        mapElementRedact(`False`, src, key);
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
