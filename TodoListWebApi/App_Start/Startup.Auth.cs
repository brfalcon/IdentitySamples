﻿using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.ActiveDirectory;
using Owin;
using System.IdentityModel.Tokens;

namespace TodoListWebApi
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            // Allow Cross-Origin Resource Sharing (CORS) from a Javascript client (SPA web application).
            app.UseCors(CorsOptions.AllowAll);

            // Use bearer authentication with tokens coming from Azure Active Directory.
            app.UseWindowsAzureActiveDirectoryBearerAuthentication(new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                {
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidAudience = SiteConfiguration.TodoListWebApiResourceId,
                        SaveSigninToken = true, // [NOTE] This places the original token on the ClaimsIdentity.BootstrapContext
                        NameClaimType = "name", // [NOTE] This indicates that the user's display name is defined in the "name" claim
                        RoleClaimType = "roles" // [NOTE] This indicates that the user's roles are defined in the "roles" claim
                    },
                    Tenant = SiteConfiguration.AadTenant
                });
        }
    }
}