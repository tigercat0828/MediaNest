using Microsoft.JSInterop;

namespace MediaNest.Web.Services;
public class JSRuntimeService(IJSRuntime jSRuntime) {
    private readonly IJSRuntime JSRuntime = jSRuntime;
    public async Task CopyToClipboard(string text) {
        await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
    }
    public async Task<int> GetImageWidth(string imgId) {
        int? imgWidth = await JSRuntime.InvokeAsync<int?>("Utils.GetImageWidth", imgId);
        return imgWidth == null ? 0 : imgWidth.Value;
    }
}
