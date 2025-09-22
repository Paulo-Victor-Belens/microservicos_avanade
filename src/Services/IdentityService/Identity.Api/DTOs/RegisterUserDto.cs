namespace Identity.Api.DTOs
{
    public record RegisterUserDto(
        string FullName,
        string Email,
        string Password,
        string ConfirmPassword
    );
}