$(document).ready(function () {
    loadDataTable();
});
function loadDataTable() {
    $('#tblData').DataTable({
        "ajax": {
            url: '/Admin/Product/GetAll',
            dataSrc: 'data' // Adjust this based on API response structure
        },
        "columns": [
            { data: 'title', "width": "20%" },
            { data: 'author', "width": "20%" },
            { data: 'isbn', "width": "10%" },
            { data: 'price', "width": "10%" },
            { data: 'category.name', "width": "15%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                     <a href="/admin/product/upsert?id=${data}" class="btn btn-dark mx-2"> <i class="bi bi-pencil-square"></i> Edit</a>               
                     <a class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i> Delete</a>
                    </div>`
                },
                "width": "25%"
            }
        ]
    });
}
