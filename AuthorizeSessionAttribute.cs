using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace RaceDay.Mvc.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizeSessionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public AuthorizeSessionAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;
            var sessionToken = session.GetString("SessionToken");

            // 1. Check if user is authenticated
            if (string.IsNullOrWhiteSpace(sessionToken))
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "controller", "Account" },
                    { "action", "Login" },
                    { "returnUrl", returnUrl }
                });
                return;
            }

            // 2. Check role if specific roles are required
            if (_allowedRoles != null && _allowedRoles.Length > 0)
            {
                var userRole = session.GetString("UserRole") ?? string.Empty;
                var hasRole = _allowedRoles.Any(r => r.Equals(userRole, StringComparison.OrdinalIgnoreCase));

                if (!hasRole)
                {
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary
                    {
                        { "controller", "Account" },
                        { "action", "AccessDenied" }
                    });
                }
            }
        }
    }
}
