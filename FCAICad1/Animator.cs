namespace FCAICad;

public static class Animator
{
    /// <summary>1 フレームの時間を指定してアニメーション機能を提供</summary>
    /// <param name="interval">1 フレームの時間をミリ秒単位で指定</param>
    /// <param name="callback">
    /// bool callback(int frame) の形でコールバックを指定
    /// frame は 0 から 1 ずつ増加
    /// </param>
    public static void Animate(int interval, Func<int, bool> callback)
    {
        var timer = new System.Windows.Forms.Timer { Interval = interval };
        var frame = 0;
        timer.Tick += (_, _) => {
            if (!callback(frame))
                timer.Stop();
            frame++;
        };
        timer.Start();
    }
}
