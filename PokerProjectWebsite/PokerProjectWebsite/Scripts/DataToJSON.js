const _CollectSettings = function () {
    //Collect data from fields on the page that are of class 'stored'
    "use strict";

    let array = [];
    let i = 0;

    for (let e of document.querySelectorAll('.stored')) {
        console.log(e);
        array[i] = {'Instelling': e.id, 'Waarde': e.value};
        i++;
    }
    return array
}

const _CheckIfSettingsFilled = function () {
    "use strict";
    for (let e of document.querySelectorAll('.stored')) {
        console.log(e);
        console.log("Checking...")
        if (!e || !e.value || e.value == null || e.value == "") {
            alert("Een instelling is niet ingevuld: " + e.placeholder)
            return false;
        }
    }
    return true;
}

const _CollectBlindData = function () {
    //Collect data from the blinds table
    "use strict";

    let blindTable = document.getElementById('blindtable');

    let l = []; // array
    let i = 0;  // iterator

    //Iterate through rows of blindtable
    for (let e of blindTable.children) {
        let j = 0; //iterator
        l[i] = {};
        l[i].Ronde = i + 1;

        // Iterate through columns of row, retrieving data
        for (let z of e.children) {
            switch (j) {
                case 0:
                    if (z.firstChild.value == "Pauze") {
                        l[i]["Pauze"] = 'True'
                    }
                    else { l[i]["Pauze"] = 'False' }

                    break;
                case 1:
                    l[i]["SmallBlind"] = z.firstChild.value;
                    break;
                case 2:
                    l[i]["BigBlind"] = z.firstChild.value;
                    break;
                case 3:
                    l[i]["Duratie"] = z.firstChild.value;
                    break;
                default:
                    break;
            }
            j++
        }
        i++;
    }
    return l;
}

const _CollectPersonData = function () {
    "use strict";

    //Get table entries
    let e = document.getElementById('spelerstable').children;
    
    // Returned array
    let a = [];
    // Iterator
    let i = 0;

    for (let z of e) {

        let firstName = z.children[0].firstChild.nodeValue;
        let secondName = z.children[1].firstChild.nodeValue;
        a[i] = {'Voornaam': firstName, 'Achternaam': secondName};

        i++;
    }
    return a;
}
const _CollectFicheData = function () {
    "use strict";
    
    //Get table entries
    let e = document.getElementById('ficheHolder');
    
    // Returned array
    let a = [];
    // Iterator
    let i = 0;
    for (let fiche of e.children) {
        let data = fiche.dataset;
        
        a[i] = {'Kleur': data.kleur, 'Waarde': data.waarde};
        i++;
        
    }
    return a;
}

const _Collect = function () {
    //Collect data from child functions, returns as dictionary
    "use strict";
    let collected = {}
    if (_CheckIfSettingsFilled()) {
        
        collected.Blinds = _CollectBlindData();
        collected.Spelers = _CollectPersonData();
        collected.Fiche = _CollectFicheData();
        collected.Instellingen = _CollectSettings();
        
    }
    return collected;
}



const _CollectAsJson = function () {
    "use strict";
    let newestData = _Collect();
    let json = JSON.stringify(newestData)
    console.log(json);
    return json
}

$('#dataForm').submit(function () {
    //Set form data to return value of '_CollectAsJson'
    "use strict";
    if (_CheckIfSettingsFilled()) {
        document.getElementById('dataInput').value = _CollectAsJson();
        return true;
    }
    return false;
});

const _GetFiche = function () {
    "use strict";
    const kleur = document.getElementById("kleurkeuze");
    const selectedkleur = kleur.options[kleur.selectedIndex].value;

    const waarde = document.getElementById("waardekeuze");
    const selectedwaarde = waarde.options[waarde.selectedIndex].value;
    return {'kleur' : selectedkleur, 'waarde' : selectedwaarde};
}

const _RemoveItem = function(element) {
    "use strict";
    element.parentNode.removeChild(element);
}

const _GenerateFicheImage = function (fiche) {
    "use strict";
    const source = "/Images/fiches/" + fiche.kleur + "/" + fiche.waarde + ".png";
    let img = document.createElement('img');
    img.width = 140;
    img.height = 140;
    img.src = source;
    img.onclick = function () { _RemoveItem(this); };
    img.setAttribute('data-kleur', fiche.kleur);
    img.setAttribute('data-waarde', fiche.waarde);
    return img;
}

const _AddFiche = function (fiche) {
    //Add fiche to ficheHolder. Sorts automagically.
    "use strict";
    const img = _GenerateFicheImage(fiche)
    const holder = document.getElementById('ficheHolder')

    // When there's a fiche that has a larger value then you have, go stand in front of it
    for (const child of holder.children) {
        if (Number(child.dataset.waarde) > Number(img.dataset.waarde)) {
            holder.insertBefore(img, child);
            return(true);
        }
    }
    holder.appendChild(img);
}

const plus1speler = function () {
    document.getElementById("aantalMensen").innerHTML = Number(document.getElementById("aantalMensen").innerHTML) + 1
}

const min1speler = function () {
    document.getElementById("aantalMensen").innerHTML = Number(document.getElementById("aantalMensen").innerHTML) - 1
}


function getlastrowdata(tableID, pauzeblind) {
    let Duratie;
    var waarde2;
    var waarde3;
    var blindfactor;
    var rijcel2;
    var rijcel3;
    var count = Number(document.getElementById("Rowcounter").value);
    var rows = document.getElementById(tableID).getElementsByTagName("tr").length;

    if (rows == 0) {
        waarde2 = document.getElementById("SmallBlind").value;
        waarde3 = document.getElementById("BigBlind").value;
        Duratie = document.getElementById("BlindDuratie").value;

        addRow(tableID, pauzeblind, waarde2, waarde3, Duratie, count)
    }
    else {
        rijcel2 = Number(document.getElementById("Smallblindwaarde" + count).value);
        rijcel3 = Number(document.getElementById("Bigblindwaarde" + count).value);

        if (pauzeblind == "Pauze") {
            waarde2 = rijcel2;
            waarde3 = rijcel3;
            Duratie = document.getElementById("PauzeDuratie").value;
        }
        else {
            if (document.getElementById("BlindFactor").value == 0) {
                blindfactor = 1;
            }
            else {
                blindfactor = document.getElementById("BlindFactor").value;
            }

            waarde2 = Math.ceil(((rijcel2 * blindfactor)+1)/10)*10;
            waarde3 = 2 * waarde2
            Duratie = document.getElementById("BlindDuratie").value;
        }

        addRow(tableID, pauzeblind, waarde2, waarde3, Duratie, count)
    }
}

function addRow(tableID, pauzeblind, SBwaarde, BBwaarde, Duratie, teller) {
    // Get a reference to the table
    let tableRef = document.getElementById(tableID);

    // Insert a row at the end of the table
    let newRow = tableRef.insertRow(-1);

    var newCell1 = newRow.insertCell(0);
    var t1 = document.createElement("input");
    t1.value = pauzeblind;
    newCell1.appendChild(t1);

    let newCell2 = newRow.insertCell(1)
    var t2 = document.createElement("input");
    t2.id = "Smallblindwaarde" + Number(teller + 1);
    t2.value = SBwaarde;
    newCell2.appendChild(t2);

    let newCell3 = newRow.insertCell(2)
    var t3 = document.createElement("input");
    t3.id = "Bigblindwaarde" + Number(teller + 1);
    t3.value = BBwaarde;
    newCell3.appendChild(t3);

    let newCell4 = newRow.insertCell(3)
    var t4 = document.createElement("input");
    t4.value = Duratie;
    newCell4.appendChild(t4);

    let newCell5 = newRow.insertCell(4)
    var t5 = document.createElement("input");
    t5.type = "button";
    t5.value = "Delete";
    t5.onclick = function () { SomeDeleteRowFunction(this); };
    newCell5.appendChild(t5);

    document.getElementById("Rowcounter").value = Number(teller + 1);
}

const HaalSpelerweg = function(o) {
    var p = o.parentNode.parentNode;
    p.parentNode.removeChild(p);
    min1speler();
}

//Spelers

function getpersondata(tableID) {
    addRowSpelers(tableID, document.getElementById("voornaam").value, document.getElementById("achternaam").value);
}

function addRowSpelers(tableID, voornaam, achternaam) {
    let tableRef = document.getElementById(tableID);

    let newRow = tableRef.insertRow(-1);

    let newCell2 = newRow.insertCell(0)
    newCell2.appendChild(document.createTextNode(voornaam));

    let newCell3 = newRow.insertCell(1)
    newCell3.appendChild(document.createTextNode(achternaam));

    let newCell4 = newRow.insertCell(2)
    var t4 = document.createElement("input");
    t4.type = "button";
    t4.value = "Delete";
    t4.className = "btn-danger";
    t4.onclick = function () { HaalSpelerweg(this); };
    newCell4.appendChild(t4);

    plus1speler();
}