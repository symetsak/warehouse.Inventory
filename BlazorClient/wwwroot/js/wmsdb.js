// Minimal IndexedDB wrapper for Blazor WASM
const DB_NAME = "wms-db";
const VERSION = 1;

let dbPromise;

/** Open (and upgrade) DB once */
function openDb() {
    if (!dbPromise) {
        dbPromise = new Promise((resolve, reject) => {
            const req = indexedDB.open(DB_NAME, VERSION);

            req.onupgradeneeded = () => {
                const db = req.result;

                // inventory_scans
                if (!db.objectStoreNames.contains("inventory_scans")) {
                    const s = db.createObjectStore("inventory_scans", { keyPath: "id" });
                    s.createIndex("code", "code");
                    s.createIndex("warehouseId", "warehouseId");
                    s.createIndex("timestamp", "timestamp");
                    s.createIndex("synced", "synced");
                }

                // pending_sync
                if (!db.objectStoreNames.contains("pending_sync")) {
                    const s = db.createObjectStore("pending_sync", { keyPath: "id" });
                    s.createIndex("type", "type");
                    s.createIndex("createdAt", "createdAt");
                }
            };

            req.onsuccess = () => resolve(req.result);
            req.onerror = () => reject(req.error);
        });
    }
    return dbPromise;
}

async function storeTx(store, mode) {
    const db = await openDb();
    return db.transaction(store, mode).objectStore(store);
}

export async function put(store, value) {
    const s = await storeTx(store, "readwrite");
    return new Promise((res, rej) => {
        const r = s.put(value);
        r.onsuccess = () => res(true);
        r.onerror = () => rej(r.error);
    });
}

export async function getAll(store) {
    const s = await storeTx(store, "readonly");
    return new Promise((res, rej) => {
        const r = s.getAll();
        r.onsuccess = () => res(r.result ?? []);
        r.onerror = () => rej(r.error);
    });
}

export async function get(store, key) {
    const s = await storeTx(store, "readonly");
    return new Promise((res, rej) => {
        const r = s.get(key);
        r.onsuccess = () => res(r.result ?? null);
        r.onerror = () => rej(r.error);
    });
}

export async function deleteKey(store, key) {
    const s = await storeTx(store, "readwrite");
    return new Promise((res, rej) => {
        const r = s.delete(key);
        r.onsuccess = () => res(true);
        r.onerror = () => rej(r.error);
    });
}
