using Pdd.ir.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Pdd.ir.Client.Authorization
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly IClientStorageService _storage;
        private ClaimsPrincipal _currentUser = new(new ClaimsPrincipal(new ClaimsIdentity()));

        public AuthStateProvider(IClientStorageService storage)
        {
            _storage = storage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = await GetCurrentUserAsync();
            return new AuthenticationState(user);
        }

        public async Task<ClaimsPrincipal> GetCurrentUserAsync()
        {
            var username = await _storage.GetLocalAsync<string>("username");
            var role = await _storage.GetLocalAsync<string>("role");

            if (string.IsNullOrEmpty(username))
                return _currentUser;

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role ?? "user")
            };

            var identity = new ClaimsIdentity(claims, "custom");
            _currentUser = new ClaimsPrincipal(identity);
            return _currentUser;
        }

        public void NotifyAuthenticationStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task LoginAsync(string username, string role)
        {
            await _storage.SetLocalAsync("username", username);
            await _storage.SetLocalAsync("role", role);
            NotifyAuthenticationStateChanged();
        }

        public async Task LogoutAsync()
        {
            await _storage.RemoveLocalAsync("username");
            await _storage.RemoveLocalAsync("role");
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged();
        }
    }
}
