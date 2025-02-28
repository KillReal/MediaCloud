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

function updateModelLoadBodyWithErrorMessage(data) {
    orderPosElem.innerHTML = data.completionMessage;
    mediaCountElem.innerHTML = '';
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

function uploadFile(event) {
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
                    if (data.isCompleted == true) {
                        if (data.isSuccess == true)
                        {
                            setModalTitle('File(-s) successfully uploaded!');
                            hideModalBody();
                        }
                        else
                        {
                            setModalTitle('An error occured during uploading :(');
                            updateModelLoadBodyWithErrorMessage(data);
                        }
                        hideLoadSpinner();

                        clearInterval(loadingUpdateInterval);
                    }
                    else if (data.isInProgress == true) {
                        if (data.workCount == 0 && data.isCompleted == false) {
                            setModalTitle('Saving in database...');
                            showLoadSpinner();
                        }
                        else if (data.workCount > 0) {
                            setModalTitle('Server processing...');
                        }
                        else if (data.queuePosition > 1) {
                            setModalTitle('Waiting queue for processing...');
                        }
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

function formSubmit(event) {
    var url = "/Gallery/IsCanUploadFiles";
    var request = new XMLHttpRequest();
    request.open('POST', url, true);
    request.onload = function() {
        var data = JSON.parse(request.response);

        if (data.success === true) {
            uploadFile(event);
            return;
        }
        
        hideModalBody();
        hideLoadSpinner();
        setModalTitle("File(-s) couldn't be uploaded");
        setModalTitle(data.message);
        $('#loadingModal').modal('show');

        event.preventDefault();
    };

    request.onerror = function() {
        hideModalBody();
        hideLoadSpinner();
        setModalTitle("File(-s) couldn't be uploaded");
        setModalTitle("Server doesn't respond in time");
        $('#loadingModal').modal('show');

        event.preventDefault();
    };
    
    var fileSizes = []
    var files = event.target[0].files;
    
    for (var i = 0; i < files.length; i++) {
        fileSizes.push(files[i].size);
    }
    
    request.send(fileSizes);
    event.preventDefault();
}

    // and you can attach form submit event like this for example
function attachFormSubmitEvent(formId){
    document.getElementById(formId).addEventListener("submit", formSubmit);
}

attachFormSubmitEvent('uploadForm');