window.barcodeCamera = (function () {
    let html5QrCode = null;
    let isRunning = false;
    let dotnetRef = null;
    let currentHost = null;

    async function start(element, dotnet, fps = 15) {
        // Προληπτικό stop αν έμεινε κάτι ανοιχτό
        try { await stop(); } catch { }

        dotnetRef = dotnet;
        currentHost = (element instanceof HTMLElement) ? element : document.querySelector(element);
        if (!currentHost) throw new Error("Scanner host not found");
        if (!currentHost.id) currentHost.id = "scan_" + Math.random().toString(36).slice(2);

        if (!window.Html5Qrcode || !window.Html5QrcodeSupportedFormats) {
            throw new Error("html5-qrcode v2+ not loaded");
        }

        // Καθάρισε τυχόν παλιά παιδιά/canvas
        try { currentHost.innerHTML = ""; } catch { }

        html5QrCode = new Html5Qrcode(currentHost.id, { verbose: false });

        // 1D formats που συνήθως θες σε αποθήκη
        const F = Html5QrcodeSupportedFormats;
        const supportedFormats = [
            F.EAN_13, F.EAN_8, F.UPC_A, F.UPC_E,
            F.CODE_128, F.CODE_39, F.ITF,
            // F.QR_CODE, // άνοιξέ το αν θες και QR παράλληλα
        ];

        // Μεγαλύτερο qrbox για 1D (στενό ύψος)
        const width = Math.floor(currentHost.clientWidth * 0.9);
        const config = {
            fps,
            qrbox: { width, height: Math.floor(width * 0.35) },
            aspectRatio: 1.7778,
            rememberLastUsedCamera: true,
            supportedFormats,
            experimentalFeatures: { useBarCodeDetectorIfSupported: true }
        };

        const onSuccess = (text) => {
            if (!text) return;
            // throttle διπλών αναγνώσεων
            if (start._last === text && (Date.now() - (start._lastAt || 0)) < 700) return;
            start._last = text; start._lastAt = Date.now();
            try { dotnetRef.invokeMethodAsync('JsBarcodeScanned', text); } catch (e) { console.warn(e); }
        };

        const onError = (_e) => { /* σιωπή ανά frame */ };

        // Διάλεξε "πίσω" κάμερα αν υπάρχει
        let cams = [];
        try { cams = await Html5Qrcode.getCameras(); } catch (e) { console.error(e); }
        if (!cams || !cams.length) throw new Error("No camera found");

        const back = cams.find(c => /back|rear|environment/i.test(c.label)) || cams[0];

        await html5QrCode.start({ deviceId: { exact: back.id } }, config, onSuccess, onError);
        isRunning = true;
    }

    async function stop() {
        if (html5QrCode) {
            try { await html5QrCode.stop(); } catch { }
            try { await html5QrCode.clear(); } catch { }
        }
        html5QrCode = null;
        isRunning = false;

        // Καθάρισε DOM για να μη μείνει overlay/canvas
        if (currentHost) {
            try { currentHost.innerHTML = ""; } catch { }
            currentHost = null;
        }
    }

    // Προαιρετικό: άναμμα φλας όπου υποστηρίζεται
    async function toggleTorch(on) {
        if (!html5QrCode) return false;
        try {
            const tracks = html5QrCode.getState() === 2 // RUNNING
                ? html5QrCode._qrRegion?.videoElement?.srcObject?.getVideoTracks?.() || []
                : [];
            const track = tracks[0];
            if (!track) return false;
            const caps = track.getCapabilities?.();
            if (!caps || !caps.torch) return false;
            await track.applyConstraints({ advanced: [{ torch: !!on }] });
            return true;
        } catch (e) {
            console.warn("Torch not supported", e);
            return false;
        }
    }

    return { start, stop, toggleTorch };
})();

window.barcodeUX = {
    feedback: () => {
        try { if (navigator.vibrate) navigator.vibrate(40); } catch { }
        try { new Audio('/sounds/beep.mp3').play().catch(() => { }); } catch { }
    }
};
