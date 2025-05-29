using System.Text;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions
{
    /// <summary>
    /// Extension methods for configuring Identity and JWT authentication services
    /// </summary>
    public static class IdentityServiceExtensions
    {
        /// <summary>
        /// Configures Identity, JWT authentication, and authorization policies
        /// </summary>
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, 
            IConfiguration config)
        {
            // Configure Identity with relaxed password requirements
            services.AddIdentityCore<AppUser>(opt => 
            {
                opt.Password.RequireNonAlphanumeric = false;
            })
                .AddRoles<AppRole>()
                .AddRoleManager<RoleManager<AppRole>>()
                .AddEntityFrameworkStores<DataContext>();

             // Configure JWT Bearer authentication
             services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => 
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding
                            .UTF8.GetBytes(config["TokenKey"])), // Get key from config
                        ValidateIssuer = false, // Simplified for development
                        ValidateAudience = false // Simplified for development
                    };

                    // Special handling for SignalR JWT tokens
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            var path = context.HttpContext.Request.Path;
                            // Extract token from query string for SignalR hubs
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });
            
            // Configure authorization policies
            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("RequireAdminRole", policy => policy.RequireRole("AdminRole"));
                opt.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("AdminRole", "Moderator"));
            });

            return services; 
        }
    }
}
