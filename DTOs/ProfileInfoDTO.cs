

using b1.Models;

public class ProfileInfoDTO
{
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public double Balance { get; init; }

    public ProfileInfoDTO(string firstName, string lastName, string email, double balance)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Balance = balance;
    }
}