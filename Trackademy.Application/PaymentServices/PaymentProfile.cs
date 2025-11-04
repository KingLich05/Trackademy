using AutoMapper;
using Trackademy.Application.PaymentServices.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.PaymentServices;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.FullName))
            .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Group.Name));

        CreateMap<PaymentCreateModel, Payment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Amount, opt => opt.Ignore()) // Рассчитывается в сервисе
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.PaidAt, opt => opt.Ignore())
            .ForMember(dest => dest.CancelledAt, opt => opt.Ignore())
            .ForMember(dest => dest.CancelReason, opt => opt.Ignore())
            .ForMember(dest => dest.Student, opt => opt.Ignore())
            .ForMember(dest => dest.Group, opt => opt.Ignore());
    }
}