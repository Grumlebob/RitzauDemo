using RitzauDemo.Core.Features.Books.Contracts;

namespace RitzauDemo.Core.Features.Books.Domain;

public class UserBook
{
    [Key]
    public int BookId { get; set; }

    [Required]
    [MaxLength(BookRules.OwnerUserIdMaxLength)]
    public required string OwnerUserId { get; set; }

    public Book Book { get; set; } = null!;
}
