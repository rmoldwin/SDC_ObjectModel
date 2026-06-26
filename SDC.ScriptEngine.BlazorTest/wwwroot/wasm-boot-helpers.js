// wasm-boot-helpers.js
// Helper to extract the .NET 10 boot manifest from dotnet.*.js.
//
// BACKGROUND — .NET 10 BOOT MANIFEST CHANGE
// ------------------------------------------
// In .NET 8 and earlier, Blazor WASM deployed a blazor.boot.json file that
// listed all assemblies in the app bundle.  In .NET 10, blazor.boot.json no
// longer exists.  Instead, the asset manifest is embedded as a JSON literal
// inside the fingerprinted dotnet.*.js file (e.g. dotnet.pf18hs7ni0.js)
// between two sentinel markers:
//
//   /*json-start*/{ "mainAssemblyName": "...", "resources": { "assembly": [...], ... } }/*json-end*/
//
// Each assembly entry in the manifest looks like:
//   { "virtualPath": "SDC.Schema.dll", "name": "SDC.Schema.uate0am3dw.dll", ... }
//
// The C# WasmReferenceProvider class calls this function via IJSRuntime to
// discover the fingerprinted names of all deployed DLLs, then fetches them
// by name from _framework/.
//
// WHY PRELOAD LINK?
// -----------------
// The blazor.webassembly.js loader adds a <link rel="preload"> for dotnet.*.js
// into the DOM as part of WASM boot.  We find that element to get the
// fingerprinted URL rather than hard-coding the filename (which changes with
// every publish).
window.getWasmBootManifest = async function () {
    // Find the <link rel="preload"> tag pointing to dotnet.*.js.
    // The fingerprinted URL looks like: _framework/dotnet.abc123.js
    const link = document.querySelector('link[rel="preload"][href*="dotnet."][href$=".js"]');
    if (!link) {
        console.warn('getWasmBootManifest: could not find dotnet.*.js preload link');
        return null;
    }
    let src;
    try {
        const response = await fetch(link.href);
        if (!response.ok) {
            console.warn('getWasmBootManifest: fetch failed for', link.href, response.status);
            return null;
        }
        src = await response.text();
    } catch (err) {
        console.warn('getWasmBootManifest: fetch error:', err);
        return null;
    }
    const startMarker = '/*json-start*/';
    const endMarker   = '/*json-end*/';
    const start = src.indexOf(startMarker);
    const end   = src.indexOf(endMarker);
    if (start < 0 || end < 0) {
        console.warn('getWasmBootManifest: boot JSON markers not found in dotnet.js');
        return null;
    }
    try {
        return JSON.parse(src.slice(start + startMarker.length, end));
    } catch (parseErr) {
        console.warn('getWasmBootManifest: JSON parse failed:', parseErr);
        return null;
    }
};
