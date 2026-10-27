using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

// No pages: the playtest page's own script drives everything through Bridge.
var builder = WebAssemblyHostBuilder.CreateDefault(args);
await builder.Build().RunAsync();
