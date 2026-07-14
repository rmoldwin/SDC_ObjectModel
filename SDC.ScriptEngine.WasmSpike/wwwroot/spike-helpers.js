// spike-helpers.js
// JavaScript helpers for the Wasm Spike page.
//
// Why JavaScript here?
//   In .NET 10, Blazor WASM no longer serves a blazor.boot.json manifest.
//   Instead, the asset inventory (including fingerprinted DLL filenames) is
//   embedded as a JSON literal inside the generated dotnet.js file, between
//   the markers /*json-start*/ and /*json-end*/.
//
//   At runtime the browser has already fetched the correct (fingerprinted) dotnet.js.
//   We can locate its URL via the <link rel="preload"> element that the build injects
//   into the page, then re-fetch the same file as text and extract the JSON.

/**
 * Returns the full JSON config string embedded in dotnet.js.
 * The JSON is between /*json-start*\/ and /*json-end*\/ markers.
 * Returns null if the preload link is not found or the fetch fails.
 */
window.getSpikeBootConfig = async function () {
    // Find the preload <link> that points to dotnet.js.
    // The build produces: <link href="_framework/dotnet.<fingerprint>.js" rel="preload" .../>
    const links = Array.from(document.querySelectorAll('link[rel="preload"]'));
    const dotnetLink = links.find(l => /\/_framework\/dotnet\.[a-z0-9]+\.js$/.test(l.href));
    if (!dotnetLink) {
        console.warn('[SpikeHelper] Could not find dotnet.js preload link. Links found:', links.map(l => l.href));
        return null;
    }

    console.log('[SpikeHelper] Fetching config from:', dotnetLink.href);
    const response = await fetch(dotnetLink.href);
    if (!response.ok) {
        console.error('[SpikeHelper] Fetch failed:', response.status, response.statusText);
        return null;
    }
    const text = await response.text();

    // Extract the JSON block between the two markers.
    const startMarker = '/*json-start*/';
    const endMarker = '/*json-end*/';
    const startIdx = text.indexOf(startMarker);
    const endIdx = text.indexOf(endMarker);
    if (startIdx === -1 || endIdx === -1) {
        console.error('[SpikeHelper] Could not find json-start/json-end markers in dotnet.js');
        return null;
    }

    return text.substring(startIdx + startMarker.length, endIdx).trim();
};
