using Api.Infrastructure.Context;
using Api.Infrastructure.Contract;
using Domain.Dto.Device;
using Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.V1.Device;

public class Put : IEndpoint
{
    private static async Task<IResult> Handler(
        [FromBody] CreateOrUpdateDeviceModel createOrUpdateDevice,
        [FromServices] IApiContext apiContext,
        [FromServices] IUserDeviceRepository userDeviceRepository,
        CancellationToken cancellationToken
    )
    {
        await userDeviceRepository.SaveOrUpdateDeviceAsync(
            userId: apiContext.CurrentUserId,
            fcmToken: createOrUpdateDevice.FcmToken,
            apnToken: createOrUpdateDevice.ApnToken,
            cancellationToken
        );

        return Results.Ok();
    }

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/v1/devices", Handler).WithTags("Device");
    }
}
