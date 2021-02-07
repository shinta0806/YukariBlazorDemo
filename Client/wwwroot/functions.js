// ====================================================================
// 変数
// ====================================================================

// Server-Sent Events 受信用
var eventSource = null;

// ====================================================================
// 関数
// ====================================================================

// --------------------------------------------------------------------
// すべての select タグの選択状態を先頭項目にする
// --------------------------------------------------------------------
function ClearSelectSelections() {
    var elements = document.getElementsByTagName("select");
    for (var i = 0; i < elements.length; i++) {
        elements[i].selectedIndex = 0;
    }
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
// Server-Sent Events 受信用の EventSource を作成
// --------------------------------------------------------------------
function CreateEventSource(helper) {
    if (eventSource != null) {
        eventSource.close();
    }
    eventSource = new EventSource("/sse");
    eventSource.onopen = function () {
        helper.invokeMethodAsync('OnSse', 'Open');
    };
    eventSource.onerror = function () {
        helper.invokeMethodAsync('OnSse', 'Error');
        eventSource = null;
    };
    eventSource.onmessage = function (event) {
        helper.invokeMethodAsync('OnSse', event.data);
    }
}

// --------------------------------------------------------------------
// ドロップダウンを開閉する
// --------------------------------------------------------------------
function OnClickDropdown(dropdownButton) {
    dropdownButton.classList.toggle('is-open');
}

// --------------------------------------------------------------------
// 指定した要素までスクロール
// 2021/01 現在、a href="#xxx" がうまく動かないため、JS で代替
// --------------------------------------------------------------------
function ScrollToElement(elementId) {
    var element = document.getElementById(elementId);
    if (element) {
        scrollTo(0, element.getBoundingClientRect().top);
    }
}


