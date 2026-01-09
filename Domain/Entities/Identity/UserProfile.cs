namespace Domain.Entities.Identity;

public class UserProfile
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Phone { get; set; } = "";

    public int? DefaultShippingAddressId { get; set; }
    public UserAddress? DefaultShippingAddress { get; set; }

    public ICollection<UserAddress> Addresses { get; set; } = [];
}
