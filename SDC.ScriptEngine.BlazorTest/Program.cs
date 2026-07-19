// Program.cs — SDC.ScriptEngine.BlazorTest
// Minimal Blazor WASM host bootstrap.

using System;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SDC.ScriptEngine.BlazorTest;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient with BaseAddress is required by WasmReferenceProvider (Phase 2)
// for fetching DLL bytes from _framework/ to build Roslyn MetadataReferences.
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

await builder.Build().RunAsync();
