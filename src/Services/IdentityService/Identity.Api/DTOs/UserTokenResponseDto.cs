namespace Identity.Api.DTOs
{
    public record UserTokenResponseDto(
        bool IsSuccess,
        string? Token,
        string? ErrorMessage
    );
}