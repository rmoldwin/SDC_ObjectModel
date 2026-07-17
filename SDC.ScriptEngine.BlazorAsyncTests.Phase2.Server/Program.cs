// Program.cs — SDC.ScriptEngine.BlazorAsyncTests.Phase2.Server
// Thin ASP.NET Core host that injects COOP/COEP headers required for SharedArrayBuffer
// (needed by WasmEnableThreads=true in the Phase2 client). For multi-threaded WASM, publish and run the output:
//   dotnet publish SDC.ScriptEngine.BlazorAsyncTests.Phase2.Server -c Release -o publish_out
//   cd publish_out && dotnet SDC.ScriptEngine.BlazorAsyncTests.Phase2.Server.dll
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets(); // required in dev (dotnet run) to discover WASM client wwwroot
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
