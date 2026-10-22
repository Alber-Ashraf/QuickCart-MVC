/**
 * Flyleaf Enterprise Commerce & Publishing Core JavaScript
 * Handles: Theme switching (zero-FOUC), Collapsible Sidebar, Keyboard Shortcuts,
 * Toastr notifications, Form submission locks, and AJAX Anti-CSRF injection.
 */

(function () {
    'use strict';

    // Apply theme immediately before DOM paint to prevent FOUC
    const storedTheme = localStorage.getItem('qc-theme') || (window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
    document.documentElement.setAttribute('data-bs-theme', storedTheme);

    // Apply sidebar collapse state immediately
    const isSidebarCollapsed = localStorage.getItem('qc-sidebar-collapsed') === 'true';
    if (isSidebarCollapsed && window.innerWidth >= 992) {
        document.addEventListener('DOMContentLoaded', function () {
            const shell = document.getElementById('appShell');
            if (shell) shell.classList.add('sidebar-collapsed');
        });
    }
})();

$(function () {
    'use strict';

    // =========================================================================
    // 1. Theme Management (Light / Dark Mode Switcher)
    // =========================================================================
    $('#themeToggleBtn').on('click', function () {
        const currentTheme = document.documentElement.getAttribute('data-bs-theme') || 'light';
        const nextTheme = currentTheme === 'dark' ? 'light' : 'dark';

        document.documentElement.setAttribute('data-bs-theme', nextTheme);
        localStorage.setItem('qc-theme', nextTheme);

        // Notify DataTables if active to adapt colors if needed
        $(document).trigger('qc:themeChanged', [nextTheme]);
    });

    // =========================================================================
    // 2. Collapsible Sidebar & Mobile Navigation
    // =========================================================================
    const $shell = $('#appShell');
    const $mobileBtn = $('#mobileMenuBtn');

    function updateSidebarToggleUI(isCollapsed) {
        const titleText = isCollapsed ? 'Expand Sidebar (Ctrl+B)' : 'Collapse Sidebar (Ctrl+B)';
        const ariaValue = isCollapsed ? 'false' : 'true';

        // Header toggle button
        $('#sidebarCollapseBtn')
            .attr('title', titleText)
            .attr('aria-expanded', ariaValue)
            .find('i')
            .attr('class', isCollapsed ? 'bi bi-chevron-bar-right' : 'bi bi-chevron-bar-left');

        // Rail footer toggle button
        $('#sidebarRailToggleBtn')
            .attr('title', titleText)
            .attr('aria-expanded', ariaValue);
        $('#sidebarRailToggleBtn .rail-collapse-icon')
            .attr('class', isCollapsed ? 'bi bi-chevron-bar-right rail-collapse-icon' : 'bi bi-chevron-bar-left rail-collapse-icon');
        $('#sidebarRailToggleBtn .rail-toggle-label')
            .text(isCollapsed ? 'Expand' : 'Collapse');

        // Topbar toggle button
        $('#topbarSidebarToggleBtn')
            .attr('title', titleText)
            .attr('aria-expanded', ariaValue)
            .find('i')
            .attr('class', isCollapsed ? 'bi bi-layout-sidebar-inset' : 'bi bi-layout-sidebar');
    }

    function toggleDesktopSidebar(explicitState) {
        const isCollapsed = typeof explicitState === 'boolean'
            ? explicitState
            : !$shell.hasClass('sidebar-collapsed');

        $shell.toggleClass('sidebar-collapsed', isCollapsed);
        localStorage.setItem('qc-sidebar-collapsed', isCollapsed ? 'true' : 'false');
        updateSidebarToggleUI(isCollapsed);
    }

    // Attach click listeners to all sidebar toggle triggers (Header, Rail Footer, Topbar)
    $('#sidebarCollapseBtn, #sidebarRailToggleBtn, #topbarSidebarToggleBtn').on('click', function () {
        toggleDesktopSidebar();
    });

    // Synchronize toggle UI state on initial page load if restored from localStorage
    if ($shell.hasClass('sidebar-collapsed')) {
        updateSidebarToggleUI(true);
    }

    $mobileBtn.on('click', function () {
        $shell.toggleClass('sidebar-mobile-open');
        const isOpen = $shell.hasClass('sidebar-mobile-open');
        $mobileBtn.attr('aria-expanded', isOpen ? 'true' : 'false');
    });

    // Close mobile sidebar when clicking outside
    $(document).on('click', function (e) {
        if ($shell.hasClass('sidebar-mobile-open')) {
            if (!$(e.target).closest('.app-sidebar, #mobileMenuBtn').length) {
                $shell.removeClass('sidebar-mobile-open');
                $mobileBtn.attr('aria-expanded', 'false');
            }
        }
    });

    // =========================================================================
    // 3. Global Keyboard Shortcuts & Accessibility Keybindings
    // =========================================================================
    $(document).on('keydown', function (e) {
        // Escape: Close mobile sidebar and restore focus
        if (e.key === 'Escape') {
            if ($shell.hasClass('sidebar-mobile-open')) {
                $shell.removeClass('sidebar-mobile-open');
                $mobileBtn.attr('aria-expanded', 'false').focus();
            }
        }

        // Ctrl+B or Cmd+B: Toggle Sidebar
        if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'b') {
            e.preventDefault();
            toggleDesktopSidebar();
        }

        // Ctrl+K or Cmd+K: Global Search Focus
        if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
            e.preventDefault();
            const searchInput = $('#tableFilterInput, #globalSearchInput');
            if (searchInput.length) {
                searchInput.first().focus().select();
            } else {
                $('#globalSearchTrigger').trigger('click');
            }
        }
    });

    // =========================================================================
    // 4. Toastr Notification Ingestion & WCAG Live Announcer
    // =========================================================================
    function announceToScreenReader(message) {
        const announcer = document.getElementById('qc-live-announcer');
        if (announcer && message) {
            announcer.textContent = '';
            setTimeout(function () {
                announcer.textContent = message;
            }, 60);
        }
    }

    const toastEl = document.getElementById('toast-notifications');
    if (toastEl && typeof toastr !== 'undefined') {
        toastr.options = {
            closeButton: true,
            progressBar: true,
            positionClass: 'toast-top-right',
            timeOut: 4000,
            extendedTimeOut: 1500,
            escapeHtml: true
        };

        const successMsg = toastEl.getAttribute('data-success');
        const errorMsg = toastEl.getAttribute('data-error');
        const warningMsg = toastEl.getAttribute('data-warning');
        const infoMsg = toastEl.getAttribute('data-info');

        if (successMsg) {
            toastr.success(successMsg);
            announceToScreenReader('Success: ' + successMsg);
        }
        if (errorMsg) {
            toastr.error(errorMsg);
            announceToScreenReader('Error: ' + errorMsg);
        }
        if (warningMsg) {
            toastr.warning(warningMsg);
            announceToScreenReader('Warning: ' + warningMsg);
        }
        if (infoMsg) {
            toastr.info(infoMsg);
            announceToScreenReader('Notice: ' + infoMsg);
        }
    }

    // =========================================================================
    // 5. Anti-Double-Submission & Button Loading Spinner
    // =========================================================================
    $('form').on('submit', function () {
        const form = $(this);
        // Only lock if form is valid (using jQuery validate if present)
        if (typeof form.valid === 'function' && !form.valid()) {
            return;
        }

        const submitBtn = form.find('button[type="submit"], input[type="submit"]');
        if (submitBtn.length && !submitBtn.hasClass('no-spinner')) {
            // Prevent immediate repeated clicks without disabling the DOM element before serialization
            submitBtn.addClass('is-submitting').css('pointer-events', 'none');
            const originalHtml = submitBtn.html();
            submitBtn.data('original-html', originalHtml);
            const loadingText = submitBtn.data('loading-text') || 'Processing...';
            submitBtn.html(`<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> ${loadingText}`);

            // Defer disabling to next tick so browser form submission can serialize button name/value
            setTimeout(function () {
                submitBtn.prop('disabled', true);
            }, 0);

            // Safety timeout to re-enable button after 12s in case of navigation abort
            setTimeout(function () {
                submitBtn.removeClass('is-submitting').css('pointer-events', '').prop('disabled', false);
                if (submitBtn.data('original-html')) {
                    submitBtn.html(submitBtn.data('original-html'));
                }
            }, 12000);
        }
    });

    // =========================================================================
    // 6. Global Anti-Forgery Header for all jQuery AJAX Requests
    // =========================================================================
    $.ajaxPrefilter(function (options, originalOptions, jqXHR) {
        if (options.type && options.type.toUpperCase() !== 'GET') {
            const token = $('input[name="__RequestVerificationToken"]').first().val();
            if (token) {
                jqXHR.setRequestHeader('RequestVerificationToken', token);
            }
        }
    });

    // =========================================================================
    // 7. Hide legacy preloader immediately if present
    // =========================================================================
    const $preloader = $('#preloader');
    if ($preloader.length) {
        $preloader.fadeOut(150);
    }
});

// =========================================================================
// 8. Shared DataTables SweetAlert2 Deletion Helper
// =========================================================================
window.qcDeleteRecord = function (options) {
    const opts = Object.assign({
        url: '',
        table: null,
        title: 'Are you sure?',
        text: 'This action cannot be undone.',
        successMsg: 'Record deleted successfully.'
    }, options);

    Swal.fire({
        title: opts.title,
        text: opts.text,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#ef4444',
        cancelButtonColor: '#64748b',
        confirmButtonText: 'Yes, Delete',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: opts.url,
                type: 'DELETE',
                success: function (data) {
                    if (data && data.success) {
                        if (opts.table && opts.table.ajax) {
                            opts.table.ajax.reload();
                        }
                        toastr.success(data.message || opts.successMsg);
                    } else {
                        toastr.error((data && data.message) || 'Error deleting record.');
                    }
                },
                error: function () {
                    toastr.error('Something went wrong while communicating with the server.');
                }
            });
        }
    });
};
