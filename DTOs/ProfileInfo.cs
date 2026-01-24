

using b1.Models;

public class ProfileInfo
{
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public Portfolio UserPortfolio { get; set; } = null!;
}