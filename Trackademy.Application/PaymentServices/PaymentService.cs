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
        if (model.PeriodEnd < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ConflictException("Период окончания не может быть в прошлом.");
        }

        if (model.PeriodStart >= model.PeriodEnd)
        {
            throw new ConflictException("Дата начала периода должна быть раньше даты окончания.");
        }

        var student = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == model.StudentId && u.Role == RoleEnum.Student);
        
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
            PaymentPeriod = model.PaymentPeriod,
            Type = model.Type,
            OriginalAmount = model.OriginalAmount,
            DiscountPercentage = model.DiscountPercentage,
            Amount = finalAmount,
            DiscountReason = model.DiscountReason,
            PeriodStart = model.PeriodStart,
            PeriodEnd = model.PeriodEnd,
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
            .Where(x => x.Group.OrganizationId == request.OrganizationId)
            .AsQueryable();

        if (request.GroupId.HasValue)
        {
            query = query.Where(p => p.GroupId == request.GroupId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(p => p.Status == request.Status.Value);
        }

        if (request.Type.HasValue)
        {
            query = query.Where(p => p.Type == request.Type.Value);
        }

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

    public async Task<GroupedPaymentResult> GetGroupedPaymentsAsync(PaymentFilterRequest request)
    {
        var pagedPayments = await GetPaymentsWithFiltersAsync(request);
        
        var groupedPayments = pagedPayments.Items
            .GroupBy(p => new { p.StudentId, p.StudentName })
            .Select(g => {
                var sortedPayments = g.OrderByDescending(p => p.CreatedAt).ToList();
                var lastPayment = sortedPayments.First();
                
                return new GroupedPaymentDto
                {
                    StudentId = g.Key.StudentId,
                    StudentName = g.Key.StudentName,
                    LastPaymentId = lastPayment.Id,
                    LastPaymentAmount = lastPayment.Amount,
                    LastPaymentStatus = lastPayment.Status,
                    LastPaymentStatusName = lastPayment.StatusName,
                    LastPaymentType = lastPayment.Type,
                    LastPaymentTypeName = lastPayment.TypeName,
                    LastPaymentPeriod = lastPayment.PaymentPeriod,
                    LastPaymentCreatedAt = lastPayment.CreatedAt,
                    LastPaymentPaidAt = lastPayment.PaidAt,
                    LastPaymentPeriodStart = lastPayment.PeriodStart,
                    LastPaymentPeriodEnd = lastPayment.PeriodEnd,
                    LastPaymentDiscountReason = lastPayment.DiscountReason,
                    LastPaymentOriginalAmount = lastPayment.OriginalAmount,
                    LastPaymentDiscountPercentage = lastPayment.DiscountPercentage,
                    Payments = sortedPayments
                };
            })
            .OrderBy(s => s.StudentName)
            .ToList();

        return new GroupedPaymentResult
        {
            Items = groupedPayments,
            TotalCount = pagedPayments.TotalCount,
            PageNumber = pagedPayments.PageNumber,
            PageSize = pagedPayments.PageSize
        };
    }

    public async Task<bool> MarkPaymentAsPaidAsync(Guid paymentId, PaymentMarkAsPaidModel model)
    {
        var payment = await dbContext.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
        if (payment == null)
            return false;

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
        {
            return false;
        }

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
            .Where(p => p.Status == PaymentStatus.Pending && p.PeriodEnd < DateOnly.FromDateTime(DateTime.UtcNow))
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
        var notificationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3));

        var payments = await dbContext.Payments
            .Include(p => p.Student)
            .Include(p => p.Group)
            .Where(p => p.Status == PaymentStatus.Pending && p.PeriodEnd == notificationDate)
            .OrderBy(p => p.Student.FullName)
            .ToListAsync();

        return mapper.Map<List<PaymentDto>>(payments);
    }

    public async Task<PaymentStatsDto> GetPaymentStatsAsync(
        Guid organizationId,
        Guid? groupId = null,
        Guid? studentId = null)
    {
        var query = dbContext.Payments
            .Include(x => x.Group)
            .Where(x => x.Group.OrganizationId == organizationId).AsQueryable();

        if (groupId.HasValue)
        {
            query = query.Where(p => p.GroupId == groupId.Value);
        }

        if (studentId.HasValue)
        {
            query = query.Where(p => p.StudentId == studentId.Value);
        }

        var payments = await query.ToListAsync();

        return new PaymentStatsDto
        {
            TotalPayments = payments.Count,
            PendingPayments = payments.Count(p => p.Status == PaymentStatus.Pending),
            PaidPayments = payments.Count(p => p.Status == PaymentStatus.Paid),
            OverduePayments = payments.Count(p => p.Status == PaymentStatus.Overdue),
            CancelledPayments = payments.Count(p => p.Status == PaymentStatus.Cancelled),
            RefundedPayments = payments.Count(p => p.Status == PaymentStatus.Refunded),
            TotalAmount = payments.Sum(p => p.Amount),
            PaidAmount = payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.Amount),
            PendingAmount = payments.Where(p => p.Status == PaymentStatus.Pending).Sum(p => p.Amount),
            OverdueAmount = payments.Where(p => p.Status == PaymentStatus.Overdue).Sum(p => p.Amount)
        };
    }

    public async Task CreatePaymentForStudentAsync(Guid studentId, Guid groupId, decimal discountPercentage = 0, string? discountReason = null)
    {
        var group = await dbContext.Groups
            .Include(g => g.Subject)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            throw new ConflictException("Группа не найдена.");

        var student = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == studentId && u.Role == RoleEnum.Student);

        if (student == null)
            throw new ConflictException("Студент не найден.");

        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ConflictException("Скидка должна быть от 0 до 100%.");

        var now = DateTime.UtcNow;
        var periodStart = DateOnly.FromDateTime(now);
        DateOnly periodEnd;
        string paymentPeriod;

        var subjectPrice = group.Subject.Price;
        var paymentType = group.Subject.PaymentType;

        if (paymentType == PaymentType.Monthly)
        {
            // Для месячных платежей: 30 дней с даты добавления
            periodEnd = periodStart.AddDays(30);
            paymentPeriod = now.ToString("MMMM yyyy", new System.Globalization.CultureInfo("ru-RU"));
        }
        else // OneTime
        {
            // Для разовых платежей: до конца текущего года или +1 год
            periodEnd = periodStart.AddYears(1);
            paymentPeriod = $"Разовая оплата до {periodEnd:dd.MM.yyyy}";
        }

        var discountAmount = subjectPrice * (discountPercentage / 100);
        var finalAmount = subjectPrice - discountAmount;

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            GroupId = groupId,
            PaymentPeriod = paymentPeriod,
            Type = paymentType,
            OriginalAmount = subjectPrice,
            DiscountPercentage = discountPercentage,
            Amount = finalAmount,
            DiscountReason = discountReason,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Status = PaymentStatus.Pending,
            CreatedAt = now
        };

        await dbContext.Payments.AddAsync(payment);
        await dbContext.SaveChangesAsync();
    }

    public async Task CancelStudentPaymentsInGroupAsync(Guid studentId, Guid groupId, string cancelReason)
    {
        var paymentsToCancel = await dbContext.Payments
            .Where(p => p.StudentId == studentId 
                && p.GroupId == groupId 
                && (p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Overdue))
            .ToListAsync();

        foreach (var payment in paymentsToCancel)
        {
            payment.Status = PaymentStatus.Cancelled;
            payment.CancelledAt = DateTime.UtcNow;
            payment.CancelReason = cancelReason;
        }

        if (paymentsToCancel.Any())
        {
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task CreateMonthlyPaymentsAsync()
    {
        // Получаем все группы с типом оплаты Monthly через Subject
        var monthlyGroups = await dbContext.Groups
            .Include(g => g.Subject)
            .Include(g => g.GroupStudents)
                .ThenInclude(gs => gs.Student)
            .Where(g => g.Subject.PaymentType == PaymentType.Monthly)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var currentMonth = now.ToString("MMMM yyyy", new System.Globalization.CultureInfo("ru-RU"));

        foreach (var group in monthlyGroups)
        {
            foreach (var groupStudent in group.GroupStudents)
            {
                // Проверяем, что студент активен в группе (не удален)
                var isStudentInGroup = await dbContext.Groups
                    .Where(g => g.Id == group.Id)
                    .SelectMany(g => g.Students)
                    .AnyAsync(s => s.Id == groupStudent.StudentId);

                if (!isStudentInGroup)
                    continue;

                // Проверяем, есть ли уже платеж за текущий месяц
                var existingPayment = await dbContext.Payments
                    .AnyAsync(p => p.StudentId == groupStudent.StudentId
                        && p.GroupId == group.Id
                        && p.PaymentPeriod == currentMonth);

                if (existingPayment)
                    continue;

                // Вычисляем период на основе даты присоединения студента к группе
                var joinDate = DateOnly.FromDateTime(groupStudent.JoinedAt);
                var dayOfMonth = joinDate.Day;
                
                // Период: с текущего дня месяца на 30 дней вперед
                var periodStart = new DateOnly(now.Year, now.Month, Math.Min(dayOfMonth, DateTime.DaysInMonth(now.Year, now.Month)));
                var periodEnd = periodStart.AddDays(30);

                var subjectPrice = group.Subject.Price;
                var discountAmount = subjectPrice * (groupStudent.DiscountPercentage / 100);
                var finalAmount = subjectPrice - discountAmount;

                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    StudentId = groupStudent.StudentId,
                    GroupId = group.Id,
                    PaymentPeriod = currentMonth,
                    Type = PaymentType.Monthly,
                    OriginalAmount = subjectPrice,
                    DiscountPercentage = groupStudent.DiscountPercentage,
                    Amount = finalAmount,
                    DiscountReason = groupStudent.DiscountReason,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    Status = PaymentStatus.Pending,
                    CreatedAt = now
                };

                await dbContext.Payments.AddAsync(payment);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task CreatePendingPaymentsForUnpaidStudentsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        // Получаем все группы с месячным типом оплаты через Subject
        var groups = await dbContext.Groups
            .Include(g => g.Subject)
            .Include(g => g.GroupStudents)
                .ThenInclude(gs => gs.Student)
            .Where(g => g.Subject.PaymentType == PaymentType.Monthly)
            .ToListAsync();

        var paymentsToCreate = new List<Payment>();
        var now = DateTime.UtcNow;

        foreach (var group in groups)
        {
            // Проверяем, что у группы есть предстоящие уроки
            var hasUpcomingLessons = await dbContext.Lessons
                .AnyAsync(l => l.GroupId == group.Id && l.Date >= today);
            
            if (!hasUpcomingLessons)
                continue;

            foreach (var groupStudent in group.GroupStudents)
            {
                // Получаем последний платеж студента в этой группе
                var lastPayment = await dbContext.Payments
                    .Where(p => p.StudentId == groupStudent.StudentId && p.GroupId == group.Id)
                    .OrderByDescending(p => p.PeriodEnd)
                    .FirstOrDefaultAsync();

                // Если платежа нет вообще, пропускаем (это должно обрабатываться при добавлении в группу)
                if (lastPayment == null)
                    continue;

                // Проверяем, истек ли период оплаты
                if (lastPayment.PeriodEnd >= today)
                    continue; // Оплата еще действительна

                // Проверяем, нет ли уже созданного платежа на следующий период
                var existingNextPayment = await dbContext.Payments
                    .AnyAsync(p => p.StudentId == groupStudent.StudentId 
                        && p.GroupId == group.Id
                        && p.PeriodStart > lastPayment.PeriodEnd);

                if (existingNextPayment)
                    continue; // Платеж уже создан

                // Создаем новый платеж
                var periodStart = lastPayment.PeriodEnd.AddDays(1);
                var periodEnd = periodStart.AddDays(30);
                var paymentPeriod = DateTime.Now.ToString("MMMM yyyy", new System.Globalization.CultureInfo("ru-RU"));

                var subjectPrice = group.Subject.Price;
                var discountAmount = subjectPrice * (groupStudent.DiscountPercentage / 100);
                var finalAmount = subjectPrice - discountAmount;

                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    StudentId = groupStudent.StudentId,
                    GroupId = group.Id,
                    PaymentPeriod = paymentPeriod,
                    Type = PaymentType.Monthly,
                    OriginalAmount = subjectPrice,
                    DiscountPercentage = groupStudent.DiscountPercentage,
                    Amount = finalAmount,
                    DiscountReason = groupStudent.DiscountReason,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    Status = PaymentStatus.Pending,
                    CreatedAt = now
                };

                paymentsToCreate.Add(payment);
            }
        }

        if (paymentsToCreate.Any())
        {
            await dbContext.Payments.AddRangeAsync(paymentsToCreate);
            await dbContext.SaveChangesAsync();
        }
    }
}