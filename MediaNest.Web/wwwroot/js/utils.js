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

