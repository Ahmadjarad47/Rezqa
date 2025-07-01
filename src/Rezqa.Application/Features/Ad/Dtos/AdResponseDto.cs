namespace Rezqa.Application.Features.Ad.Dtos;

public class AdResponseDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = null!;
    public AdDto? Data { get; set; }
    public List<string>? Errors { get; set; }

    public AdResponseDto(bool isSuccess, string message, AdDto? data = null, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Data = data;
        Errors = errors;
    }
} 