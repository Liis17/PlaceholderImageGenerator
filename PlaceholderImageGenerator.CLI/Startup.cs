using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.Drawing.Imaging;

namespace PlaceholderImageGenerator.CLI
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/placeholder", async context =>
                {
                    var widthParam = context.Request.Query["width"];
                    var heightParam = context.Request.Query["height"];

                    if (string.IsNullOrEmpty(widthParam) || string.IsNullOrEmpty(heightParam) ||
                        !int.TryParse(widthParam, out int width) || !int.TryParse(heightParam, out int height) ||
                        width <= 0 || height <= 0 || width > 5000 || height > 5000)
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("Invalid resolution. Width and height must be positive integers not exceeding 5000.");
                        return;
                    }

                    using var image = new Bitmap(width, height);
                    using var graphics = Graphics.FromImage(image);
                    var grayColor = ColorTranslator.FromHtml("#303030");
                    graphics.Clear(grayColor);

                    var text = $"{width}x{height}";
                    var fontSize = (int)(0.09 * Math.Min(width, height)); // 9% 
                    using var font = new System.Drawing.Font("Arial", fontSize);
                    var textColor = ColorTranslator.FromHtml("#212121");
                    var textBrush = new SolidBrush(textColor);

                    var textSize = graphics.MeasureString(text, font);
                    var textPosition = new PointF((width - textSize.Width) / 2, (height - textSize.Height) / 2);
                    graphics.DrawString(text, font, textBrush, textPosition);


                    context.Response.ContentType = "image/png";
                    using var ms = new MemoryStream();
                    image.Save(ms, ImageFormat.Png);
                    ms.Seek(0, SeekOrigin.Begin);
                    await ms.CopyToAsync(context.Response.Body);
                });
            });
        }
    }
}
