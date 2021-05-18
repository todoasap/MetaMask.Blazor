using Microsoft.JSInterop;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace MetaMask.Blazor
{
    // This class provides JavaScript functionality for MetaMask wrapped
    // in a .NET class for easy consumption. The associated JavaScript module is
    // loaded on demand when first needed.
    //
    // This class can be registered as scoped DI service and then injected into Blazor
    // components for use.

    public class MetaMaskJsInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        public MetaMaskJsInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => LoadScripts(jsRuntime).AsTask());
        }

        public ValueTask<IJSObjectReference> LoadScripts(IJSRuntime jsRuntime)
        {
            //await jsRuntime.InvokeAsync<IJSObjectReference>("import", "https://cdn.ethers.io/lib/ethers-5.1.0.umd.min.js");
            return jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/MetaMask.Blazor/metaMaskJsInterop.js");
        }

        public async ValueTask LoadMetaMask()
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("loadMetaMask");
        }

        public async ValueTask<string> GetAddress()
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<string>("getAddress", null);
        }

        public async ValueTask<int> GetTransactionCount()
        {
            var module = await moduleTask.Value;
            var result = await module.InvokeAsync<JsonElement>("getTransactionCount");
            var resultString = result.GetString()?.Replace("0x", string.Empty);
            if (resultString != null)
            {
                int intValue = int.Parse(resultString, System.Globalization.NumberStyles.HexNumber);
                return intValue;
            }
            return 0;
        }

        public async ValueTask<string> SignTypedData(string label, string value)
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<string>("signTypedData", label, value);
        }

        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }
    }
}
