using MediaNest.Shared.Dtos;
using MediaNest.Web.AuthStateProvider;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;
namespace MediaNest.Web; 
public class ApiClient (HttpClient client, ProtectedLocalStorage localStorage, AuthenticationStateProvider authStateProvider){
    public async Task SetAuthorizeHeader() {
        var session = (await localStorage.GetAsync<AuthResponse>("sessionState")).Value;
        if(session != null && !string.IsNullOrEmpty(session.Token)) {
            if (session.Expiration < DateTime.UtcNow) {
                await ((CustomAuthStateProvider)authStateProvider).MarkUserAsLoggedOut();
            }
            else if (session.Expiration < DateTime.UtcNow.AddMinutes(10)) {
                var result = await client.GetFromJsonAsync<AuthResponse>($"/api/account/refreshLogin?refreshToken={session.RefreshToken}");
                if (result != null) {
                    await ((CustomAuthStateProvider)authStateProvider).MarkUserAsAuthenticated(result);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
                }
                else {
                    await ((CustomAuthStateProvider) authStateProvider).MarkUserAsLoggedOut();
                }
            }
            else {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
            }
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
            // return JsonConvert.DeserializeObject<T1>(await result.Content.ReadAsStringAsync()); // newtonsoft
            return await result.Content.ReadFromJsonAsync<T1>();
        }
        return default;
    }
    public async Task<T1> PutAsync<T1, T2>(string path, T2 item) {

        await SetAuthorizeHeader();

        var result = await client.PutAsJsonAsync(path, item);
        if(result != null && result.IsSuccessStatusCode) {
            // return JsonConvert.DeserializeObject<T1>(await result.Content.ReadAsStringAsync());  // newtonsoft
            return await result.Content.ReadFromJsonAsync<T1>();
        }
        return default;
    }
    public async Task<T> DeleteAsync<T>(string path) {
        await SetAuthorizeHeader();
        return await client.DeleteFromJsonAsync<T>(path);
    }
}
