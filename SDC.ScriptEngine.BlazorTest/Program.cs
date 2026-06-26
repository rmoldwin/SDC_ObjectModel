// Program.cs — SDC.ScriptEngine.BlazorTest
// Minimal Blazor WASM host bootstrap.  No extra services needed for Phase 1
// (no HttpClient for DLL fetching, since AppDomainReferenceProvider is tried
// first and falls back to hardcoded IL on failure).

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SDC.ScriptEngine.BlazorTest;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// No HttpClient registration required for Phase 1.
// Phase 2 (in-browser Roslyn compilation via DLL fetch) will add:
//   builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
