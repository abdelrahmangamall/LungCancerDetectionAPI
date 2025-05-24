using LungCancerDetectionAPI.DTO;

namespace Services
{
    public interface IAIDetectionService
    {
        Task<DetectionResponse> AnalyzeCTScanAsync(IFormFile imageFile);
    }
}
