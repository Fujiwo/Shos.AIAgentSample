using System.Diagnostics;

namespace FCAICad;

// C# 14 の拡張ブロック (extension) による拡張メンバー
public static class Extensions
{
    extension(object value)
    {
        // 任意のオブジェクトをデバッグ出力に書き出す
        public void Log() => Debug.WriteLine(value);
    }
}

// シーケンス (IEnumerable<T>) 用の拡張メンバー
public static class EnumerableExtensions
{
    extension<TElement>(IEnumerable<TElement> source)
    {
        // 各要素に action を実行する (List<T>.ForEach と同様の処理を任意のシーケンスで使えるようにする)
        public void ForEach(Action<TElement> action)
        {
            foreach (var element in source)
                action(element);
        }
    }
}
