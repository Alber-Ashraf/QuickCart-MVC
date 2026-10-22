let userDataTable;

$(document).ready(function () {
    loadUserDataTable();
});

function getRoleBadgeHtml(role) {
    if (!role) return '<span class="qc-badge qc-badge-muted">—</span>';
    const r = role.toLowerCase();

    if (r.includes('admin')) {
        return `<span class="qc-badge qc-badge-danger"><i class="bi bi-shield-shaded me-1"></i>${role}</span>`;
    }
    if (r.includes('employee')) {
        return `<span class="qc-badge qc-badge-accent"><i class="bi bi-person-badge me-1"></i>${role}</span>`;
    }
    if (r.includes('company')) {
        return `<span class="qc-badge qc-badge-warning"><i class="bi bi-building me-1"></i>${role}</span>`;
    }

    return `<span class="qc-badge qc-badge-info"><i class="bi bi-person me-1"></i>${role}</span>`;
}

function loadUserDataTable() {
    userDataTable = $('#tblData').DataTable({
        ajax: {
            url: '/Admin/User/GetAll',
            dataSrc: 'data'
        },
        columns: [
            {
                data: 'name',
                width: '18%',
                className: 'fw-semibold text-primary',
                render: function (data) {
                    return `
                    <div class="d-flex align-items-center gap-2">
                        <div class="avatar-initials-chip" style="width: 28px; height: 28px; font-size: 11px;">
                            ${data ? data.substring(0, 2).toUpperCase() : 'U'}
                        </div>
                        <span class="text-truncate" style="max-width: 140px;" title="${data}">${data}</span>
                    </div>`;
                }
            },
            {
                data: 'email',
                width: '22%',
                className: 'text-secondary small',
                render: function (data) {
                    return `<span class="text-truncate d-inline-block" style="max-width: 180px;" title="${data}">${data}</span>`;
                }
            },
            {
                data: 'phoneNumber',
                width: '14%',
                className: 'font-mono small text-secondary',
                render: function (data) {
                    return data || '<span class="text-muted">—</span>';
                }
            },
            {
                data: 'company.name',
                width: '14%',
                render: function (data) {
                    return data
                        ? `<span class="qc-badge qc-badge-muted"><i class="bi bi-building me-1"></i>${data}</span>`
                        : '<span class="text-muted small">—</span>';
                }
            },
            {
                data: 'role',
                width: '14%',
                render: function (data) {
                    return getRoleBadgeHtml(data);
                }
            },
            {
                data: { id: "id", lockoutEnd: "lockoutEnd" },
                width: '18%',
                className: 'text-center',
                orderable: false,
                render: function (data) {
                    const today = new Date().getTime();
                    const lockout = data.lockoutEnd ? new Date(data.lockoutEnd).getTime() : 0;
                    const isLocked = lockout > today;

                    return `
                    <div class="d-inline-flex gap-1" role="group">
                        <button type="button"
                                onclick="LockUnlock('${data.id}')"
                                class="btn btn-sm ${isLocked ? 'btn-outline-success' : 'btn-outline-danger'}"
                                title="${isLocked ? 'Unlock User' : 'Lock Account'}">
                            <i class="bi ${isLocked ? 'bi-unlock-fill me-1' : 'bi-lock-fill me-1'}"></i>
                            ${isLocked ? 'Unlock' : 'Lock'}
                        </button>
                        <a href="/Admin/User/ManagePermission/${data.id}"
                           class="btn btn-sm btn-outline-secondary qc-btn-secondary"
                           title="Edit Permissions & Role">
                            <i class="bi bi-shield-lock me-1"></i> Role
                        </a>
                    </div>`;
                }
            }
        ],
        responsive: true,
        language: {
            emptyTable: 'No user accounts found in directory',
            search: '',
            searchPlaceholder: 'Search directory...',
            paginate: {
                previous: '<i class="bi bi-chevron-left"></i>',
                next: '<i class="bi bi-chevron-right"></i>'
            }
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
            if (data && data.success) {
                toastr.success(data.message || 'Account status updated.');
                userDataTable.ajax.reload();
            } else {
                toastr.error((data && data.message) || 'Error updating account status.');
            }
        },
        error: function () {
            toastr.error('Error communicating with authentication server.');
        }
    });
}
