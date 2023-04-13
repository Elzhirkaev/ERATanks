var blockMap = document.getElementById("mapBlock");
var mapPointListBG = document.getElementById(`Map_MapPointListBG`);
var mapPointListCV = document.getElementById(`Map_MapPointListCV`);
var jsonObjBG;
var jsonObjCV;
var mapId = document.getElementById(`Map_MapId`);
var width;
var heigth;
var square;
var sizeSq = 20;
var count = width * heigth;
var jsonArrMapElement = {};
var jsonArrCoverageElement = {};
let arr = [];

if (mapPointListBG.value != "" || mapPointListCV.value != "") {
    jsonObjBG = JSON.parse(mapPointListBG.value);
    jsonObjCV = JSON.parse(mapPointListCV.value);
}

if (mapId.value != 0) {
    square = document.getElementById("mapWidth").value / 20 * document.getElementById("mapHeight").value / 20;
    render();
}
else {
    width = 30;
    heigth = 30;
    document.getElementById("mapWidth").value = width * 20;
    document.getElementById("mapHeight").value = heigth * 20;
    createMapElement(width, heigth);
}

if (document.getElementById("SqSizeRange") != null) {
    document.getElementById("SqSizeRange").addEventListener("change", function () {
        sizeSq = this.value;
        document.getElementById("sqSize").innerHTML = ": " + Math.round(sizeSq / 20 * 100) + `%`;
        if (mapId.value != 0) {

            if (square <= width * heigth) {
                render();
            }
            else {
                createMapElement(width, heigth);
            }
        }
        else {
            createMapElement(width, heigth);
        }
    });
}

document.getElementById("mapWidth").addEventListener("change", function () {
    width = this.value / 20;
    if (mapId.value != 0) {

        if (square <= width * heigth) {
            render();
        }
        else {
            createMapElement(width, heigth);
        }
    }
    else {
        createMapElement(width, heigth);
    }
});

document.getElementById("mapHeight").addEventListener("change", function () {
    heigth = this.value / 20;
    if (mapId.value != 0) {
        
        if (square <= width * heigth) {
            render();
        }
        else {
            createMapElement(width, heigth);
        }
    }
    else {
        createMapElement(width, heigth);
    }
});

function createMapElement(w, h, rand = `False`) {
    count = w * h;
    arr = [];
    jsonArrMapElement = {};
    jsonArrCoverageElement = {};

    document.getElementById("mapBlock").innerHTML = "";
    blockMap.style.width = w * sizeSq+1 + "px";
    blockMap.style.height = h * sizeSq+1 + "px";
    document.getElementById("displayWidth").innerHTML = ": " + w * 20;
    document.getElementById("displayHeight").innerHTML = ": " + h * 20;


    for (let i = 0; i < count; i++) {

        jsonArrMapElement[`bgme${i}`] = ``;
        jsonArrCoverageElement[`cvme${i}`] = ``;
        var mapElement = document.createElement("div");
        blockMap.appendChild(mapElement);
        mapElement.style.width = sizeSq + "px";
        mapElement.style.height = sizeSq + "px";
        mapElement.id = `me${i}`;
        mapElement.className = "border-top border-start bg-light p-0 m-0 position-relative";
        arr.push(mapElement);
    }
    if (rand == `True`) {
        return;
    }
    mapPointListBG.value = JSON.stringify(Object.assign({}, jsonArrMapElement));
    mapPointListCV.value = JSON.stringify(Object.assign({}, jsonArrCoverageElement));

}

function mapElementRedact(background, mapElementId, src, id = "") {
    var block;
    if (id != "") {

        if (background == `True`) {
            block = document.getElementById(id.replace("bg", ""));
            block.innerHTML = "";
            jsonArrMapElement[`${id}`] = `${mapElementId}`;
            var img = document.createElement("img");
            block.appendChild(img);
            img.id = id;
            img.setAttribute(`src`, `${src}`);
            img.className = "w-100 h-100 user-select-none position-absolute";
        }
        else {
            block = document.getElementById(id.replace("cv", "bg"));
            jsonArrCoverageElement[`${id.replace("bg", "cv")}`] = `${mapElementId}`;
            var img = document.createElement("img");
            block.after(img);
            img.id = "Coverage";
            img.setAttribute(`src`, `${src}`);
            img.className = "w-100 h-100 user-select-none position-absolute";
        }
    }
    else {
        mapElementMouseRedact(background, mapElementId, src);
    }
}

function mapElementMouseRedact(background, mapElementId, src) {
    var block;
    var buffer;
    blockMap.onclick = function (event) {
        block = event.target;
        if (background == `True`) {
            if (block.tagName == "IMG" && block.id != "Coverage") {
                if (jsonArrMapElement[`${block.id}`] != mapElementId) {
                    jsonArrMapElement[`${block.id}`] = mapElementId;
                    var img = document.createElement("img");
                    block.after(img);
                    img.id = `${block.id}`;
                    img.setAttribute(`src`, `${src}`);
                    img.className = "w-100 h-100 user-select-none position-absolute";
                    block.remove();
                }
                else {
                    jsonArrMapElement[`${block.id}`] = ``;
                    block.remove();
                }
            }
            if (block.tagName == "IMG" && block.id == "Coverage") {
                buffer = block.previousElementSibling;
                jsonArrMapElement[`${buffer.id}`] = ``;
                jsonArrCoverageElement[`${buffer.id.replace("bg", "cv")}`] = ``;
                buffer.remove();
                block.remove();
            }
            if (block.children.length == 0 && block.tagName == "DIV") {
                block.innerHTML = "";
                jsonArrMapElement[`bg${block.id}`] = `${mapElementId}`;
                var img = document.createElement("img");
                block.appendChild(img);
                img.id = "bg" + block.id;
                img.setAttribute(`src`, `${src}`);
                img.className = "w-100 h-100 user-select-none position-absolute";
            }

        }
        else {
            if (block.tagName == "DIV" && block.children.length == 0) {
                insertBG();
                block.innerHTML = "BG";
            }
            if (block.tagName == "IMG" && block.id == "Coverage") {
                buffer = block.previousElementSibling;
                jsonArrCoverageElement[`${buffer.id.replace("bg", "cv")}`] = ``;
                block.remove();
            }
            if (block.tagName == "IMG" && block.id != "Coverage") {

                jsonArrCoverageElement[`${block.id.replace("bg", "cv")}`] = `${mapElementId}`;
                var img = document.createElement("img");
                block.after(img);
                img.id = "Coverage";
                img.setAttribute(`src`, `${src}`);
                img.className = "w-100 h-100 user-select-none position-absolute";
            }
        }
        mapPointListBG.value = JSON.stringify(Object.assign({}, jsonArrMapElement));
        mapPointListCV.value = JSON.stringify(Object.assign({}, jsonArrCoverageElement));
    }
    document.onmouseup = function () {
        blockMap.onmousemove = null;
    }
    blockMap.onmousedown = function () {
        blockMap.onmousemove = function (event) {
            block = event.target;
            if (background == `True`) {
                if (block.tagName == "IMG" && block.id != "Coverage") {

                    if (jsonArrMapElement[`${block.id}`] != mapElementId) {
                        jsonArrMapElement[`${block.id}`] = mapElementId;
                        var img = document.createElement("img");
                        block.after(img);
                        img.id = `${block.id}`;
                        img.setAttribute(`src`, `${src}`);
                        img.className = "w-100 h-100 user-select-none position-absolute";
                        block.remove();
                    }
                }
                
                if (block.tagName == "IMG" && block.id == "Coverage") {

                    buffer = block.previousElementSibling;
                    jsonArrMapElement[`${buffer.id}`] = ``;
                    jsonArrCoverageElement[`${buffer.id.replace("bg", "cv")}`] = ``;
                    buffer.remove();
                    block.remove();
                }
                if (block.children.length == 0 && block.tagName == "DIV") {

                    block.innerHTML = "";
                    jsonArrMapElement[`bg${block.id}`] = `${mapElementId}`;
                    var img = document.createElement("img");
                    block.appendChild(img);
                    img.id = "bg" + block.id;
                    img.setAttribute(`src`, `${src}`);
                    img.className = "w-100 h-100 user-select-none position-absolute";
                }

            }
            else {
                if (block.tagName == "IMG" && block.id != "Coverage") {

                    jsonArrCoverageElement[`${block.id.replace("bg", "cv")}`] = `${mapElementId}`;
                    var img = document.createElement("img");
                    block.after(img);
                    img.id = "Coverage";
                    img.setAttribute(`src`, `${src}`);
                    img.className = "w-100 h-100 user-select-none position-absolute";
                }
            }
            mapPointListBG.value = JSON.stringify(Object.assign({}, jsonArrMapElement));
            mapPointListCV.value = JSON.stringify(Object.assign({}, jsonArrCoverageElement));
        }
    }
}

function render() {
    if (mapPointListBG.value == "" || mapPointListCV.value == "") {
        return;
    }
    if (invalidateRand()) {
        errorRend();
        return;
    }
    var value, src;
    width = document.getElementById("mapWidth").value / 20;
    heigth = document.getElementById("mapHeight").value / 20;
    createMapElement(width, heigth, `True`);
    for (var key in jsonObjBG) {
        if (jsonObjBG[key] == "") {
            continue;
        }
        value = jsonObjBG[key];
        src = document.getElementById(`imgBG-${value}`).src;
        mapElementRedact(`True`, value, src, key)
    }
    for (var key in jsonObjCV) {
        if (jsonObjCV[key] == "") {
            continue;
        }
        value = jsonObjCV[key];
        src = document.getElementById(`imgCV-${value}`).src;
        mapElementRedact(`False`, value, src, key)
    }
}



function insertBG() {
    Swal.fire(
        'Error!',
        'Please apply the Background first',
        'error'
    )
}

function invalidateRand() {
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
function errorRend() {
    Swal.fire(
        'Error!',
        'An error occurred when building the map. The required map element is missing.',
        'error'
    )
}

function validateInput() {
    let a = 0;
    var jsonObjBG = JSON.parse(mapPointListBG.value);
    for (var key in jsonObjBG) {

        if (jsonObjBG[key] == "") {
            a++;
        }
    }
    if (a != 0) {
        Swal.fire(
            'Error!',
            'Please fill in all the cells.',
            'error'
        )
        return false
    }
    return true
}