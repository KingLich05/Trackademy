using AutoMapper;
using Trackademy.Application.Shared.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.Submissions
{
    public class SubmissionProfile : Profile
    {
        public SubmissionProfile()
        {
            CreateMap<Submission, SubmissionResponseModel>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.FullName))
                .ForMember(dest => dest.Score, opt => opt.MapFrom(src => src.Scores.FirstOrDefault() != null ? src.Scores.FirstOrDefault()!.NumericValue : null))
                .ForMember(dest => dest.Files, opt => opt.MapFrom(src => src.Files));

            CreateMap<SubmissionFile, SubmissionFileResponseModel>()
                .ForMember(dest => dest.DownloadUrl, opt => opt.MapFrom(src => $"/api/submission/file/{src.Id}"));
        }
    }
}