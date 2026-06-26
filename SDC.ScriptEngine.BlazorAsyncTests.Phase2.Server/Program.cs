// Program.cs — SDC.ScriptEngine.BlazorAsyncTests.Phase2.Server
// Thin ASP.NET Core host that injects COOP/COEP headers required for SharedArrayBuffer
// (needed by WasmEnableThreads=true in the Phase2 client). Run with:
//   dotnet run --project SDC.ScriptEngine.BlazorAsyncTests.Phase2.Server

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// CRITICAL: must be before UseBlazorFrameworkFiles/UseStaticFiles.
// These headers enable SharedArrayBuffer, required for WasmEnableThreads=true.
// Middleware order is load-bearing: headers apply to _framework/*.wasm, blazor.boot.json, etc.
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Cross-Origin-Opener-Policy", "same-origin");
    context.Response.Headers.Append("Cross-Origin-Embedder-Policy", "require-corp");
    await next();
});

app.UseBlazorFrameworkFiles();   // serves _framework/ from client project build output
app.UseStaticFiles();            // serves wwwroot/
app.MapFallbackToFile("index.html");
app.Run();
