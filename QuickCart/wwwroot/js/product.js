var dataTable;

$(document).ready(function () {
    loadDataTable();
});
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            url: '/Admin/Product/GetAll',
            dataSrc: 'data'
        },
        "columns": [
            {
                data: 'title',
                width: "20%",
                className: "text-nowrap"
            },
            {
                data: 'author',
                width: "20%",
                className: "text-nowrap"
            },
            {
                data: 'isbn',
                width: "10%",
                className: "text-nowrap"
            },
            {
                data: 'price',
                width: "10%",
                className: "text-nowrap"
            },
            {
                data: 'category.name',
                width: "15%",
                className: "text-nowrap"
            },
            {
                data: 'id',
                "render": function (data) {
                    return `
                    <div class="btn-group w-100 justify-content-center" role="group">
                        <a href="/Admin/Product/Upsert?id=${data}"
                           class="btn btn-dark mx-2"> <i class="bi bi-pencil-square"></i> Edit
                        </a>
                        <a onClick="Delete('/Admin/Product/Delete/${data}')"
                           class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i> Delete
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

function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    dataTable.ajax.reload();
                    toastr.success(data.message);
                },
                error: function () {
                    toastr.error("Something went wrong while deleting.");
                }
            });
        }
    });
}
