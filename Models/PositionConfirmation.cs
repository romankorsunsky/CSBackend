namespace b1.Models
{
    public class PositionConfirmation
    {
        public string VerificationId { get; set; } = null!;
        public bool Confirmed { get; set; }

        public PositionConfirmation(string verificationId, bool confirmed)
        {
            VerificationId = verificationId;
            Confirmed = confirmed;
        }
    }
}