using BC = BCrypt.Net.BCrypt;
namespace b1.Models
{
    public class UserRegistrationForm
    {
        public string Username { get; init; }
        public string FirstName { get; init; }

        public string Email { get; init; }
        public string LastName { get; init; }
        public string Password { get; init; }
        public UserRegistrationForm(string username, string email, string password, string firstName, string lastName)
        {
            Username = username;
            Email = email;
            Password = password;
            FirstName = firstName;
            LastName = lastName;
        }
        public static User CreateUserFromRegistration(UserRegistrationForm form)
        {
            return new User()
            {
                Username = form.Username,
                Email = form.Email,
                Password = BC.EnhancedHashPassword(form.Password),
                Fname = form.FirstName,
                Lname = form.LastName
            };
        }
        public override string ToString()
        {
            return $"Username = {Username} Email = {Email} Pasword = {Password} FirstName = {FirstName}, LastName = {LastName}";
        }
    }
}

//curl -v --json '{"Username":"RomanTheBaws","Password":"1233","Email":"korsunsky.roma@gmail.com","FirstName":"Roma","LastName":"Korsunsky"}' http://localhost:5008/api/v1/users/register
//curl -v --json '{"Username":"RomanTheBaws","Password":"1233"}' http://localhost:5008/api/v1/users/login
//curl -X GET http://localhost:5008/api/v1/portfolio/Boris -H "Authorization: Bearer AUTH_CODE_HERE"