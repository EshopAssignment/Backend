
namespace Domain.Entities.Identity;

public class UserAddress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public UserProfile Profile { get; set; } = null!;

    public string Label { get; set; } = "Home";
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public string Country { get; set; } = "";

    public bool IsDeleted { get; set; } = false;
}
