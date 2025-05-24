namespace Services
{
    public interface IAIDetectionService
    {
        Task<DetectionResponse> AnalyzeCTScanAsync(IFormFile imageFile);
    }
}
