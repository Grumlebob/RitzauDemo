using RitzauDemo.Core.Features.Books.Contracts;

namespace RitzauDemo.Core.Features.Books.Domain;

public class AuthorBook
{
    [Key]
    public int BookId { get; set; }

    [Required]
    [MaxLength(BookRules.AuthorSeedKeyMaxLength)]
    public required string SeedKey { get; set; }

    public Book Book { get; set; } = null!;
}
