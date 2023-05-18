using Azure.Storage.Blobs;
using GameStore.Api.Authorization;
using GameStore.Api.Cors;
using GameStore.Api.Data;
using GameStore.Api.Endpoints;
using GameStore.Api.ErrorHandling;
using GameStore.Api.ImageUpload;
using GameStore.Api.Middleware;
using GameStore.Api.OpenAPI;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRepositories(builder.Configuration);

builder.Services.AddAuthentication()
                .AddJwtBearer()
                .AddJwtBearer("Auth0");
builder.Services.AddGameStoreAuthorization();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new(1.0);
    options.AssumeDefaultVersionWhenUnspecified = true;
})
.AddApiExplorer(options => options.GroupNameFormat = "'v'VVV");

builder.Services.AddGameStoreCors(builder.Configuration);

builder.Services.AddSwaggerGen()
                .AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>()
                .AddEndpointsApiExplorer(); //for minimal api framework, not needed for MVC

builder.Services.AddSingleton<IImageUploader>(
    new ImageUploader(
        new BlobContainerClient(
            builder.Configuration.GetConnectionString("AzureStorage"),
            "images"
        )
    )
);

builder.Logging.AddAzureWebAppDiagnostics();

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp => exceptionHandlerApp.ConfigureExceptionHandler());
app.UseMiddleware<RequestTimingMiddleware>();

await app.Services.InitializeDbAsync();

app.UseHttpLogging();

app.MapGamesEndpoints();
app.MapImagesEndpoints();

app.UseCors();

app.UseGameStoreSwagger();

app.Run();

//Log in JSON format with indentaion
// builder.Logging.AddJsonConsole(options =>
// {
//     options.JsonWriterOptions = new()
//     {
//         Indented = true
//     };
// });
