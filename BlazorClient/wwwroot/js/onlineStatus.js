export function isOnline() { return navigator.onLine; }
export function subscribeOnline(dotnetRef) {
    function notify() { dotnetRef.invokeMethodAsync('OnStatusChanged', navigator.onLine); }
    window.addEventListener('online', notify);
    window.addEventListener('offline', notify);
    notify();
    return true;
}
