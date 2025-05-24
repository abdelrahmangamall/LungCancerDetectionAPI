
using Keras;
using Keras.Models;
using Keras.PreProcessing.Image;
using LungCancerDetectionAPI.DTO;
using Microsoft.Extensions.Options;
using Models;
using Numpy;
using Services;
using System.Drawing;
using System.Drawing.Imaging;

namespace Services;

public class AIDetectionService : IAIDetectionService
{
    private readonly AIModelSettings _settings;
    private readonly ILogger<AIDetectionService> _logger;
    private BaseModel? _model;

    public AIDetectionService(
        IOptions<AppSettings> settings,
        ILogger<AIDetectionService> logger)
    {
        _settings = settings.Value.AIModelSettings;
        _logger = logger;

        // Ensure temp directory exists
        if (!Directory.Exists(_settings.TempImageStoragePath))
        {
            Directory.CreateDirectory(_settings.TempImageStoragePath);
        }

        // Load the model when the service starts
        LoadModel();
    }

    private void LoadModel()
    {
        try
        {
            _logger.LogInformation("Loading Keras model from {ModelPath}", _settings.ModelPath);
            _model = Sequential.LoadModel(_settings.ModelPath);
            _logger.LogInformation("Keras model loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load Keras model");
            throw;
        }
    }


    public async Task<DetectionResponse> AnalyzeCTScanAsync(IFormFile imageFile)
    {
        // Validate file
        ValidateImageFile(imageFile);

        // Save to temp location
        var tempFilePath = await SaveToTempLocation(imageFile);

        try
        {
            // Preprocess and predict
            var result = await PredictWithModel(tempFilePath);

            return new DetectionResponse
            {
                HasCancer = result.HasCancer,
                Confidence = result.Confidence,
                Message = result.Message,
                DetailedResults = result.DetailedResults
            };
        }
        finally
        {
            // Clean up temp file
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    private void ValidateImageFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("No file uploaded");
        }

        // Check file size
        var maxBytes = _settings.MaxFileSizeMB * 1024 * 1024;
        if (file.Length > maxBytes)
        {
            throw new ArgumentException($"File size exceeds {_settings.MaxFileSizeMB}MB limit");
        }

        // Check extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !_settings.AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"Invalid file type. Allowed types: {string.Join(", ", _settings.AllowedExtensions)}");
        }
    }

    private async Task<string> SaveToTempLocation(IFormFile file)
    {
        var tempFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var tempFilePath = Path.Combine(_settings.TempImageStoragePath, tempFileName);

        using (var stream = new FileStream(tempFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return tempFilePath;
    }

    private async Task<DetectionResponse> PredictWithModel(string imagePath)
    {
        try
        {
            // Preprocess the image to match your model's expected input
            var processedImage = PreprocessImage(imagePath);

            // Make prediction
            var predictions = _model.Predict(processedImage);
            var prediction = predictions[0].GetData<float>()[0]; // Assuming single output

            // Interpret results (adjust based on your model's output)
            var hasCancer = prediction > 0.5f; // Assuming binary classification
            var confidence = hasCancer ? prediction : 1 - prediction;

            return new DetectionResponse
            {
                HasCancer = hasCancer,
                Confidence = confidence,
                Message = hasCancer ? "Potential cancer detected" : "No signs of cancer detected",
                DetailedResults = hasCancer
                    ? $"Malignancy confidence: {confidence:P2}"
                    : $"Normal tissue confidence: {confidence:P2}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during prediction");
            throw;
        }
    }

    private NDarray PreprocessImage(string imagePath)
    {
        // Load image
        var img = ImageUtil.LoadImg(imagePath, target_size: new Shape(224, 224)); // Adjust size to your model's input

        // Convert to array and normalize
        var array = ImageUtil.ImageToArray(img);
        array = array / 255.0f; // Normalize to [0,1]

        // Add batch dimension
        array = np.expand_dims(array, axis: 0);

        return array;
    }
}