using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Newtonsoft.Json;
using System.Net.Http.Headers;
namespace MediaNest.Web; 
public class ApiClient (HttpClient client, ProtectedLocalStorage localStorage){
    public async Task SetAuthorizeHeader() {
        var token = (await localStorage.GetAsync<string>("authToken")).Value;
        if(token != null) {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
    public async Task<T> GetFromJsonAsync<T>(string path) {
        await SetAuthorizeHeader();
        return await client.GetFromJsonAsync<T>(path);
    }
    public async Task<T1> PostAsync<T1, T2>(string path, T2 item) {
        await SetAuthorizeHeader();

        var result = await client.PostAsJsonAsync(path, item);
        if(result != null && result.IsSuccessStatusCode) {
            return JsonConvert.DeserializeObject<T1>(await result.Content.ReadAsStringAsync());
        }
        return default;
    }
    public async Task<T1> PutAsync<T1, T2>(string path, T2 item) {

        await SetAuthorizeHeader();

        var result = await client.PutAsJsonAsync(path, item);
        if(result != null && result.IsSuccessStatusCode) {
            return JsonConvert.DeserializeObject<T1>(await result.Content.ReadAsStringAsync());
        }
        return default;
    }
    public async Task<T> DeleteAsync<T>(string path) {
        await SetAuthorizeHeader();
        return await client.DeleteFromJsonAsync<T>(path);
    }
}
