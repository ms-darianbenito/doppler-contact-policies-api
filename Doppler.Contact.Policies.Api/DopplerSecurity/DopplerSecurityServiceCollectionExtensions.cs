using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace doppler_contact_policies_api.DopplerSecurity
{
    public static class DopplerSecurityServiceCollectionExtensions
    {
        public static IServiceCollection AddDopplerSecurity(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationHandler, IsSuperUserAuthorizationHandler>();

            services.AddOptions<AuthorizationOptions>()
                  .Configure(o =>
                  {
                      var simpleAuthenticationPolicy = new AuthorizationPolicyBuilder()
                          .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                          .RequireAuthenticatedUser()
                          .Build();

                      var onlySuperUserPolicy = new AuthorizationPolicyBuilder()
                          .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                          .AddRequirements(new DopplerAuthorizationRequirement()
                          {
                              AllowSuperUser = true
                          })
                          .RequireAuthenticatedUser()
                          .Build();

                      var ownResourceOrSuperUserPolicy = new AuthorizationPolicyBuilder()
                          .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                          .AddRequirements(new DopplerAuthorizationRequirement()
                          {
                              AllowSuperUser = true,
                              AllowOwnResource = true
                          })
                          .RequireAuthenticatedUser()
                          .Build();

                    // TODO: I would like to use ownResourceOrSuperUserPolicy as the default policy, but I
                    // cannot override a more restrictive policy with a less restrictive one. So,
                    // for the moment, we have to be carefull and chooses the right one for each
                    // controller.
                    o.DefaultPolicy = simpleAuthenticationPolicy;

                      o.AddPolicy(Policies.ONLY_SUPERUSER, onlySuperUserPolicy);
                  });

            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<DopplerSecurityOptions>>((o, securityOptions) =>
                {
                    o.SaveToken = true;
                    o.TokenValidationParameters = new TokenValidationParameters()
                    {
                        IssuerSigningKeys = securityOptions.Value.SigningKeys,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                    };
                });

            services.AddAuthentication()
                .AddJwtBearer();

            services.AddAuthorization();

            return services;
        }
    }
}
