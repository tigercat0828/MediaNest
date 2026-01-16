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
        return confirm("檔案正在上傳中，確定要離開嗎？");
    }
};

