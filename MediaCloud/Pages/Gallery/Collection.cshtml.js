function startReorder() {
    document.getElementById('order-changed').value = true;
    document.getElementById('reorder-button').style.display = "none";
    document.getElementById('sortable-gallery').style.display = "flex";
    document.getElementById('gallery').style.display = "none";
}

function CreateImg(content) {
    //Create new picture element
    var img = document.createElement('img');
    img.className = "w-100 shadow-1-strong";
    img.style = "border-radius: 0.5rem";
    img.src = "data:image/png;base64," + content;
    
    return img;
}

function CreateActualCard(data, id, collectionId) {
    // Create new actual card
    var cardDiv = document.createElement('div');
    cardDiv.className = "picture-card picture shadow-1-strong col-md-4";
    cardDiv.id = id;
    cardDiv.onclick = function (e) {
        var id = e.currentTarget.id;
        window.location = "Detail?id=" + data[id].id + "&returnUrl=/Medias/Collection?id=" + collectionId + "&rootReturnUrl=" + rootReturnUrl;
    }

    // With image
    var img = CreateImg(data[id].content);

    cardDiv.appendChild(img);

    return cardDiv;
}

function CreateSortCard(data, id, orderId) {
    // Create new sort card
    var sortCardDiv = document.createElement('div');
    sortCardDiv.className = "picture-card shadow-1-strong col-md-2";
    sortCardDiv.draggable = true;
    sortCardInnerDiv = document.createElement('div');
    sortCardInnerDiv.className = "card list-clickable p-2 pb-4 d-flex flex-column align-items-center justify-content-between thumbnail";
    sortCardInnerDiv.style = "border-radius: 0.75rem";

    // With image
    var img = CreateImg(data[id].content);

    // Create sort input iterator
    var sortInput = document.createElement('input');
    sortInput.className = "order-val visually-hidden";
    sortInput.type = "number";
    sortInput.id = "Orders_" + orderId;
    sortInput.name = "Orders[" + orderId + "]";
    sortInput.value = data[id].order;

    sortCardInnerDiv.appendChild(img);
    sortCardInnerDiv.appendChild(sortInput);
    sortCardDiv.appendChild(sortCardInnerDiv);

    return sortCardDiv;
}