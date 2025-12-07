window.wmsCharts = window.wmsCharts || {};

// ===== Bar chart: Top Products by Movements =====
window.wmsCharts.renderBarChart = function (canvasId, labels, data) {

    console.log("renderBarChart called", canvasId, labels, data);

    const ctx = document.getElementById(canvasId);
    if (!ctx) {
        console.error("renderBarChart: canvas not found:", canvasId);
        return;
    }

    try {
        if (ctx.chartInstance) {
            ctx.chartInstance.destroy();
        }

        ctx.chartInstance = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: '#0d6efd',
                    barThickness: 25
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: { display: false }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: { font: { size: 10 } }
                    },
                    x: {
                        ticks: { font: { size: 10 } }
                    }
                }
            }
        });
    } catch (err) {
        console.error("renderBarChart ERROR:", err);
    }
};

// ===== Doughnut chart: Warehouse distribution =====
window.wmsCharts.renderWarehousePie = function (canvasId, labels, data) {

    console.log("renderWarehousePie called", canvasId, labels, data);

    const ctx = document.getElementById(canvasId);
    if (!ctx) {
        console.error("renderWarehousePie: canvas not found:", canvasId);
        return;
    }

    try {
        if (ctx.chartInstance) {
            ctx.chartInstance.destroy();
        }

        const arr = Array.isArray(data) ? data : [];
        const total = arr.reduce((a, b) => a + b, 0);

        ctx.chartInstance = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: arr,
                    backgroundColor: [
                        '#0d6efd',
                        '#198754',
                        '#dc3545',
                        '#fd7e14',
                        '#20c997',
                        '#6f42c1',
                        '#ffc107',
                        '#0dcaf0'
                    ],
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'bottom' },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                const label = context.label || '';
                                const value = context.parsed || 0;
                                const pct = total ? ((value / total) * 100).toFixed(1) : 0;
                                return `${label}: ${value} (${pct}%)`;
                            }
                        }
                    }
                }
            }
        });
    } catch (err) {
        console.error("renderWarehousePie ERROR:", err);
    }
};
