using b1.Models;

namespace b1.Repositories
{
    public interface ITokenRepository
    {
        //returns user if exists or null
        public Task<RefreshToken> GetTokenByUid();

        public Task AddNewRefreshToken(User user);
    }

    public class RefreshToken
    {
    }
}