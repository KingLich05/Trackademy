using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackademy.Api.Authorization;
using Trackademy.Application.PaymentServices;
using Trackademy.Application.PaymentServices.Models;
using Trackademy.Application.Shared.Exception;
using Trackademy.Domain.Enums;

namespace Trackademy.Api.Controllers.Payment;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController(IPaymentService paymentService) : ControllerBase
{
    /// <summary>
    /// Создание платежа (только администраторы)
    /// </summary>
    [HttpPost]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> CreatePayment([FromBody] PaymentCreateModel model)
    {
        try
        {
            var paymentId = await paymentService.CreatePaymentAsync(model);
            return Ok(new { Id = paymentId });
        }
        catch (ConflictException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Получение платежа по ID
    /// </summary>
    [HttpGet("{id}")]
    [RoleAuthorization(RoleEnum.Student)]
    public async Task<IActionResult> GetPaymentById(Guid id)
    {
        var payment = await paymentService.GetPaymentByIdAsync(id);
        if (payment == null)
            return NotFound("Платеж не найден");

        // Студенты могут видеть только свои платежи
        if (User.IsInRole(RoleEnum.Student.ToString()))
        {
            // TODO: Получить ID текущего пользователя из токена
            // var currentUserId = GetCurrentUserId();
            // if (payment.StudentId != currentUserId)
            //     return Forbid();
        }

        return Ok(payment);
    }

    /// <summary>
    /// Получение платежей студента
    /// </summary>
    [HttpGet("student/{studentId}")]
    [RoleAuthorization(RoleEnum.Student)]
    public async Task<IActionResult> GetStudentPayments(Guid studentId)
    {
        // Студенты могут видеть только свои платежи
        if (User.IsInRole(RoleEnum.Student.ToString()))
        {
            // TODO: Проверить, что studentId соответствует текущему пользователю
        }

        var payments = await paymentService.GetStudentPaymentsAsync(studentId);
        return Ok(payments);
    }

    /// <summary>
    /// Получение платежей группы
    /// </summary>
    [HttpGet("group/{groupId}")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> GetGroupPayments(Guid groupId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var payments = await paymentService.GetGroupPaymentsAsync(groupId, page, pageSize);
        return Ok(payments);
    }

    /// <summary>
    /// Получение всех платежей с фильтрами
    /// </summary>
    [HttpGet]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> GetAllPayments(
        [FromQuery] PaymentStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var payments = await paymentService.GetAllPaymentsAsync(status, page, pageSize);
        return Ok(payments);
    }

    /// <summary>
    /// Пометить платеж как оплаченный
    /// </summary>
    [HttpPatch("{id}/paid")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> MarkPaymentAsPaid(Guid id, [FromBody] PaymentMarkAsPaidModel model)
    {
        try
        {
            var result = await paymentService.MarkPaymentAsPaidAsync(id, model);
            if (!result)
                return NotFound("Платеж не найден");

            return Ok();
        }
        catch (ConflictException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Отменить платеж
    /// </summary>
    [HttpPatch("{id}/cancel")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> CancelPayment(Guid id, [FromBody] PaymentCancelModel model)
    {
        try
        {
            var result = await paymentService.CancelPaymentAsync(id, model);
            if (!result)
                return NotFound("Платеж не найден");

            return Ok();
        }
        catch (ConflictException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Сделать возврат по платежу
    /// </summary>
    [HttpPatch("{id}/refund")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> RefundPayment(Guid id, [FromBody] PaymentRefundModel model)
    {
        try
        {
            var result = await paymentService.RefundPaymentAsync(id, model);
            if (!result)
                return NotFound("Платеж не найден");

            return Ok();
        }
        catch (ConflictException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Обновить просроченные платежи (для фоновой задачи)
    /// </summary>
    [HttpPost("update-overdue")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> UpdateOverduePayments()
    {
        await paymentService.UpdateOverduePaymentsAsync();
        return Ok();
    }

    /// <summary>
    /// Получить платежи для уведомлений
    /// </summary>
    [HttpGet("notifications")]
    [RoleAuthorization(RoleEnum.Administrator)]
    public async Task<IActionResult> GetPaymentsForNotification()
    {
        var payments = await paymentService.GetPaymentsForNotificationAsync();
        return Ok(payments);
    }
}