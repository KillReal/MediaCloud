var orderPosElem = document.getElementById('orderPos');
var mediaCountElem = document.getElementById('blobsCount');
var modalTitle = document.getElementById('modalTitle');
var modalBody = document.getElementById("modalBody");
var loadSpinner = document.getElementById("loadingSpinner");

function updateModalLoadBody(data) {
    if (data == undefined) {
        orderPosElem.innerHTML = 'Your position in order: updating...';
        mediaCountElem.innerHTML = 'Gallery count: updating...';
    }
    else {
        orderPosElem.innerHTML = 'Your position in order: ' + data.queuePosition;
        mediaCountElem.innerHTML = 'Gallery count: ' + data.workCount;
    }
}

function setModalTitle(title) {
    modalTitle.innerHTML = title;
}

function hideModalBody() {
    modalBody.style.display = "none";
}

function showModalBody() {
    modalBody.style.display = "block";
}

function showLoadSpinner() {
    loadSpinner.style.display = "block";
}

function hideLoadSpinner() {
    loadSpinner.style.display = "none";
}

function formSubmit(event) {
    setModalTitle('Sending to server...');
    showModalBody();
    updateModalLoadBody();
    $('#loadingModal').modal('show');

    var url = "/Gallery/Upload";
    var request = new XMLHttpRequest();
    request.open('POST', url, true);
    request.onload = function() { // request successful
        setModalTitle('Waiting queue for processing...');
        showModalBody();
        updateModalLoadBody();
        var data = JSON.parse(request.response);

        var url = "/TaskScheduler/GetTaskStatus?id=" + data.id;
        
        console.log('Goes to update status');

        const updator = function () {
            fetch(url)
                .then((response) => response.json())
                .then(function (data) {
                    updateModalLoadBody(data);
                    if (data.queuePosition > 1) {
                        setModalTitle('Waiting queue for processing...');
                    }
                    else if (data.queuePosition == 1) {
                        setModalTitle('Server processing...');
                    }
                    else if (data.workCount == 0 && data.isExist) {
                        setModalTitle('Saving in database...');
                        showLoadSpinner();
                    }
                    else if (data.workCount == 0 && data.isExist == false) {
                        setModalTitle('File(-s) successfully uploaded!');
                        hideModalBody();
                        hideLoadSpinner();

                        clearInterval(loadingUpdateInterval);
                    }
                })
        }

        updator();
        var loadingUpdateInterval = setInterval(updator, 1500);
    };

    request.onerror = function() {
    // request failed
    };

    request.send(new FormData(event.target)); // create FormData from form that triggered event
    event.preventDefault();
}

    // and you can attach form submit event like this for example
function attachFormSubmitEvent(formId){
    document.getElementById(formId).addEventListener("submit", formSubmit);
}

attachFormSubmitEvent('uploadForm');