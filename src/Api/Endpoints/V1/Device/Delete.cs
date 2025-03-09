using Api.Infrastructure.Context;
using Api.Infrastructure.Contract;
using Domain.Dto.Device;
using Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.V1.Device;

public class Delete : IEndpoint
{
    private static async Task<IResult> Handler(
        [FromRoute] string fcmToken,
        [FromServices] IApiContext apiContext,
        [FromServices] IUserDeviceRepository userDeviceRepository,
        CancellationToken cancellationToken
    )
    {
        await userDeviceRepository.DeleteUserDeviceTokenAsync(
            userId: apiContext.CurrentUserId,
            fcmToken: fcmToken,
            cancellationToken
        );

        return Results.Ok();
    }

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/v1/devices/{fcmToken}", Handler).WithTags("Device");
    }
}
