using System.Diagnostics;
using KGySoft.CoreLibraries;

public class Rate
{
    public float hz;
    private HiResTimer timer;

    public event Action callback;

    public Rate(float hz)
    {
        this.hz = hz;
        timer = new HiResTimer(1f / hz * 1000f);
        timer.Start();
        timer.Elapsed += (s, e) => callback?.Invoke();
    }
}