namespace b1.Authentication
{
    public class TokenTriplet
    {
        public string IdToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        public string AccessToken { get; set; } = null!;
    }
}