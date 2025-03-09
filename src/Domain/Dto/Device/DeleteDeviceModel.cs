using System.ComponentModel.DataAnnotations;
using Domain.Enum;

namespace Domain.Dto.Device;

public class DeleteDeviceModel
{
    [Required]
    public string FcmToken { get; set; } = default!;
}
