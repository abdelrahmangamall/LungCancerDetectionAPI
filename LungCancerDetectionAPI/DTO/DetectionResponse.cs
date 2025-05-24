namespace LungCancerDetectionAPI.DTO
{
    public class DetectionResponse
    {
        public bool HasCancer { get; set; }
        public float Confidence { get; set; }
        public string Message { get; set; }
        public string DetailedResults { get; set; } // Could include cancer type, location, etc.
    }
}
