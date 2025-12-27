
namespace b1.Models
{
   public class UserRegistrationForm
    {
        private string _email;
        private string _firstName;
        private string _lastName;

        private string _password;
        public UserRegistrationForm(string email,string password, string firstName, string lastName)
        {
            _email = email;
            _password = password;
            _firstName = firstName;
            _lastName = lastName;
        }
        public string Email
        {
            get { return _email; }
            init
            {
                _email = value;
            }
        }
        public string Password
        {
            get { return _password; }
            init
            {
                _password = value;
            }
        }

        public string FirstName
        {
            get{ return _firstName; }
            init { _firstName = value; }
                     
        }
        public string LastName
        {
            get { return _lastName; }
            init
            {
                _lastName = value;
            }
        }
    } 
}
