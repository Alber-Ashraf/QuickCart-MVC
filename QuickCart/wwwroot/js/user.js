var dataTable;

$(document).ready(function () {
    loadDataTable();
});
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            url: '/Admin/User/GetAll',
            dataSrc: 'data'
        },
        "columns": [
            {
                data: 'name',
                width: "20%",
                className: "text-nowrap"
            },
            {
                data: 'email',
                width: "20%",
                className: "text-nowrap"
            },
            {
                data: 'phoneNumber',
                width: "10%",
                className: "text-nowrap"
            },
            {
                data: 'company.name',
                width: "10%",
                className: "text-nowrap"
            },
            {
                data: 'role',
                width: "15%",
                className: "text-nowrap"
            },
            {
                data: 'id',
                "render": function (data) {
                    return `
                    <div class="btn-group w-100 justify-content-center" role="group">
                        <a href="/Admin/User/Edit?id=${data}"
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
            emptyTable: "No users available",
            search: "_INPUT_",
            searchPlaceholder: "Search users..."
        }
    });
}