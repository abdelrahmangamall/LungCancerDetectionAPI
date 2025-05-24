using LungCancerDetectionAPI.DTO;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace CancerDetectionAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PredictionController : ControllerBase
{
    private readonly IAIDetectionService _detectionService;
    private readonly ILogger<PredictionController> _logger;

    public PredictionController(IAIDetectionService detectionService, ILogger<PredictionController> logger)
    {
        _detectionService = detectionService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(DetectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Predict([FromForm] DetectionRequest request)
    {
        try
        {
            if (request.CTScanImage == null)
            {
                return Ok("Cancer Detection API is running"); // No image = health check
            }

            _logger.LogInformation("Processing CT scan for patient {PatientId}", request.PatientId);
            var result = await _detectionService.AnalyzeCTScanAsync(request.CTScanImage);
            _logger.LogInformation("Analysis completed for patient {PatientId}. Result: {Result}", request.PatientId, result.Message);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CT scan");
            return StatusCode(500, "An error occurred while processing the CT scan");
        }
    }
}
