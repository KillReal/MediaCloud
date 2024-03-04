let autocomplete = (inp) => {
    /*the autocomplete function takes two arguments,
    the text field element and an array of possible autocompleted values:*/
    let currentFocus;
    inp.addEventListener("input", function (e) {
        let a, //OUTER html: variable for listed content with html-content
            b, // INNER html: filled with array-Data and html
            i, //Counter
            val = this.value.replaceAll("!", "").split(' ').pop();

        closeAllLists();

        if (!val) {
            return false;
        }

        currentFocus = -1;

        a = document.createElement("DIV");

        a.setAttribute("id", this.id + "autocomplete-list");
        a.setAttribute("class", "autocomplete-items list-group text-left");
        a.setAttribute("style", "max-width: 15rem; padding-left: 0.8rem");

        this.parentNode.appendChild(a);

        var url = "/Gallery/GetSuggestions?searchString=" + val + '&limit=7';
        fetch(url).then(function (response) {
            return response.json();
        }).then(function (data) {

            /*for each item in the array...*/
            for (i = 0; i < data.length; i++) {
                if (data[i].substr(0, val.length).toUpperCase() == val.toUpperCase()) {
                    b = document.createElement("DIV");
                    b.setAttribute("class", "list-group-item list-group-item-action");
                    b.innerHTML = "<strong>" + data[i].substr(0, val.length) + "</strong>";
                    b.innerHTML += data[i].substr(val.length);
                    b.innerHTML += "<input type='hidden' value='" + data[i] + "'>";
                    b.addEventListener("click", function (e) {
                        var inputs = inp.value.split(' ');
                        var tag = this.getElementsByTagName("input")[0].value.toLowerCase() + ' ';
                        if (inputs[inputs.length - 1].includes("!")) {
                            tag = "!" + tag;
                        }
                        inputs[inputs.length - 1] = tag;
                        inp.value = inputs.join(' ');
                        closeAllLists();
                    });
                    a.appendChild(b);
                }
            }
        }).catch(function (err) {
            console.log('Fetch Error :-S', err);
        });

    });

    /*execute a function presses a key on the keyboard:*/
    inp.addEventListener("keydown", function (e) {
        var x = document.getElementById(this.id + "autocomplete-list");
        if (x) x = x.getElementsByTagName("div");
        if (e.keyCode == 40) {
            currentFocus++;
            addActive(x);
        } else if (e.keyCode == 38) {
            currentFocus--;
            addActive(x);
        } else if (e.keyCode == 13) {
            e.preventDefault();
            if (currentFocus > -1) {
                if (x) x[currentFocus].click();
            }
        } else if (e.keyCode == 9) {
            e.preventDefault();
            x[0].click();
            this.focus();
        }
    });

    let addActive = (x) => {
        if (!x) return false;
        removeActive(x);
        if (currentFocus >= x.length) currentFocus = 0;
        if (currentFocus < 0) currentFocus = x.length - 1;
        x[currentFocus].classList.add("active");
    }

    let removeActive = (x) => {
        for (let i = 0; i < x.length; i++) {
            x[i].classList.remove("active");
        }
    }

    let closeAllLists = (elmnt) => {
        var x = document.getElementsByClassName("autocomplete-items");
        for (var i = 0; i < x.length; i++) {
            if (elmnt != x[i] && elmnt != inp) {
                x[i].parentNode.removeChild(x[i]);
            }
        }
    }

    document.addEventListener("click", function (e) {
        closeAllLists(e.target);
    });
}

autocomplete(document.getElementById("filterGrab"));