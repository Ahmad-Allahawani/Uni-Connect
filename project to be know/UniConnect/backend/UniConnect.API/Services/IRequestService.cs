using UniConnect.API.DTOs;

namespace UniConnect.API.Services;

public interface IRequestService
{
    Task<IEnumerable<AcademicRequestDto>> GetAllRequestsAsync();
    Task<AcademicRequestDto?> GetRequestByIdAsync(int id);
    Task<AcademicRequestDto> CreateRequestAsync(int userId, CreateAcademicRequestDto dto);
    Task<bool> SubmitProposalAsync(int requestId, int userId, CreateProposalDto dto);
    Task<bool> AcceptProposalAsync(int proposalId, int requestId);
}
