var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services
	.AddHttp304() // 添加 Http304 和 HttpConnectionInfo 服务
	.AddMemoryCache()
	.AddResponseCaching()
	.AddCors(options => options.AddDefaultPolicy(policy => {
		_ = policy.AllowAnyHeader();
		_ = policy.AllowAnyMethod();
		_ = policy.AllowCredentials();
		_ = policy.SetPreflightMaxAge(TimeSpan.FromDays(1));
		_ = policy.WithOrigins(/* Please enter your domain. */);
	})).AddHttpsRedirection(options => {
		options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
		options.HttpsPort = 443;
	}).AddHsts(options => {
		options.ExcludedHosts.Add("localhost");
		options.IncludeSubDomains = true;
		options.MaxAge = TimeSpan.FromDays(365);
		options.Preload = true;
	}).AddResponseCompression(options => {
		options.EnableForHttps = true;
		options.ExcludedMimeTypes = new[] { "application/json" }; // 这压缩不是浪费性能吗？没起太大作用
	}).AddControllers(options => {
		options.CacheProfiles.Add("Private30d", new() { Duration = 2592000, Location = ResponseCacheLocation.Client });
		options.CacheProfiles.Add("Public30d", new() { Duration = 2592000, Location = ResponseCacheLocation.Any });
		options.CacheProfiles.Add("Private1d", new() { Duration = 86400, Location = ResponseCacheLocation.Client });
		options.CacheProfiles.Add("Public1d", new() { Duration = 86400, Location = ResponseCacheLocation.Any });
		options.CacheProfiles.Add("Private1h", new() { Duration = 3600, Location = ResponseCacheLocation.Client });
		options.CacheProfiles.Add("Public1h", new() { Duration = 3600, Location = ResponseCacheLocation.Any });
		options.CacheProfiles.Add("Private10m", new() { Duration = 600, Location = ResponseCacheLocation.Client });
		options.CacheProfiles.Add("Private5m", new() { Duration = 300, Location = ResponseCacheLocation.Client });
		options.CacheProfiles.Add("Private1m", new() { Duration = 60, Location = ResponseCacheLocation.Client });
		options.CacheProfiles.Add("NoCache", new() { Duration = 0, Location = ResponseCacheLocation.None });
		options.CacheProfiles.Add("NoStore", new() { NoStore = true });
	});

var app = builder.Build();


if (!app.Environment.IsDevelopment()) {
	_ = app.UseExceptionHandler("/Error")
		.UseStatusCodePagesWithReExecute("/StatusCode/{0}")
		.UseHttpsRedirection()
		.UseHsts();
}

app.UseResponseCompression()
	.UseCors()
	.UseResponseCaching()
	.UseAddResponseHeaders(new HeaderDictionary {
		{ "Expect-CT", "max-age=31536000; enforce" },
		{ "Content-Security-Policy", "upgrade-insecure-requests; default-src 'self' 'unsafe-inline'; img-src 'self' https://*.bing.com/th; frame-ancestors 'self'" },
		{ "X-XSS-Protection", "1; mode=block" },
		{ "X-Content-Type-Options", "nosniff" }
	}).UseStaticFiles(new StaticFileOptions {
		OnPrepareResponse = context => context.Context.Response.Headers.CacheControl = app.Environment.IsDevelopment() ? "private,max-age=10" : "public,max-age=1209600" // 14天
	});

app.MapRazorPages();

app.MapControllers();

app.Run();