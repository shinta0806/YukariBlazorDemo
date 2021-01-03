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
    loginDropdownButton = document.getElementById('login-button');
    if (loginDropdownButton) {
        loginDropdownButton.classList.remove('is-open');
    }
}