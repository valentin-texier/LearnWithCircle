namespace LearnWithCircle.Controls;

public class ThreeFingerImage : Image
{
    public event EventHandler? ThreeFingerTapped;

    internal void RaiseThreeFingerTapped()
    {
        ThreeFingerTapped?.Invoke(this, EventArgs.Empty);
    }
}
