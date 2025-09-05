using Microsoft.Extensions.Http.Logging;
using Newtonsoft.Json;
namespace MediaNest.Web; 
public class ApiClient (HttpClient client){
   
    public Task<T> GetFromJsonAsync<T>(string path) {
        return client.GetFromJsonAsync<T>(path);
    }
    public async Task<T1> PostAsync<T1, T2>(string path, T2 item) {
        var result = await client.PostAsJsonAsync(path, item);
        if(result != null && result.IsSuccessStatusCode) {
            return JsonConvert.DeserializeObject<T1>(await result.Content.ReadAsStringAsync());
        }
        return default;
    }
    public async Task<T1> PutAsync<T1, T2>(string path, T2 item) {
        var result = await client.PutAsJsonAsync(path, item);
        if(result != null && result.IsSuccessStatusCode) {
            return JsonConvert.DeserializeObject<T1>(await result.Content.ReadAsStringAsync());
        }
        return default;
    }
    public async Task<T> DeleteAsync<T>(string path) {
        return await client.DeleteFromJsonAsync<T>(path);
    }
}
