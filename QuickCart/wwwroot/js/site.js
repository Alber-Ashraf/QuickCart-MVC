window.addEventListener("load", function () {
    if (!localStorage.getItem("splashSeen")) {
        setTimeout(function () {
            document.body.classList.add("loaded");
            localStorage.setItem("splashSeen", "true");
        }, 1000);
    } else {
        setTimeout(function () {
            document.body.classList.add("loaded");
        }, 1000);
    }
});