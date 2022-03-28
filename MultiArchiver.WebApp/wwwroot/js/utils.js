(function () {
    var newline = "\n";

    new Blob([newline], { endings: 'native' }).text().then(value => {
        newline = value;
    });

    getNewline = function() {
        return newline;
    }
})();

function createArray() {
    return [];
}

function appendBytes(array, data) {
    array.push(new Uint8Array(data));
}

function finalizeBlob(array) {
    var blob = new Blob(array, { type: 'text/plain', endings: 'native' });
    return URL.createObjectURL(blob);
}
