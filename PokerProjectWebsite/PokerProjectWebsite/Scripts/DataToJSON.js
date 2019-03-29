const _CollectSettings = function () {
    "use strict";

    let dict = {};
    let elements = document.querySelectorAll('.stored');
    for (let e of elements) {
        dict[e.id] = e.value
    }

    console.log(dict);
    console.log(JSON.stringify(dict));
}

const _CollectBlindData = function () {
    "use strict";

    let blindTable = document.getElementById('blindtable');
    
    let l = []; // list
    let i = 0;  // iterator

    //Iterate through rows
    for (let e of blindTable.children) {
        
        let j = 0; //iterator
        l[i] = {};
        l[i].Ronde = i + 1;

        // Iterate through columns of row
        for (let z of e.children) {
            switch (j) {
                case 0:
                    if (z.firstChild.value == "Pauze") {
                        l[i]["Pauze"] = 'True'
                    }
                    else {l[i]["Pauze"] = 'False'}
                    
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

const _Collect = function () {
    "use strict";

    let collected = {}
    collected.Blinds = _CollectBlindData();
    collected.Spelers = [
           {'Voornaam':'Maurice','Achternaam':'Ponte'},
           {'Voornaam':'Geert','Achternaam':'Hagema'}, 
           {'Voornaam':'Tjeerd','Achternaam': 'van Gelder'}, 
           {'Voornaam':'Jan-Sietze','Achternaam': 'Hoekstra'}]
    collected.Fiche = [
           {'Kleur':'blue','Waarde':'10'},    
           {'Kleur':'white','Waarde':'20'},    
           {'Kleur':'green','Waarde':'100'},    
           {'Kleur':'red','Waarde':'200'},    
           {'Kleur':'black','Waarde':'500'}]
    return collected;
}

const _CollectData = function () {
    "use strict";
    console.log("Click!");
    let newestData = _Collect();
    let json = JSON.stringify(newestData)
    return json
}

$('#dataForm').submit(function() {
  document.getElementById('dataInput').value = _CollectData();
  return true;
});


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

            waarde2 = (rijcel2 * blindfactor);
            waarde3 = (rijcel3 * blindfactor);
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

    document.getElementById("Rowcounter").value = Number(teller + 1)
}

function SomeDeleteRowFunction(o) {
    //no clue what to put here?
    var p = o.parentNode.parentNode;
    p.parentNode.removeChild(p);
}