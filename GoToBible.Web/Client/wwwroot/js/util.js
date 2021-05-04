window.changeUrl = function (url) {
    history.pushState(null, '', url);
}
window.closeMenu = function (id) {
    $('#' + id).collapse('hide');
}
window.scrollToElement = function (id) {
    var element = document.getElementById(id);
    if (element != null) {
        try {
            element.scrollIntoView({ behavior: 'smooth' });
        } catch {
            // Safari support
            elmnt.scrollIntoView(true);
        }
    }
}
window.setContent = function (id, content) {
    $('#' + id).html(content);
}
window.showDialog = function (id) {
    $('#' + id).modal();
}