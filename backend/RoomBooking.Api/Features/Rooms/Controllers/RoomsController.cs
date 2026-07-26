using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomBooking.Api.Features.Rooms.Services;
using RoomBooking.Api.Shared.Http;

namespace RoomBooking.Api.Features.Rooms.Controllers;

[ApiController]
[Authorize]
[Route("rooms")]
public sealed class RoomsController(IRoomService roomService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RoomDto>>>> List(CancellationToken cancellationToken)
    {
        var rooms = await roomService.ListAsync(cancellationToken);
        return Ok(ApiResponse<List<RoomDto>>.Ok(rooms));
    }

    /// <summary>
    /// Returns occupied/free slots for a half-open Montevideo calendar-date range.
    /// </summary>
    [HttpGet("{code}/schedule")]
    public async Task<IActionResult> Schedule(
        string code,
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDateExclusive,
        CancellationToken cancellationToken)
    {
        var (result, error) = await roomService.GetScheduleAsync(
            code, fromDate, toDateExclusive, cancellationToken);

        if (result is null)
        {
            var fail = ApiResponse<ScheduleResponse>.Fail(error ?? "Invalid request");
            if (error is not null && error.Contains("does not exist", StringComparison.Ordinal))
                return NotFound(fail);
            return BadRequest(fail);
        }

        return Ok(ApiResponse<ScheduleResponse>.Ok(result));
    }
}
