using Api;
using Api.Infrastructure;
using JasperFx;
using Scalar.AspNetCore;
using Serilog;
using Shared.Endpoints;
using Shared.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddModules(ModuleRegistry.Modules);
#if (UseAuth)
builder.AddApiAuthentication();
#endif
builder.Services.AddApiRateLimiting(builder.Configuration);
builder.Services.AddApiCors(builder.Configuration);
builder.Services.AddApiOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseSecurityHeaders();
app.UseSerilogRequestLogging();
app.UseCors();
#if (UseAuth)
app.UseAuthentication();
app.UseAuthorization();
#endif
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapDefaultEndpoints();
#if (UseAuth)
app.MapModuleEndpoints(ModuleRegistry.Modules, group => group.RequireAuthorization());
#else
app.MapModuleEndpoints(ModuleRegistry.Modules);
#endif

// RunJasperFxCommands enables Wolverine's CLI, e.g. `dotnet run -- codegen write` or `dotnet run -- describe`.
return await app.RunJasperFxCommands(args);
