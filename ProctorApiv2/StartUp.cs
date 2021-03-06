﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Owin;
using ProctorApiv2.Providers;
using ProctorApiv2.Models;
using ProctorApiv2.Utils;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;

[assembly: OwinStartup(typeof(ProctorApiv2.Startup))]
namespace ProctorApiv2
{
    public partial class Startup
    {
        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static string PublicClientId { get; private set; }

        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context and user manager to use a single instance per request
            app.CreatePerOwinContext(ProctorV2Context.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            //Configure the application for OAuth based flow
            ConfigureOAuthForJWT(app);
            ConfigureJWTConsumption(app);
        }

        public void ConfigureOAuthForJWT(IAppBuilder app)
        {
            var expireTime =
            PublicClientId = "self";
            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/api/Token"),
                Provider = new ApplicationOAuthProvider(PublicClientId),
                AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(ProctorApiv2.Utils.Configuration.TokenExpireTimeMinutes),
                // In production mode set AllowInsecureHttp = false
                AllowInsecureHttp = true,
                AccessTokenFormat = new JWTFormat(ProctorApiv2.Utils.Configuration.TokenIssuer),
            };
            app.UseOAuthAuthorizationServer(OAuthOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
        }

        private void ConfigureJWTConsumption(IAppBuilder app)
        {
            var issuer = ProctorApiv2.Utils.Configuration.TokenIssuer;
            string audienceId = Utils.Configuration.TokenAudienceId;
            byte[] audienceSecret = TextEncodings.Base64Url.Decode(Utils.Configuration.TokenAudienceSecret);

            // Api controllers with an [Authorize] attribute will be validated with JWT
            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    AllowedAudiences = new[] { audienceId },
                    IssuerSecurityTokenProviders = new IIssuerSecurityTokenProvider[]
                    {
                        new SymmetricKeyIssuerSecurityTokenProvider(issuer, audienceSecret)
                    }
                });
        }
    }
}