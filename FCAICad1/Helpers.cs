using System.Diagnostics;

namespace FCAICad
{
    public static class Extensions
    {
        public static void Log(this object @this) => Debug.WriteLine(@this);
    }

    public static class EnumerableExtensions
    {
        public static void ForEach<TElement>(this IEnumerable<TElement> @this, Action<TElement> action)
        {
            foreach (var element in @this)
                action(element);
        }
    }
}
