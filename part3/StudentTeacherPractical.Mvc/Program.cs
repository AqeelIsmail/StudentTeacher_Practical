namespace StudentTeacherPractical.Mvc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // 1.Register an HttpClient for communicating with the API.
            builder.Services.AddHttpClient("StudentTeacherApi", client =>
            {
                string baseUrl =
                    builder.Configuration["ApiSettings:BaseUrl"]
                    ?? throw new InvalidOperationException(
                        "API address is missing.");

                client.BaseAddress = new Uri(baseUrl);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    UseCookies = false
                });

            // 2. Services needed for MVC session management.
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(); //tells ASP.NET Core that our application needs session services.

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            // 3. Enable MVC session for incoming requests.
            // Enables session handling for HTTP requests.
            app.UseSession();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
