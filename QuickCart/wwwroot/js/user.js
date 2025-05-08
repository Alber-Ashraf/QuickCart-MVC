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
            { data: 'name', width: "20%", className: "text-nowrap" },
            { data: 'email', width: "20%", className: "text-nowrap" },
            { data: 'phoneNumber', width: "10%", className: "text-nowrap" },
            { data: 'company.name', width: "10%", className: "text-nowrap" },
            { data: 'role', width: "15%", className: "text-nowrap" },
            {
                data: { id: "id", lockoutEnd: "lockoutEnd" },
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();

                    if (lockout > today) {
                        lockText = 'Unlock';
                        lockIcon = 'bi-unlock-fill';
                        lockBtnClass = 'btn-success';

                        return `
                        <div class="d-flex justify-content-center gap-2">
                            <button onclick="LockUnlock('${data.id}')" class="btn ${lockBtnClass} width:100px; btn-sm d-flex align-items-center gap-1 px-3">
                                <i class="bi ${lockIcon}"></i> ${lockText}
                            </button>
                            <a href="/Admin/User/ManagePermission/${data.id}" class="btn btn-dark btn-sm d-flex align-items-center gap-1 px-3">
                                <i class="bi bi-pencil-square"></i> Permission
                            </a>
                        </div>
                    `;
                    }
                    else {
                        lockText = 'Lock';
                        lockIcon = 'bi-lock-fill';
                        lockBtnClass = 'btn-danger';
                        return `
                        <div class="d-flex justify-content-center gap-2">
                            <button onclick="LockUnlock('${data.id}')" class="btn ${lockBtnClass} width:100px; btn-sm d-flex align-items-center gap-1 px-3">
                                <i class="bi ${lockIcon}"></i> ${lockText}
                            </button>   
                            <a href="/Admin/User/ManagePermission/${data.id}" class="btn btn-dark btn-sm d-flex align-items-center gap-1 px-3">
                                <i class="bi bi-pencil-square"></i> Permission
                            </a>
                        </div>
                    `;
                     }
                    
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

function LockUnlock(id) {
    $.ajax({
        type: "POST",
        url: '/Admin/User/LockUnlock',
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            } else {
                toastr.error("An error occurred.");
            }
        }
    });
}
