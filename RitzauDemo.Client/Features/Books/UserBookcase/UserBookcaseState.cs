using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using RitzauDemo.Core.Features.Books.Contracts;
using RitzauDemo.Core.Features.Books.UseCases.DeleteBook;
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

        var userId = GetUserKey(authenticationState.User);
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

            var latestUserId = GetUserKey(latestAuthenticationState.User);
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
        var userId = GetUserKey(user);
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

    public void ApplySavedBook(BookListItemResponse savedBook)
    {
        Interlocked.Increment(ref _version);
        Books = Books
            .Where(book => book.Id != savedBook.Id)
            .Append(savedBook)
            .OrderBy(book => book.Id)
            .ToList();
        IsLoading = false;
        Error = null;
        NotifyChanged();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var version = Interlocked.Increment(ref _version);
        var userId = _currentUserId;
        try
        {
            var success = await _books.DeleteAsync(new DeleteBookRequest { Id = id }, cancellationToken);
            if (!success)
            {
                return false;
            }

            var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            if (version == Volatile.Read(ref _version) &&
                string.Equals(userId, GetUserKey(authenticationState.User), StringComparison.Ordinal))
            {
                Books = Books.Where(book => book.Id != id).ToList();
                IsLoading = false;
                Error = null;
                NotifyChanged();
            }

            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception ex)
        {
            if (version == Volatile.Read(ref _version))
            {
                _logger.LogWarning(ex, "User book delete failed for book {BookId}", id);
                Error = "Book could not be deleted.";
                IsLoading = false;
                NotifyChanged();
            }

            return false;
        }
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

    private static string? GetUserKey(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
               user.FindFirst(ClaimTypes.Name)?.Value ??
               user.Identity.Name;
    }
}
