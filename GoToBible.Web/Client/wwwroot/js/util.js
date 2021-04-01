window.changeUrl = function (url) {
    history.pushState(null, '', url);
}