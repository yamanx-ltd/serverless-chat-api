using System.ComponentModel.DataAnnotations;
using Domain.Enum;

namespace Domain.Dto.Device;

public class CreateOrUpdateDeviceModel
{
    [Required]
    public string FcmToken { get; set; } = default!;

    public string? ApnToken { get; set; }
}
