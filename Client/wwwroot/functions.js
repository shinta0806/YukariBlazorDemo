// --------------------------------------------------------------------
// ドロップダウンを開閉する
// --------------------------------------------------------------------
function OnClickDropdown(dropdownButton) {
    dropdownButton.classList.toggle('is-open');
}

// --------------------------------------------------------------------
// ログインドロップダウンを閉じる
// --------------------------------------------------------------------
function CloseLoginDropdown() {
    var loginDropdownButton = document.getElementById('login-button');
    if (loginDropdownButton) {
        loginDropdownButton.classList.remove('is-open');
    }
}

// --------------------------------------------------------------------
// 指定した要素までスクロール
// 2021/01 現在、a href="#xxx" がうまく動かないため、JS で代替
// --------------------------------------------------------------------
function ScrollToElement(elementName) {
    var element = document.getElementById(elementName);
    if (element) {
        scrollTo(0, element.getBoundingClientRect().top);
    }
}

