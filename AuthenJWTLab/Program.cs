using AuthenJWTLab.Middleware;
using AuthenJWTLab.Models;
using AuthenJWTLab.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Add services to the container.

builder.Services.AddControllersWithViews(options =>
{
    // Apply ExceptionFilter globally.
    options.Filters.Add(typeof(CommonExceptionFilter));
});

// Add DBContext service
var connectionString = config.GetConnectionString("linkToDb");
builder.Services.AddDbContext<LabDBContext>(options =>
{
    options.UseSqlServer(connectionString);
});
// Add Db Repository service
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Add Swagger service
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    //����api�p�����O�@
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        //��������Atype���http�ɡA�z�Lswagger�e�����{�Үɥi�H�ٲ�Bearer�e���
        Type = SecuritySchemeType.Http,
        //�ĥ�Bearer token
        Scheme = "Bearer",
        //bearer�榡�ϥ�jwt
        BearerFormat = "JWT",
        //�{�ҩ�bhttp request��header�W
        In = ParameterLocation.Header,
        //�y�z
        Description = "JWT���Ҵy�z"
    });
    //�s�@�B�~���L�o���A�L�oAuthorize�BAllowAnonymous�A�ƦܬO�S����attribute
    options.OperationFilter<SwaggerAuthorizeOperationFilter>();    
});

// Add Cookie access limited for XSS
builder.Services.AddAuthentication().AddCookie(c => c.Cookie.HttpOnly = true);

// Add Jwt Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // shall be true for production, also GetPrincipalFromExpiredToken()
            ValidateAudience = true, // shall be true for production, also GetPrincipalFromExpiredToken()
            ValidIssuer = config["JWT:Issuer"],
            ValidAudience = config["JWT:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Key"]))
        };
    });

// Add JWTManagerRepository service for generating JWT token and so on.
builder.Services.AddScoped<IJWTManagerRepository, JWTManagerRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add Jwt Authentication service
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
