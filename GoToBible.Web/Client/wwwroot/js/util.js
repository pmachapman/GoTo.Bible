window.changeUrl = function (url) {
    history.pushState(null, '', url);
}
window.closeMenu = function (id) {
    // ReSharper disable once UseOfImplicitGlobalInFunctionScope
    $('#' + id).collapse('hide');
}
window.scrollToElement = function (id) {
    var element = document.getElementById(id);
    if (element != null) {
        try {
            element.scrollIntoView({ behavior: 'smooth' });
        } catch (e) {
            // Safari support
            element.scrollIntoView(true);
        }
    }
}
window.setContent = function (id, content) {
    // ReSharper disable once UseOfImplicitGlobalInFunctionScope
    $('#' + id).html(content);
}
window.showDialog = function (id) {
    // ReSharper disable once UseOfImplicitGlobalInFunctionScope
    $('#' + id).modal();
}