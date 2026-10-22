let companyTable;

$(function () {
    loadCompanyTable();
});

function loadCompanyTable() {
    companyTable = $('#tblData').DataTable({
        ajax: {
            url: '/Admin/Company/GetAll',
            dataSrc: 'data'
        },
        columns: [
            {
                data: 'name',
                width: '50%',
                className: 'fw-semibold text-primary',
                render: function (data) {
                    return `
                    <div class="d-flex align-items-center gap-2">
                        <div class="empty-state-icon-circle bg-primary-subtle text-primary p-1" style="width: 28px; height: 28px; font-size: 14px;">
                            <i class="bi bi-building"></i>
                        </div>
                        <span>${data}</span>
                    </div>`;
                }
            },
            {
                data: 'phone',
                width: '30%',
                className: 'font-mono text-secondary',
                render: function (data) {
                    return data ? `<i class="bi bi-telephone text-muted me-1"></i>${data}` : '<span class="text-muted">—</span>';
                }
            },
            {
                data: 'id',
                width: '20%',
                className: 'text-center',
                orderable: false,
                render: function (data) {
                    return `
                    <div class="d-inline-flex gap-1" role="group">
                        <a href="/Admin/Company/Upsert?id=${data}" class="btn btn-sm btn-outline-secondary qc-btn-secondary" title="Edit Company">
                            <i class="bi bi-pencil me-1"></i> Edit
                        </a>
                        <button type="button" onclick="deleteCompany('/Admin/Company/Delete/${data}')" class="btn btn-sm btn-outline-danger" title="Delete Company">
                            <i class="bi bi-trash3"></i>
                        </button>
                    </div>`;
                }
            }
        ],
        responsive: true,
        language: {
            emptyTable: 'No corporate accounts registered',
            search: '',
            searchPlaceholder: 'Search companies...',
            paginate: {
                previous: '<i class="bi bi-chevron-left"></i>',
                next: '<i class="bi bi-chevron-right"></i>'
            }
        }
    });
}

function deleteCompany(url) {
    window.qcDeleteRecord({
        url: url,
        table: companyTable,
        title: 'Delete this corporate account?',
        text: 'This will remove company billing configurations and associated user affiliations.',
        successMsg: 'Corporate account deleted successfully.'
    });
}
