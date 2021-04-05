window.changeUrl = function (url) {
    history.pushState(null, '', url);
}
window.showDialog = function (id) {
    $('#' + id).modal();
}