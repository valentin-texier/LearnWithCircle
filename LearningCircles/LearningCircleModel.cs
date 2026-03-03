namespace LearnWithCircle.LearningCircles;

public class LearningCircleModel
{
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public string SvgFile { get; init; } = string.Empty;

    public string ThumbnailFile
    {
        get
        {
            var dotIndex = SvgFile.LastIndexOf('.');
            if (dotIndex <= 0)
                return $"{SvgFile}_thumb";

            return $"{SvgFile[..dotIndex]}_thumb{SvgFile[dotIndex..]}";
        }
    }
}
