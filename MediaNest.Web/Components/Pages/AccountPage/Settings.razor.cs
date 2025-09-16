using Blazored.Toast.Services;
using MediaNest.Shared.Dtos;
using MediaNest.Shared.Entities;
using MediaNest.Web.AuthStateProvider;
using MediaNest.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace MediaNest.Web.Components.Pages.AccountPage; 
public partial class Settings : ComponentBase{
    [Inject] public ApiClient ApiClient { get; set; }
    [Inject] public AuthenticationStateProvider AuthProvider { get; set; }
    [Inject] public SettingService SettingService { get; set; }
    [Inject] public IToastService Toast { get; set; }
    [Inject] public NavigationManager Navigation { get; set; }
    private List<Account> _users;
    private string _showDeleteConfirmFor;

    private AuthRequest changePasswordRequest = new();
    private string _newPassword;
    private string _confirmPassword;

    protected override async Task OnInitializedAsync() {
        var authState = await AuthProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated ?? false && authState.User.IsInRole("Admin")) {
            _users = await ApiClient.GetAsync<List<Account>>("/api/account/users");
        }
    }

    private async Task UpdateRole(Account user) {
        var request = new AccountUpdateRequest {
            Username = user.Username,
            Role = user.Role
        };

        await ApiClient.PutAsync<string, AccountUpdateRequest>($"/api/account/users/updateRole", request);
        _users = await ApiClient.GetAsync<List<Account>>("/api/account/users");

    }
    private void ShowCheckDeleteButton(string username) {
        _showDeleteConfirmFor = username;
    }
    private void CancelDelete() {
        _showDeleteConfirmFor = null;
    }
    private async Task DeleteAccount(string username) {
        var response = await ApiClient.DeleteAsync<AuthResponse>($"/api/account/delete/{username}");
        if (response != null && response.Message?.Contains("success", StringComparison.OrdinalIgnoreCase) == true) {
            Toast.ShowSuccess("Account deleted successfully.");
            _users = await ApiClient.GetAsync<List<Account>>("/api/account/users");
        }
        else {
            Toast.ShowError(response?.Message ?? "Delete failed.");
        }
        _showDeleteConfirmFor = null;
    }
    private async Task ChangePassword() {
        var authState = await AuthProvider.GetAuthenticationStateAsync();
        var username = authState.User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username)) {
            Toast.ShowError("User not authenticated.");
            return;
        }
        changePasswordRequest.Username = username;

        if (string.IsNullOrWhiteSpace(_newPassword) || string.IsNullOrWhiteSpace(_confirmPassword)) {
            Toast.ShowError("New password and confirmation are required.");
            return;
        }
        if (_newPassword != _confirmPassword) {
            Toast.ShowError("New password and confirmation do not match.");
            return;
        }
        // 將新密碼暫存於 Message 欄位（後端需支援）
        changePasswordRequest.Password = _newPassword;
        var response = await ApiClient.PostAsync<AuthResponse, AuthRequest>("/api/account/changePassword", changePasswordRequest);
        if (response != null && response.Token != null) {
            await ((CustomAuthStateProvider)AuthProvider).MarkUserAsAuthenticated(response);

            Toast.ShowSuccess("Password changed successfully.");
            changePasswordRequest.Password = "";
            _newPassword = "";
            _confirmPassword = "";
            Navigation.NavigateTo(Navigation.Uri, forceLoad: true);
        }
        else {
            Toast.ShowError(response?.Message ?? "Password change failed.");
        }
    }

    private bool showChangePasswordPanel = false;

    private void ToggleChangePasswordPanel() {
        showChangePasswordPanel = !showChangePasswordPanel;
    }
}
