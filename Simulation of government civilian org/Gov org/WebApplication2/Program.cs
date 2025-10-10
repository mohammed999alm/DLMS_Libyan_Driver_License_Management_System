using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();


builder.Services.AddControllers();

// Register configuration
var configuration = builder.Configuration;
string connectionString = configuration.GetConnectionString("DLVDAppCon");

// Initialize the static data access class with the connection string

if (connectionString == null)
{
    Console.WriteLine("Connection string is null. Please check the appsettings.json file.");
}

else
{
    Console.WriteLine($"Connection String: {connectionString}");
}

DataAccessLayer.DataAccessSettings.Initialize(connectionString);




// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader();


        //policyBuilder.WithOrigins("http://localhost:8000/Desktop/indexFetchData.html,")
        //            .AllowAnyMethod()
        //            .AllowAnyHeader();

    });
});


//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowSpecificOrigins", policyBuilder =>
//    {
//        policyBuilder.WithOrigins("http://localhost:8000", "http://localhost:8080") // Exact match required
//                     .AllowAnyMethod()
//                     .AllowAnyHeader();
//    });
//});

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowSpecificOrigins", policyBuilder =>
//    {
//        policyBuilder.WithOrigins("http://localhost:8000", "http://localhost:8080", "http://localhost:64873") // Add your server origin here
//                     .AllowAnyMethod()
//                     .AllowAnyHeader();
//        //policyBuilder.WithHeaders("http://localhost:8000", "http://localhost:8080").AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
//    });
//});




builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new ()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "myIssuer",
            ValidAudience = "myAudience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("mySuperSecureSecretKeyWith256Bits123456789"))
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
app.UseCors("AllowSpecificOrigins");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//app.UseStaticFiles();
//app.UseRouting();

app.Run();

