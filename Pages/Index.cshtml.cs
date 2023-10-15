namespace Execute233Website.Pages;
public class IndexModel : PageModel {
	private readonly ILogger<IndexModel> _logger;

	public IndexModel(ILogger<IndexModel> logger) {
		_logger = logger;
	}

	public void OnGet() {
		if ((!Request.Path.Value?.EndsWith('/')) ?? true) {
			_logger.LogDebug("Model 308 重定向，From {}", Request.Path);
			Response.Redirect("/", true, true);
		}

		ViewData["Title"] = "首页";
		ViewData["Keywords"] = "execute233,龙川县第一中学";
		ViewData["Description"] = "execute233 的个人主页。";
	}
}
