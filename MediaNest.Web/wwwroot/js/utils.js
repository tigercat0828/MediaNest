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
};

window.UploadWarn = {
    enable: function () {
        window.addEventListener("beforeunload", uploadWarn._handler);
    },
    disable: function () {
        window.removeEventListener("beforeunload", uploadWarn._handler);
    },
    _handler: function (e) {
        e.preventDefault();
        e.returnValue = "�ɮפ��b�W�Ǥ��A�T�w�n���}�ܡH";
        return e.returnValue;
    }
};
