window.wmsCharts = {
    renderBarChart: function (canvasId, labels, data) {

        const ctx = document.getElementById(canvasId);

        if (!ctx) return;

        // Destroy existing chart (fix refresh issues)
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
                    arThickness: 10 
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
    }
};
