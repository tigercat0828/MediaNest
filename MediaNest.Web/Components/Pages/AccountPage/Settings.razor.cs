using Blazored.Toast.Services;
using MediaNest.Shared.Dtos;
using MediaNest.Shared.Entities;
using MediaNest.Web.AuthStateProvider;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace MediaNest.Web.Components.Pages.AccountPage;

public partial class Settings : ComponentBase {
    [Inject] public ApiClient ApiClient { get; set; }
    [Inject] public AuthenticationStateProvider AuthProvider { get; set; }
    [Inject] public IToastService Toast { get; set; }
    [Inject] public NavigationManager Navigation { get; set; }
    private List<Account> _users;
    private string _showDeleteConfirmFor;

    private AccountUpdateRequest changePasswordRequest = new();
    private string _confirmPassword;

    protected override async Task OnInitializedAsync() {
        var authState = await AuthProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated ?? false && authState.User.IsInRole("Admin")) {
            _users = await ApiClient.GetAsync<List<Account>>("/api/account/users");
            changePasswordRequest.Username = authState.User.Identity?.Name;
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

        if (string.IsNullOrWhiteSpace(changePasswordRequest.Username)) {
            Toast.ShowError("User not authenticated.");
            return;
        }


        if (string.IsNullOrWhiteSpace(changePasswordRequest.NewPassword) || string.IsNullOrWhiteSpace(_confirmPassword)) {
            Toast.ShowError("New password and confirmation are required.");
            return;
        }
        if (changePasswordRequest.NewPassword != _confirmPassword) {
            Toast.ShowError("New password and confirmation do not match.");
            return;
        }
        var response = await ApiClient.PostAsync<AuthResponse, AccountUpdateRequest>("/api/account/changePassword", changePasswordRequest);
        if (response != null && response.Token != null) {
            await ((CustomAuthStateProvider)AuthProvider).MarkUserAsAuthenticated(response);

            Toast.ShowSuccess("Password changed successfully.");
            changePasswordRequest.NewPassword = "";
            changePasswordRequest.CurrentPassword = "";
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
