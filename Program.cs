using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Tantalus.Data;
using Tantalus.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.Configure<Settings>(builder.Configuration.GetSection("Settings"));

services.AddDbContext<DataContext>();
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;     // absurd requirement to correctly map Postgres attribute names

services.AddCors();

services.AddControllers().AddJsonOptions(options => {
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Settings:Issuer"],
            ValidAudience = builder.Configuration["Settings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Settings:Secret"])),
        };
    });

services.AddAuthorization();

services.AddScoped<IUserService, UserService>();
services.AddScoped<IFoodService, FoodService>();

var app = builder.Build();

app.UseCors(policyBuilder => policyBuilder
    .SetIsOriginAllowed(_ => true)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();