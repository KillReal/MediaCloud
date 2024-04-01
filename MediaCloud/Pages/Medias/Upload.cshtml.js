var orderPosElem = document.getElementById('orderPos');
    var mediaCountElem = document.getElementById('mediasCount');
    var modalTitle = document.getElementById('modalTitle');
    var modalBody = document.getElementById("modalBody");
    var loadSpinner = document.getElementById("loadingSpinner");
    
    function updateModalLoadBody(data) {
        if (data == undefined) {
            orderPosElem.innerHTML = 'Your position in order: updating...';
            mediaCountElem.innerHTML = 'Medias count: updating...';
        }
        else {
            orderPosElem.innerHTML = 'Your position in order: ' + data.queuePosition;
            mediaCountElem.innerHTML = 'Medias count: ' + data.workCount;
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

    function showLoadSpinner()
    {
        loadSpinner.style.display = "block";
    }

    function hideLoadSpinner()
    {
        loadSpinner.style.display = "none";
    }
    
    function formSubmit(event) {
        setModalTitle('Sending to server...');
        showModalBody();
        updateModalLoadBody();

        var url = "/Medias/Upload";
        var request = new XMLHttpRequest();
        request.open('POST', url, true);
        request.onload = function() { // request successful
            setModalTitle('Waiting queue for processing...');
            showModalBody();
            updateModalLoadBody();
            $('#loadingModal').modal('show');
            var data = JSON.parse(request.response);

            var url = "/Uploader/GetTaskStatus?id=" + data.id;
            
            console.log('Goes to update status');

            const updator = function () {
                fetch(url)
                    .then((response) => response.json())
                    .then(function (data) {
                        updateModalLoadBody(data);
                        if (data.queuePosition > 1) {
                            setModalTitle('Waiting queue for processing...');
                        }
                        else if (data.queuePosition = 0 && data.workCount > 0) {
                            setModalTitle('Server processing...');
                        }
                        else if (data.workCount == 0 && data.isInProgress == false) {
                            setModalTitle('Media successfully uploaded!');
                            hideModalBody();
                            hideLoadSpinner();

                            clearInterval(loadingUpdateInterval);
                        }
                        else if (data.workCount == 0) {
                            setModalTitle('Saving in database...');
                            showLoadSpinner();
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