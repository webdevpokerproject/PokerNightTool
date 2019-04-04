function timer() {


    var eindtijd2 = new Date().getTime() + (eindtijd / 10000);
    if (ronde == 2000) {
        document.getElementById("ronde").innerHTML = "Het evenement is nog niet begonnen of al afgelopen";
        document.getElementById("smallblind").innerHTML = "";
        document.getElementById("bigblind").innerHTML = "";
        document.getElementById("ronde2").innerHTML = "";
        document.getElementById("smallblind2").innerHTML = "";
        document.getElementById("bigblind2").innerHTML = "";
    } else {
        if (pauzeofblind == 0) {
            document.getElementById("ronde").innerHTML = "Ronde: " + ronde + " blind";
            document.getElementById("smallblind").innerHTML = "Smallblind: " + sb;
            document.getElementById("bigblind").innerHTML = "Bigblind: " + bb;


            if (pauzeofblindnext == 0) {
                document.getElementById("ronde2").innerHTML = "Ronde: " + rondenext + " blind";
                document.getElementById("smallblind2").innerHTML = "Smallblind: " + sbnext;
                document.getElementById("bigblind2").innerHTML = "Bigblind: " + bbnext;
            } else {
                document.getElementById("ronde2").innerHTML = "Ronde: " + rondenext + " Pauze";
                document.getElementById("smallblind2").innerHTML = "Pauzeertijd: " + duratiepauze;
                document.getElementById("bigblind2").innerHTML = "";
            }

        }
        else {
            document.getElementById("ronde").innerHTML = "Ronde: " + ronde + " pauze";
            document.getElementById("smallblind").innerHTML = "";
            document.getElementById("bigblind").innerHTML = "";

            if (pauzeofblindnext == 0) {
                document.getElementById("ronde2").innerHTML = "Ronde: " + rondenext + " blind";
                document.getElementById("smallblind2").innerHTML = "Smallblind: " + sbnext;
                document.getElementById("bigblind2").innerHTML = "Bigblind: " + bbnext;
            } else {
                document.getElementById("ronde2").innerHTML = "Ronde: " + rondenext + " pauze";
                document.getElementById("smallblind2").innerHTML = "Pauzeertijd: " + duratiepauze + " minuten";
                document.getElementById("bigblind2").innerHTML = "";
            }
        }

        var x = setInterval(function () {

            // Get todays date and time
            var now = new Date().getTime();

            // Find the distance between now and the count down date
            var distance = eindtijd2 - now;


            // Time calculations for days, hours, minutes and seconds
            var minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
            var seconds = Math.floor((distance % (1000 * 60)) / 1000);

            // Output the result in an element with id="demo"
            document.getElementById("timer").innerHTML = minutes + ":" + seconds;

            // If the count down is over, write some text
            if (distance <= 0) {
                clearInterval(x);

                location.reload();
            }
        }, 1000);
    }
}
timer();