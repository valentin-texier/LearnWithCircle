namespace LearnWithCircle.Controls;

public class RotatableImage : Image
{
    public static readonly BindableProperty MinScaleProperty = BindableProperty.Create(
        nameof(MinScale),
        typeof(double),
        typeof(RotatableImage),
        1d);

    public static readonly BindableProperty MaxScaleProperty = BindableProperty.Create(
        nameof(MaxScale),
        typeof(double),
        typeof(RotatableImage),
        5d);

    public static readonly BindableProperty PinchSensitivityProperty = BindableProperty.Create(
        nameof(PinchSensitivity),
        typeof(double),
        typeof(RotatableImage),
        1.6d);

    public static readonly BindableProperty RotationSensitivityProperty = BindableProperty.Create(
        nameof(RotationSensitivity),
        typeof(double),
        typeof(RotatableImage),
        1.6);

    public static readonly BindableProperty PanSensitivityProperty = BindableProperty.Create(
        nameof(PanSensitivity),
        typeof(double),
        typeof(RotatableImage),
        1d);

    public static readonly BindableProperty EnableRasterizationProperty = BindableProperty.Create(
        nameof(EnableRasterization),
        typeof(bool),
        typeof(RotatableImage),
        true);

    public static readonly BindableProperty RasterizationScaleProperty = BindableProperty.Create(
        nameof(RasterizationScale),
        typeof(double),
        typeof(RotatableImage),
        2d);

    public static readonly BindableProperty GestureThrottleMsProperty = BindableProperty.Create(
        nameof(GestureThrottleMs),
        typeof(int),
        typeof(RotatableImage),
        16);

    public static readonly BindableProperty GestureSmoothingProperty = BindableProperty.Create(
        nameof(GestureSmoothing),
        typeof(double),
        typeof(RotatableImage),
        0d);

    public double MinScale
    {
        get => (double)GetValue(MinScaleProperty);
        set => SetValue(MinScaleProperty, value);
    }

    public double MaxScale
    {
        get => (double)GetValue(MaxScaleProperty);
        set => SetValue(MaxScaleProperty, value);
    }

    public double PinchSensitivity
    {
        get => (double)GetValue(PinchSensitivityProperty);
        set => SetValue(PinchSensitivityProperty, value);
    }

    public double RotationSensitivity
    {
        get => (double)GetValue(RotationSensitivityProperty);
        set => SetValue(RotationSensitivityProperty, value);
    }

    public double PanSensitivity
    {
        get => (double)GetValue(PanSensitivityProperty);
        set => SetValue(PanSensitivityProperty, value);
    }

    public bool EnableRasterization
    {
        get => (bool)GetValue(EnableRasterizationProperty);
        set => SetValue(EnableRasterizationProperty, value);
    }

    public double RasterizationScale
    {
        get => (double)GetValue(RasterizationScaleProperty);
        set => SetValue(RasterizationScaleProperty, value);
    }

    public int GestureThrottleMs
    {
        get => (int)GetValue(GestureThrottleMsProperty);
        set => SetValue(GestureThrottleMsProperty, value);
    }

    public double GestureSmoothing
    {
        get => (double)GetValue(GestureSmoothingProperty);
        set => SetValue(GestureSmoothingProperty, value);
    }
}
