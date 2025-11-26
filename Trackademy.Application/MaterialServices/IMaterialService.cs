using Trackademy.Application.MaterialServices.Models;
using Trackademy.Application.Shared.Models;

namespace Trackademy.Application.MaterialServices;

public interface IMaterialService
{
    Task<MaterialDto> UploadMaterialAsync(MaterialAddModel model, Guid uploadedById);
    Task<PagedResult<MaterialDto>> GetMaterialsAsync(GetMaterialsRequest request, Guid userId, string userRole);
    Task<MaterialDto> GetByIdAsync(Guid materialId, Guid userId, string userRole);
    Task<MaterialDto> UpdateMaterialAsync(Guid materialId, MaterialUpdateModel model, Guid userId, string userRole);
    Task DeleteMaterialAsync(Guid materialId, Guid userId, string userRole);
    Task<FileDownloadResult> DownloadMaterialAsync(Guid materialId, Guid userId, string userRole);
}
