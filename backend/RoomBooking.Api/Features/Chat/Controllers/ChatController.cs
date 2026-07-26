using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomBooking.Api.Features.Chat.Services;
using RoomBooking.Api.Shared.Http;
using RoomBooking.Api.Shared.Security;

namespace RoomBooking.Api.Features.Chat.Controllers;

[ApiController]
[Authorize]
[Route("chat")]
public sealed class ChatController(IChatService chatService, ILogger<ChatController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        if (!CurrentUser.TryGetUsername(User, out var username))
            return Unauthorized(ApiResponse<ChatResponse>.Fail("Not authenticated"));

        try
        {
            var reply = await chatService.ChatAsync(request.Messages, username, cancellationToken);
            return Ok(ApiResponse<ChatResponse>.Ok(new ChatResponse(reply)));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return StatusCode(499, ApiResponse<ChatResponse>.Fail("Request cancelled"));
        }
        catch (LlmException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<ChatResponse>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Chat failed for {User}", username);
            return StatusCode(502, ApiResponse<ChatResponse>.Fail("Chat error"));
        }
    }
}
