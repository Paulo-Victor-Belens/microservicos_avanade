namespace Stock.Api.DTOs
{
    public class StockValidationResultDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
