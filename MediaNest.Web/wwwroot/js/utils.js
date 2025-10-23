window.Utils = {
    ShowAlert: function (message) {
        alert(message);
    },
    GetImageWidth: function (imgId) {
        var img = document.getElementById(imgId);
        if (img) {
            return img.width;
        } else {
            return null;
        }
    },
    CopyTextToClipBoard: function (text) {
        navigator.clipboard.writeText(text);
        alert('Text copied to clipboard');
    },
    confirmNavigate: function () {
        return confirm("檔案仍在上傳中，確定要離開此頁面嗎？");
    }
};

window.UploadWarn = {
    enable: function () {
        window.addEventListener("beforeunload", UploadWarn._handler);
    },
    disable: function () {
        window.removeEventListener("beforeunload", UploadWarn._handler);
    },
    _handler: function (e) {
        e.preventDefault();
        e.returnValue = "檔案仍在上傳中，確定要離開此頁面嗎？";
        return e.returnValue;
    }
};
