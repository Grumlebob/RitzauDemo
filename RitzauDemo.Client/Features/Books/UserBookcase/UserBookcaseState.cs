using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using RitzauDemo.Client.Features.Accounts;
using RitzauDemo.Core.Features.Books.Contracts;
using RitzauDemo.Core.Features.Books.UseCases.GetBooks;

namespace RitzauDemo.Client.Features.Books.UserBookcase;

public sealed class UserBookcaseState(
    IBooksApi books,
    AuthenticationStateProvider authenticationStateProvider,
    ILogger<UserBookcaseState> logger)
{
    private readonly IBooksApi _books = books;
    private readonly AuthenticationStateProvider _authenticationStateProvider = authenticationStateProvider;
    private readonly ILogger<UserBookcaseState> _logger = logger;
    private string? _currentUserId;
    private int _version;

    public event Action? Changed;

    public bool IsLoading { get; private set; } = true;

    public string? Error { get; private set; }

    public IReadOnlyList<BookListItemResponse> Books { get; private set; } = [];

    public async Task LoadForCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var version = Interlocked.Increment(ref _version);
        IsLoading = true;
        Error = null;
        NotifyChanged();

        var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        if (version != Volatile.Read(ref _version))
        {
            return;
        }

        var userId = authenticationState.User.GetStableUserId();
        SetCurrentUser(userId);
        if (string.IsNullOrWhiteSpace(userId))
        {
            IsLoading = false;
            NotifyChanged();
            return;
        }

        try
        {
            var response = await _books.GetAsync(cancellationToken);
            var latestAuthenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            if (version != Volatile.Read(ref _version))
            {
                return;
            }

            var latestUserId = latestAuthenticationState.User.GetStableUserId();
            if (!string.Equals(userId, latestUserId, StringComparison.Ordinal))
            {
                ResetForIdentity(latestAuthenticationState.User);
                return;
            }

            ApplyLoadResult(version, response.Books);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Component disposal cancels this request; keep expected cancellation out of the error state.
        }
        catch (Exception ex)
        {
            if (version != Volatile.Read(ref _version))
            {
                return;
            }

            _logger.LogWarning(ex, "User books load failed for user {UserId}", userId);
            Error = "Your books could not be loaded.";
            IsLoading = false;
            NotifyChanged();
        }
    }

    public void ResetForIdentity(ClaimsPrincipal user)
    {
        var userId = user.GetStableUserId();
        if (string.Equals(userId, _currentUserId, StringComparison.Ordinal))
        {
            return;
        }

        Interlocked.Increment(ref _version);
        _currentUserId = userId;
        Books = [];
        Error = null;
        IsLoading = false;
        NotifyChanged();
    }

    private void ApplyLoadResult(int version, IReadOnlyList<BookListItemResponse> books)
    {
        if (version != Volatile.Read(ref _version))
        {
            return;
        }

        Books = books.OrderBy(book => book.Id).ToList();
        Error = null;
        IsLoading = false;
        NotifyChanged();
    }

    private void SetCurrentUser(string? userId)
    {
        if (string.Equals(userId, _currentUserId, StringComparison.Ordinal))
        {
            return;
        }

        _currentUserId = userId;
        Books = [];
    }

    private void NotifyChanged() => Changed?.Invoke();

}
