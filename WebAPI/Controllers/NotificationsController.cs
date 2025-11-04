using Application.Core;
using Application.Notifications.Commands;
using Application.Notifications.DTOs;
using Application.Notifications.Queries;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;

namespace WebAPI.Controllers;

/// <summary>
/// Controller for managing user notifications.
/// </summary>
public class NotificationsController(IMapper mapper) : BaseApiController
{
    /// <summary>
    /// Get all notifications for the authenticated user.
    /// </summary>
    /// <param name="unreadOnly">Optional filter to only return unread notifications.</param>
    /// <returns>List of notifications.</returns>
    [Authorize(Roles = "User")]
    [HttpGet]
    public async Task<ActionResult<List<ResNotificationDto>>> GetNotifications([FromQuery] bool? unreadOnly)
    {
        var result = await Mediator.Send(new GetUserNotifications.Query { UnreadOnly = unreadOnly });

        if (!result.IsSuccess)
            return HandleResult(result);

        var dtos = mapper.Map<List<ResNotificationDto>>(result.Value);

        return HandleResult(Result<List<ResNotificationDto>>.Success(dtos, 200));
    }

    /// <summary>
    /// Get only unread notifications for the authenticated user.
    /// </summary>
    /// <returns>List of unread notifications.</returns>
    [Authorize(Roles = "User")]
    [HttpGet("unread")]
    public async Task<ActionResult<List<ResNotificationDto>>> GetUnreadNotifications()
    {
        var result = await Mediator.Send(new GetUserNotifications.Query { UnreadOnly = true });

        if (!result.IsSuccess)
            return HandleResult(result);

        var dtos = mapper.Map<List<ResNotificationDto>>(result.Value);

        return HandleResult(Result<List<ResNotificationDto>>.Success(dtos, 200));
    }

    /// <summary>
    /// Mark a notification as read.
    /// </summary>
    /// <param name="id">ID of the notification to mark as read.</param>
    /// <returns>No content on success.</returns>
    [Authorize(Roles = "User")]
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(string id)
    {
        return HandleResult(await Mediator.Send(new MarkNotificationAsRead.Command { NotificationId = id }));
    }

    /// <summary>
    /// Delete a notification.
    /// </summary>
    /// <param name="id">ID of the notification to delete.</param>
    /// <returns>No content on success.</returns>
    [Authorize(Roles = "User")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(string id)
    {
        return HandleResult(await Mediator.Send(new DeleteNotification.Command { NotificationId = id }));
    }
}