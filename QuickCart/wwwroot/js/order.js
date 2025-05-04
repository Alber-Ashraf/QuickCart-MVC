var dataTable;

$(document).ready(function () {
    loadDataTable();
});
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            url: '/Admin/Order/GetAll',
            dataSrc: 'data'
        },
        "columns": [
            {
                data: 'orderHeaderId',
                width: "5%",
                className: "text-nowrap"
            },
            {
                data: 'name',
                width: "15%",
                className: "text-nowrap"
            },
            {
                data: 'phoneNumber',
                width: "15%",
                className: "text-nowrap"
            },
            {
                data: 'applicationUser.email',
                width: "20%",
                className: "text-nowrap"
            },
            {
                data: 'orderStatus',
                width: "10%",
                className: "text-nowrap"
            },
            {
                data: 'orderTotal',
                width: "10%",
                className: "text-nowrap"
            },
            {
                data: 'id',
                "render": function (data) {
                    return `
                    <div class="btn-group w-100 justify-content-center" role="group">
                        <a href="/Admin/Peoduct/Upsert?id=${data}"
                           class="btn btn-dark mx-2"> <i class="bi bi-pencil-square"></i> Edit
                        </a>
                    </div>`;
                },
                "width": "25%",
                className: "text-nowrap"
            }
        ],
        responsive: true,
        language: {
            emptyTable: "No products available",
            search: "_INPUT_",
            searchPlaceholder: "Search products..."
        }
    });
}

