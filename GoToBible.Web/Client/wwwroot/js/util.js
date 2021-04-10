window.changeUrl = function (url) {
    history.pushState(null, '', url);
}
window.closeMenu = function (id) {
    $('#' + id).collapse('hide');
}
window.showDialog = function (id) {
    $('#' + id).modal();
}