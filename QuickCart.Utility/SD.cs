using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCart.Utility
{
    public class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";
        public const string Role_Company = "Company";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusInProcess = "Progressing";
        public const string StatusShipped = "Shipped";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusDelayedPayment = "ApprovedForDelayedPayment";
        public const string PaymentStatusRejected = "Rejected";

        public const string SessionCart = "SessionShoppingCart";

        /// <summary>
        /// Sanitizes return URLs to prevent open redirects and self-referential redirect loops
        /// back to login or external authentication callbacks.
        /// </summary>
        public static string CleanReturnUrl(string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return "~/";
            }

            // Reject open redirects or protocol-relative schemes
            if (returnUrl.StartsWith("//") || returnUrl.StartsWith("/\\") ||
                returnUrl.Contains("://", StringComparison.Ordinal))
            {
                return "~/";
            }

            // Must be a relative local path
            if (!returnUrl.StartsWith('/') && !returnUrl.StartsWith("~/"))
            {
                return "~/";
            }

            // Prevent self-referential redirect loops back to login/external login pages
            if (returnUrl.Contains("/Identity/Account/Login", StringComparison.OrdinalIgnoreCase) ||
                returnUrl.Contains("/Identity/Account/ExternalLogin", StringComparison.OrdinalIgnoreCase) ||
                returnUrl.Contains("/Identity/Account/Register", StringComparison.OrdinalIgnoreCase) ||
                returnUrl.Contains("handler=Callback", StringComparison.OrdinalIgnoreCase))
            {
                return "~/";
            }

            return returnUrl;
        }
    }
}
