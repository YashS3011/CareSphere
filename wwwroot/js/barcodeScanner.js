// Barcode Scanner using ZXing-js library
// CDN: https://unpkg.com/@zxing/library@latest/umd/index.min.js

let codeReader = null;
let activeStream = null;

window.startBarcodeScanner = function (videoElementId, dotNetRef) {
    return new Promise(function (resolve, reject) {
        if (typeof ZXing === 'undefined') {
            // Dynamically load ZXing-js if not already loaded
            var script = document.createElement('script');
            script.src = 'https://unpkg.com/@zxing/library@latest/umd/index.min.js';
            script.onload = function () {
                initScanner(videoElementId, dotNetRef, resolve, reject);
            };
            script.onerror = function () {
                reject('Failed to load ZXing library.');
            };
            document.head.appendChild(script);
        } else {
            initScanner(videoElementId, dotNetRef, resolve, reject);
        }
    });
};

function initScanner(videoElementId, dotNetRef, resolve, reject) {
    try {
        codeReader = new ZXing.BrowserMultiFormatReader();
        var videoElement = document.getElementById(videoElementId);

        if (!videoElement) {
            reject('Video element not found: ' + videoElementId);
            return;
        }

        codeReader.decodeFromVideoDevice(null, videoElement, function (result, err) {
            if (result) {
                var text = result.getText();
                if (text && text.length > 0) {
                    dotNetRef.invokeMethodAsync('OnBarcodeScanned', text);
                }
            }
            // Errors during scanning are normal (e.g. no barcode detected in frame)
        }).then(function (controls) {
            activeStream = controls;
            resolve(true);
        }).catch(function (err) {
            console.error('Barcode scanner error:', err);
            reject('Failed to start barcode scanner: ' + err.message);
        });
    } catch (ex) {
        console.error('Barcode scanner init error:', ex);
        reject('Failed to initialize barcode scanner: ' + ex.message);
    }
}

window.stopBarcodeScanner = function () {
    try {
        if (codeReader) {
            codeReader.reset();
            codeReader = null;
        }
        if (activeStream && activeStream.stop) {
            activeStream.stop();
            activeStream = null;
        }
    } catch (ex) {
        console.warn('Error stopping barcode scanner:', ex);
    }
};
