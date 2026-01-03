using System.Text.Json;

namespace LearnWithCircle.LearningCircles;

public static class LearningCircleService
{
    private const string JsonFile = "learning_circles.json";

    public static async Task<List<LearningCircleModel>> LoadCirclesAsync()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(JsonFile);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var circles = await JsonSerializer.DeserializeAsync<List<LearningCircleModel>>(stream, options);

            return circles ?? new List<LearningCircleModel>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"JSON reading error: {ex.Message}");
            return new List<LearningCircleModel>();
        }
    }
}
