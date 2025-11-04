using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.PaymentServices.Models;
using Trackademy.Application.Persistance;
using Trackademy.Application.Shared.Exception;
using Trackademy.Application.Shared.Extensions;
using Trackademy.Application.Shared.Models;
using Trackademy.Domain.Enums;
using Trackademy.Domain.Users;

namespace Trackademy.Application.PaymentServices;

public class PaymentService(
    TrackademyDbContext dbContext,
    IMapper mapper) : IPaymentService
{
    public async Task<Guid> CreatePaymentAsync(PaymentCreateModel model)
    {
        if (model.DueDate.Date < DateTime.UtcNow.Date)
        {
            throw new ConflictException("Срок оплаты не может быть в прошлом.");
        }

        if (model.PeriodStart >= model.PeriodEnd)
        {
            throw new ConflictException("Дата начала периода должна быть раньше даты окончания.");
        }

        var student = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == model.StudentId && u.Role == RoleEnum.Student);
        if (student == null)
        {
            throw new ConflictException("Студент не найден.");
        }

        var group = await dbContext.Groups.FirstOrDefaultAsync(g => g.Id == model.GroupId);
        if (group == null)
        {
            throw new ConflictException("Группа не найдена.");
        }

        var isStudentInGroup = await dbContext.Groups
            .Where(g => g.Id == model.GroupId)
            .SelectMany(g => g.Students)
            .AnyAsync(s => s.Id == model.StudentId);

        if (!isStudentInGroup)
        {
            throw new ConflictException("Студент не состоит в указанной группе.");
        }

        if (model.DiscountPercentage > 100)
        {
            throw new ConflictException("Скидка должна быть от 0 до 100%.");
        }

        var discountAmount = model.OriginalAmount * (model.DiscountPercentage / 100);
        var finalAmount = model.OriginalAmount - discountAmount;

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            StudentId = model.StudentId,
            GroupId = model.GroupId,
            Description = model.Description,
            Type = model.Type,
            OriginalAmount = model.OriginalAmount,
            DiscountPercentage = model.DiscountPercentage,
            Amount = finalAmount,
            DiscountReason = model.DiscountReason,
            PeriodStart = model.PeriodStart,
            PeriodEnd = model.PeriodEnd,
            DueDate = model.DueDate,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await dbContext.Payments.AddAsync(payment);
        await dbContext.SaveChangesAsync();

        return payment.Id;
    }

    public async Task<PaymentDto?> GetPaymentByIdAsync(Guid id)
    {
        var payment = await dbContext.Payments
            .Include(p => p.Student)
            .Include(p => p.Group)
            .FirstOrDefaultAsync(p => p.Id == id);

        return payment == null ? null : mapper.Map<PaymentDto>(payment);
    }

    public async Task<List<PaymentDto>> GetStudentPaymentsAsync(Guid studentId)
    {
        var payments = await dbContext.Payments
            .Include(p => p.Student)
            .Include(p => p.Group)
            .Where(p => p.StudentId == studentId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return mapper.Map<List<PaymentDto>>(payments);
    }

    public async Task<PagedResult<PaymentDto>> GetGroupPaymentsAsync(Guid groupId, int page = 1, int pageSize = 10)
    {
        var query = dbContext.Payments
            .Include(p => p.Student)
            .Include(p => p.Group)
            .Where(p => p.GroupId == groupId)
            .OrderByDescending(p => p.CreatedAt);

        return await query
            .Select(p => mapper.Map<PaymentDto>(p))
            .ToPagedResultAsync(page, pageSize);
    }

    public async Task<PagedResult<PaymentDto>> GetPaymentsWithFiltersAsync(PaymentFilterRequest request)
    {
        var query = dbContext.Payments
            .Include(p => p.Student)
            .Include(p => p.Group)
            .AsQueryable();

        // Фильтр по группе
        if (request.GroupId.HasValue)
        {
            query = query.Where(p => p.GroupId == request.GroupId.Value);
        }

        // Фильтр по статусу
        if (request.Status.HasValue)
        {
            query = query.Where(p => p.Status == request.Status.Value);
        }

        // Фильтр по типу платежа
        if (request.Type.HasValue)
        {
            query = query.Where(p => p.Type == request.Type.Value);
        }

        // Фильтр по дате создания
        if (request.FromDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= request.ToDate.Value);
        }

        query = query.OrderByDescending(p => p.CreatedAt);

        return await query
            .Select(p => mapper.Map<PaymentDto>(p))
            .ToPagedResultAsync(request.Page, request.PageSize);
    }

    public async Task<PagedResult<PaymentDto>> GetAllPaymentsAsync(PaymentStatus? status = null, int page = 1, int pageSize = 10)
    {
        var query = dbContext.Payments
            .Include(p => p.Student)
            .Include(p => p.Group)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        query = query.OrderByDescending(p => p.CreatedAt);

        return await query
            .Select(p => mapper.Map<PaymentDto>(p))
            .ToPagedResultAsync(page, pageSize);
    }

    public async Task<bool> MarkPaymentAsPaidAsync(Guid paymentId, PaymentMarkAsPaidModel model)
    {
        var payment = await dbContext.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
        if (payment == null)
            return false;

        // Проверка, что платеж можно оплатить
        if (payment.Status != PaymentStatus.Pending && payment.Status != PaymentStatus.Overdue)
        {
            throw new ConflictException("Можно оплатить только ожидающие оплаты или просроченные платежи.");
        }

        payment.Status = PaymentStatus.Paid;
        payment.PaidAt = model.PaidAt ?? DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelPaymentAsync(Guid paymentId, PaymentCancelModel model)
    {
        var payment = await dbContext.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
        if (payment == null)
            return false;

        // Проверка, что платеж можно отменить
        if (payment.Status == PaymentStatus.Paid)
        {
            throw new ConflictException("Нельзя отменить оплаченный платеж. Используйте возврат.");
        }

        if (payment.Status == PaymentStatus.Cancelled || payment.Status == PaymentStatus.Refunded)
        {
            throw new ConflictException("Платеж уже отменен или возвращен.");
        }

        payment.Status = PaymentStatus.Cancelled;
        payment.CancelledAt = DateTime.UtcNow;
        payment.CancelReason = model.CancelReason;

        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RefundPaymentAsync(Guid paymentId, PaymentRefundModel model)
    {
        var payment = await dbContext.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
        if (payment == null)
            return false;

        // Проверка, что можно сделать возврат
        if (payment.Status != PaymentStatus.Paid)
        {
            throw new ConflictException("Возврат возможен только для оплаченных платежей.");
        }

        payment.Status = PaymentStatus.Refunded;
        payment.CancelledAt = DateTime.UtcNow;
        payment.CancelReason = model.RefundReason;

        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task UpdateOverduePaymentsAsync()
    {
        var overduePayments = await dbContext.Payments
            .Where(p => p.Status == PaymentStatus.Pending && p.DueDate.Date < DateTime.UtcNow.Date)
            .ToListAsync();

        foreach (var payment in overduePayments)
        {
            payment.Status = PaymentStatus.Overdue;
        }

        if (overduePayments.Any())
        {
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<List<PaymentDto>> GetPaymentsForNotificationAsync()
    {
        var notificationDate = DateTime.UtcNow.AddDays(3).Date;

        var payments = await dbContext.Payments
            .Include(p => p.Student)
            .Include(p => p.Group)
            .Where(p => p.Status == PaymentStatus.Pending && p.DueDate.Date == notificationDate)
            .OrderBy(p => p.Student.FullName)
            .ToListAsync();

        return mapper.Map<List<PaymentDto>>(payments);
    }
}