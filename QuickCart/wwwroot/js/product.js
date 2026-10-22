let productTable;

$(function () {
    loadProductTable();
});

function loadProductTable() {
    productTable = $('#tblData').DataTable({
        ajax: {
            url: '/Admin/Product/GetAll',
            dataSrc: 'data'
        },
        columns: [
            {
                data: 'title',
                width: '25%',
                className: 'fw-semibold text-primary',
                render: function (data) {
                    return `<span class="text-truncate d-inline-block" style="max-width: 240px;" title="${data}">${data}</span>`;
                }
            },
            {
                data: 'author',
                width: '18%',
                className: 'text-secondary',
                render: function (data) {
                    return data || '<span class="text-muted">—</span>';
                }
            },
            {
                data: 'isbn',
                width: '14%',
                className: 'font-mono small',
                render: function (data) {
                    return data ? `<span class="qc-badge qc-badge-muted">${data}</span>` : '<span class="text-muted">—</span>';
                }
            },
            {
                data: 'price',
                width: '12%',
                className: 'font-mono fw-bold text-primary',
                render: function (data) {
                    return '$' + parseFloat(data).toFixed(2);
                }
            },
            {
                data: 'category.name',
                width: '15%',
                render: function (data) {
                    return data
                        ? `<span class="qc-badge qc-badge-accent"><i class="bi bi-tag-fill me-1"></i>${data}</span>`
                        : '<span class="qc-badge qc-badge-muted">Uncategorized</span>';
                }
            },
            {
                data: 'id',
                width: '16%',
                className: 'text-center',
                orderable: false,
                render: function (data) {
                    return `
                    <div class="d-inline-flex gap-1" role="group">
                        <a href="/Admin/Product/Upsert?id=${data}" class="btn btn-sm btn-outline-secondary qc-btn-secondary" title="Edit Product">
                            <i class="bi bi-pencil me-1"></i> Edit
                        </a>
                        <button type="button" onclick="deleteProduct('/Admin/Product/Delete/${data}')" class="btn btn-sm btn-outline-danger" title="Delete Product">
                            <i class="bi bi-trash3"></i>
                        </button>
                    </div>`;
                }
            }
        ],
        responsive: true,
        language: {
            emptyTable: 'No products available in the catalog',
            search: '',
            searchPlaceholder: 'Search products...',
            paginate: {
                previous: '<i class="bi bi-chevron-left"></i>',
                next: '<i class="bi bi-chevron-right"></i>'
            }
        }
    });
}

function deleteProduct(url) {
    window.qcDeleteRecord({
        url: url,
        table: productTable,
        title: 'Delete this product?',
        text: 'This action permanently removes the product and associated image assets.',
        successMsg: 'Product deleted successfully.'
    });
}
