namespace PapeleriaDB.Models
{
    public record UserAuthenticationChangedMessage(bool IsAuthenticated);
    public record NavigationRequestedMessage(string Target);
}
