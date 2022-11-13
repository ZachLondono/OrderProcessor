function toggleDropdown(dropdown) {
    if (dropdown.getAttribute("hidden") == null) {
        dropdown.setAttribute("hidden", "");
        return true;
    } else {
        dropdown.removeAttribute("hidden", "");
        return false;
    }
}

window.onclick = function (event) {

    var dropdownbtns = document.getElementsByClassName("dropdown-btn")
    for (i = 0; i < dropdownbtns.length; i++) {
        if (dropdownbtns[i].contains(event.target)) return;
    }

    var dropdowns = document.getElementsByClassName("dropdown");
    for (i = 0; i < dropdowns.length; i++) {
        var dropdown = dropdowns[i];
        
        // If click is within a dropdown, don't close it
        if (dropdown.contains(event.target)) continue;

        if (dropdown.getAttribute("hidden") != null) return;
        else dropdown.setAttribute("hidden", "");
    }

}