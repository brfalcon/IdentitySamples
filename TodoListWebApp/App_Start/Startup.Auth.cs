﻿using Common;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace TodoListWebApp
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            var postLogoutRedirectUri = new Uri(new Uri(SiteConfiguration.TodoListWebAppRootUrl), urlHelper.Action("SignedOut", "Account")).ToString();

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = SiteConfiguration.TodoListWebAppClientId,
                    Authority = SiteConfiguration.AadAuthority,
                    PostLogoutRedirectUri = postLogoutRedirectUri,

                    TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters
                    {
                        NameClaimType = "name", // [NOTE] This indicates that the user's display name is defined in the "name" claim
                        RoleClaimType = "roles" // [NOTE] This indicates that the user's roles are defined in the "roles" claim
                    },

                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        AuthorizationCodeReceived = async (context) =>
                        {
                            // If there is an authorization code in the OpenID Connect response, redeem it for an access token and refresh token,
                            // and store those away in the cache.
                            // Note that typically this is a "Multiple Resource Refresh Token" which means the refresh token can be used not only against
                            // the requested "resource" but also against any other resource defined in the same directory tenant the user has access to.
                            var credential = new ClientCredential(SiteConfiguration.TodoListWebAppClientId, SiteConfiguration.TodoListWebAppClientSecret);
                            var userId = context.AuthenticationTicket.Identity.GetUniqueIdentifier();
                            var authContext = new AuthenticationContext(SiteConfiguration.AadAuthority, TokenCacheFactory.GetTokenCache(userId));
                            var result = await authContext.AcquireTokenByAuthorizationCodeAsync(context.Code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, SiteConfiguration.TodoListWebApiResourceId);
                        },
                        AuthenticationFailed = context =>
                        {
                            context.HandleResponse();
                            context.Response.Redirect(urlHelper.Action("Error", "Home", new { message = context.Exception.Message }));
                            return Task.FromResult(0);
                        }
                    }
                });
        }
    }
}