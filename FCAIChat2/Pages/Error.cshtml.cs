using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace FCAIChat.Pages;

// エラー ページのページモデル
// エラー ページはキャッシュしない
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
// POST リクエストでエラーになった場合も表示できるように、偽造防止トークンを検証しない
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    // リクエストの ID
    public string? RequestId { get; set; }

    // リクエスト ID があれば画面に表示する
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    // ページの表示時 (GET) に、現在のトレースの ID (なければ HttpContext の識別子) をリクエスト ID にする
    public void OnGet() => RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
}
