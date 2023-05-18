namespace GameStore.Api.OpenAPI;

public static class SwaggerExtentions
{
    public static IApplicationBuilder UseGameStoreSwagger(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(option =>
            {
                foreach (var description in app.DescribeApiVersions())
                {
                    var url = $"/swagger/{description.GroupName}/swagger.json";
                    var name = description.GroupName.ToUpperInvariant();
                    option.SwaggerEndpoint(url, name);
                }
            });
        }

        return app;
    }
}