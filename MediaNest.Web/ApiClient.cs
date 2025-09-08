using MediaNest.Shared.Dtos;
using MediaNest.Web.AuthStateProvider;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net;
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
    public async Task<T> GetAsync<T>(string path) {
        await SetAuthorizeHeader();
        var response = await client.GetAsync(path);
        if (response != null && response.IsSuccessStatusCode) {
            return await response.Content.ReadFromJsonAsync<T>();
        }
        return default;
    }
    public async Task<T1> PostAsync<T1, T2>(string path, T2 item) {
        await SetAuthorizeHeader();

        var response = await client.PostAsJsonAsync(path, item);
        if(response != null && response.IsSuccessStatusCode) {
            return await response.Content.ReadFromJsonAsync<T1>();
        }
        else {
            try {
                var error = await response.Content.ReadFromJsonAsync<T1>();
                return error;
            }
            catch {
                return default;
            }
        }
    }
    public async Task<T1> PutAsync<T1, T2>(string path, T2 item) {

        await SetAuthorizeHeader();

        var response = await client.PutAsJsonAsync(path, item);
        if(response != null && response.IsSuccessStatusCode) {
            return await response.Content.ReadFromJsonAsync<T1>();
        }
        return default;
    }
    public async Task<T> DeleteAsync<T>(string path) {
        await SetAuthorizeHeader();

        var response = await client.DeleteAsync(path);
        if (response.StatusCode == HttpStatusCode.NoContent) {
            return default; 
        }

        if (response.IsSuccessStatusCode) {
            return await response.Content.ReadFromJsonAsync<T>();
        }
        return default;
    }
}
