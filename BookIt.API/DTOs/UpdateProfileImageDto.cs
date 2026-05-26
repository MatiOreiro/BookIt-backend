using Microsoft.AspNetCore.Http;

namespace BookIt.API.DTOs;

public class UpdateProfileImageDto
{
    public IFormFile? ProfileImage { get; set; }
}