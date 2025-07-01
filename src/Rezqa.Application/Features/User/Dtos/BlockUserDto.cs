using System;

namespace Rezqa.Application.Features.User.Dtos;

public class BlockUserDto
{
    public string UserId { get; set; } = null!;
    public DateTime BlockUntil { get; set; }
} 