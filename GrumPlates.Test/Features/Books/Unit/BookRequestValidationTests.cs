using System.ComponentModel.DataAnnotations;
using GrumPlates.Core.Features.Books.Contracts;
using GrumPlates.Core.Features.Books.UseCases.CreateBook;
using GrumPlates.Core.Features.Books.UseCases.UpdateBook;

namespace GrumPlates.Test.Features.Books.Unit;

public sealed class BookRequestValidationTests
{
    [Fact]
    public void CreateRequest_AcceptsAValidBookWithOptionalUrl()
    {
        var request = new CreateBookRequest { Title = "Demo book", Author = "Demo author", Url = null };

        Assert.Empty(Validate(request));
    }

    [Theory]
    [InlineData("relative/path")]
    [InlineData("ftp://example.test/book")]
    public void CreateRequest_RejectsNonHttpUrls(string url)
    {
        var request = new CreateBookRequest { Title = "Demo book", Url = url };

        var error = Assert.Single(Validate(request));
        Assert.Equal(nameof(CreateBookRequest.Url), Assert.Single(error.MemberNames));
    }

    [Fact]
    public void CreateRequest_RequiresTitle()
    {
        var request = new CreateBookRequest { Title = string.Empty };

        var error = Assert.Single(Validate(request));
        Assert.Equal(nameof(CreateBookRequest.Title), Assert.Single(error.MemberNames));
    }

    [Fact]
    public void UpdateRequest_EnforcesTheCoreTitleLimit()
    {
        var request = new UpdateBookRequest
        {
            Id = 1,
            Title = new string('x', BookRules.TitleMaxLength + 1)
        };

        var error = Assert.Single(Validate(request));
        Assert.Equal(nameof(UpdateBookRequest.Title), Assert.Single(error.MemberNames));
    }

    private static IReadOnlyList<ValidationResult> Validate(object request)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);
        return results;
    }
}
