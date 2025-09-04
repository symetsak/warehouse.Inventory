// wwwroot/js/wms-export.js
// Απαιτεί να έχει φορτωθεί πρώτα το SheetJS (xlsx.full.min.js)
(function () {
    if (!window.XLSX) {
        console.error("SheetJS (XLSX) is not loaded.");
        return;
    }

    // headers: string[], data: object[][]
    window.exportXlsx = function (filename, headers, data) {
        const aoa = [headers, ...data];                 // Array-of-Arrays: 1η γραμμή = headers
        const ws = XLSX.utils.aoa_to_sheet(aoa);

        // λίγο πιο άνετα πλάτη στηλών
        ws['!cols'] = headers.map(() => ({ wch: 18 }));

        // AutoFilter στην πρώτη γραμμή
        const endRef = XLSX.utils.encode_cell({ r: data.length, c: headers.length - 1 });
        ws['!autofilter'] = { ref: `A1:${endRef}` };

        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, "Inventory");

        const wbout = XLSX.write(wb, { bookType: 'xlsx', type: 'array' });
        const blob = new Blob([wbout], {
            type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        });

        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        a.click();
        URL.revokeObjectURL(url);
    };
})();
