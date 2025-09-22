namespace Identity.Api.DTOs
{
    public record LoginUserDto(
        string Email,
        string Password
    );
}