using GlobalUtility;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.IncludeFields = true;
});


// Register configuration
var configuration = builder.Configuration;
string connectionString = configuration.GetConnectionString("DLVDAppCon");

// Initialize the static data access class with the connection string

//if (connectionString == null)
//{
//    //Console.WriteLine("Connection string is null. Please check the appsettings.json file.");
//}

//else
//{
//    //Console.WriteLine($"Connection String: {connectionString}");
//}


DLMS_DataAccessLayer.DataAccessSettings.InjectConntection(connectionString);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DLMS API", Version = "v1" });

    // ✅ Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and your JWT token.\nExample: Bearer abc.def.ghi"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//builder.Services.AddSwaggerGen(c =>
//{
//    c.OperationFilter<Swashbuckle.AspNetCore.Filters.Fil>();
//});

//builder.Services.AddSwaggerGen(c =>
//{
//    c.OperationFilter<FileUploadOperationFilter>();
//});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader();

    });
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // Limit size to 10 MB for file uploads
});



builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "myIssuer",
            ValidAudiences = new[] { "DLMS_Desktop", "DLMS_MobileApp" },
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("mySuperSecureSecretKeyWith256Bits123456789")),
            ClockSkew = TimeSpan.Zero
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors("AllowAllOrigins");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

//app.MapPersonEndpoints();

//Console.WriteLine(PDF_Builder.ImageURL);
//Console.WriteLine(PDF_Builder.TemplatePath);

//if (File.Exists(PDF_Builder.TemplatePath))
//    Console.WriteLine("It's Exist Invoice Template");
////using System.Diagnostics;

//if (File.Exists(PDF_Builder.TemplatePath))
//{
//    Console.WriteLine("Template exists. Opening...");
//    Process.Start(new ProcessStartInfo(PDF_Builder.TemplatePath) { UseShellExecute = true });
//}


app.Run();

