function createArray() {
    return [];
}

function appendBytes(array, data) {
    array.push(new Uint8Array(data));
}

function finalizeBlob(array) {
    var blob = new Blob(array, { type: 'application/octet-stream' });
    return URL.createObjectURL(blob);
}
