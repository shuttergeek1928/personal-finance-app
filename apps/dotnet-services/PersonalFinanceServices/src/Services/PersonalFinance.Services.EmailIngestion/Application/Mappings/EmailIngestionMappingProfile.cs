using AutoMapper;

using PersonalFinance.Services.EmailIngestion.Application.DataTransferObjects;
using PersonalFinance.Services.EmailIngestion.Domain.Entities;

namespace PersonalFinance.Services.EmailIngestion.Application.Mappings
{
    public class EmailIngestionMappingProfile : Profile
    {
        public EmailIngestionMappingProfile()
        {
            CreateMap<ParsedTransaction, ParsedTransactionDto>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.EmailSubject, opt => opt.MapFrom(s => s.ProcessedEmail != null ? s.ProcessedEmail.Subject : null))
                .ForMember(d => d.EmailSender, opt => opt.MapFrom(s => s.ProcessedEmail != null ? s.ProcessedEmail.SenderEmail : null))
                .ForMember(d => d.EmailDate, opt => opt.MapFrom(s => s.ProcessedEmail != null ? s.ProcessedEmail.EmailDate : (DateTime?)null));
        }
    }
}
