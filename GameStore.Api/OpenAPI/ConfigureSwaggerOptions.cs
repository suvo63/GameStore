using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GameStore.Api.OpenAPI;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach(var description in _provider.ApiVersionDescriptions){
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo(){
                    Title=$"Game store API {description.ApiVersion}",
                    Version=description.ApiVersion.ToString(),
                    Description="Manages the games catalog."
                }
            );
        }
    }
}