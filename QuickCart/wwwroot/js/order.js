let orderTable;

$(document).ready(function () {
    const urlParams = new URLSearchParams(window.location.search);
    const status = urlParams.get('status') || 'all';
    loadDataTable(status);
});

function getStatusBadgeHtml(status) {
    if (!status) return '<span class="qc-badge qc-badge-muted">—</span>';
    const s = status.toLowerCase();

    if (s.includes('approved')) {
        return `<span class="qc-badge qc-badge-success"><i class="bi bi-check2-circle me-1"></i>${status}</span>`;
    }
    if (s.includes('in process') || s.includes('inprocess')) {
        return `<span class="qc-badge qc-badge-info"><i class="bi bi-arrow-repeat me-1"></i>${status}</span>`;
    }
    if (s.includes('shipped') || s.includes('completed')) {
        return `<span class="qc-badge qc-badge-accent"><i class="bi bi-truck me-1"></i>${status}</span>`;
    }
    if (s.includes('pending') || s.includes('delayed')) {
        return `<span class="qc-badge qc-badge-warning"><i class="bi bi-hourglass-split me-1"></i>${status}</span>`;
    }
    if (s.includes('cancelled') || s.includes('refunded')) {
        return `<span class="qc-badge qc-badge-danger"><i class="bi bi-x-circle me-1"></i>${status}</span>`;
    }

    return `<span class="qc-badge qc-badge-muted">${status}</span>`;
}

function loadDataTable(status) {
    orderTable = $('#tblData').DataTable({
        ajax: {
            url: '/Admin/Order/GetAll?status=' + encodeURIComponent(status),
            dataSrc: 'data'
        },
        columns: [
            {
                data: 'orderHeaderId',
                width: '10%',
                className: 'font-mono fw-bold text-primary',
                render: function (data) {
                    return `#${data}`;
                }
            },
            {
                data: 'name',
                width: '18%',
                className: 'fw-semibold text-primary',
                render: function (data) {
                    return data || '<span class="text-muted">—</span>';
                }
            },
            {
                data: 'phoneNumber',
                width: '15%',
                className: 'font-mono small text-secondary',
                render: function (data) {
                    return data || '<span class="text-muted">—</span>';
                }
            },
            {
                data: 'applicationUser.email',
                width: '20%',
                className: 'small text-secondary',
                render: function (data) {
                    return data ? `<span class="text-truncate d-inline-block" style="max-width: 180px;" title="${data}">${data}</span>` : '<span class="text-muted">—</span>';
                }
            },
            {
                data: 'orderStatus',
                width: '15%',
                render: function (data) {
                    return getStatusBadgeHtml(data);
                }
            },
            {
                data: 'orderTotal',
                width: '12%',
                className: 'font-mono fw-bold text-primary',
                render: function (data) {
                    return '$' + parseFloat(data).toFixed(2);
                }
            },
            {
                data: 'orderHeaderId',
                width: '10%',
                className: 'text-center',
                orderable: false,
                render: function (data) {
                    return `
                    <a href="/Admin/Order/Details?orderId=${data}" class="btn btn-sm btn-outline-secondary qc-btn-secondary" title="Manage Order">
                        <i class="bi bi-eye me-1"></i> Manage
                    </a>`;
                }
            }
        ],
        responsive: true,
        language: {
            emptyTable: 'No orders found matching the filter criteria',
            search: '',
            searchPlaceholder: 'Search orders...',
            paginate: {
                previous: '<i class="bi bi-chevron-left"></i>',
                next: '<i class="bi bi-chevron-right"></i>'
            }
        }
    });
}
