using AutoMapper;
using Lykke.Service.EncryptedFileStorage.Client.Models.Requests;
using Lykke.Service.EncryptedFileStorage.Client.Models.Responses;
using Lykke.Service.EncryptedFileStorage.Domain.Models;

namespace Lykke.Service.EncryptedFileStorage.Profiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<CreateFileInfoRequest, EncryptedFile>(MemberList.Destination)
                .ForMember(dest => dest.FileId, opt => opt.Ignore())
                .ForMember(dest => dest.BlobName, opt => opt.Ignore())
                .ForMember(dest => dest.Content, opt => opt.Ignore())
                .ForMember(dest => dest.FileDate, opt => opt.Ignore())
                .ForMember(dest => dest.Length, opt => opt.Ignore());

            CreateMap<EncryptedFile, FileInfoResponse>(MemberList.Destination);

            CreateMap<PaginatedEncryptedFiles, PaginatedFilesInfoResponse>(MemberList.Destination);
        }
    }
}
