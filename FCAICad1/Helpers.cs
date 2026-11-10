using System.Diagnostics;

namespace FCAICad;

// C# 14 の拡張ブロック (extension) による拡張メンバー
public static class Extensions
{
    extension(object value)
    {
        public void Log() => Debug.WriteLine(value);
    }
}

public static class EnumerableExtensions
{
    extension<TElement>(IEnumerable<TElement> source)
    {
        public void ForEach(Action<TElement> action)
        {
            foreach (var element in source)
                action(element);
        }
    }
}
