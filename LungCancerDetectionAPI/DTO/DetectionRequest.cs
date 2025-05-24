namespace LungCancerDetectionAPI.DTO
{
    public class DetectionRequest
    {
        public IFormFile CTScanImage { get; set; }
        public string PatientId { get; set; } // Optional
    }
}
