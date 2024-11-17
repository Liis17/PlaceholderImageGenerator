using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Font = System.Drawing.Font;

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
                    // Параметры запроса
                    var widthParam = context.Request.Query["width"];
                    var heightParam = context.Request.Query["height"];
                    var startColorParam = context.Request.Query["startColor"];
                    var endColorParam = context.Request.Query["endColor"];
                    var angleParam = context.Request.Query["angle"];
                    var textColorParam = context.Request.Query["textColor"];

                    // Проверка ширины и высоты
                    if (string.IsNullOrEmpty(widthParam) || string.IsNullOrEmpty(heightParam) ||
                        !int.TryParse(widthParam, out int width) || !int.TryParse(heightParam, out int height) ||
                        width <= 0 || height <= 0 || width > 5000 || height > 5000)
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("Invalid resolution. Width and height must be positive integers not exceeding 5000.");
                        return;
                    }

                    // Фон по умолчанию — однотонный цвет
                    Color backgroundColor = ColorTranslator.FromHtml("#212121");
                    Color textColor = ColorTranslator.FromHtml("#ececec");

                    // Проверка на использование градиента
                    bool useGradient = !string.IsNullOrEmpty(startColorParam) &&
                   !string.IsNullOrEmpty(endColorParam) &&
                   !string.IsNullOrEmpty(angleParam) &&
                   int.TryParse(angleParam, out int angle);

                    if (useGradient)
                    {
                        try
                        {
                            if (!int.TryParse(angleParam, out angle))
                            {
                                throw new ArgumentException("Invalid angle value");
                            }

                            var startColor = ColorTranslator.FromHtml(startColorParam);
                            var endColor = ColorTranslator.FromHtml(endColorParam);
                            textColor = ColorTranslator.FromHtml(textColorParam);

                            // Создание изображения с градиентом
                            using var image = new Bitmap(width, height);
                            using var graphics = Graphics.FromImage(image);

                            using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                                new Rectangle(0, 0, width, height),
                                startColor,
                                endColor,
                                angle
                            );
                            graphics.FillRectangle(brush, 0, 0, width, height);

                            // Добавление текста
                            AddText(graphics, width, height, textColor);

                            // Отправка изображения
                            context.Response.ContentType = "image/png";
                            using var ms = new MemoryStream();
                            image.Save(ms, ImageFormat.Png);
                            ms.Seek(0, SeekOrigin.Begin);
                            await ms.CopyToAsync(context.Response.Body);
                        }
                        catch (Exception ex)
                        {
                            context.Response.StatusCode = 400;
                            await context.Response.WriteAsync($"Error generating gradient: {ex.Message}");
                        }
                        return;
                    }
                

                    // Однотонный фон 
                    using var solidImage = new Bitmap(width, height);
                    using var solidGraphics = Graphics.FromImage(solidImage);
                    solidGraphics.Clear(backgroundColor);

                    // Добавление текста
                    AddText(solidGraphics, width, height, textColor);

                    // Отправка изображения
                    context.Response.ContentType = "image/png";
                    using var msSolid = new MemoryStream();
                    solidImage.Save(msSolid, ImageFormat.Png);
                    msSolid.Seek(0, SeekOrigin.Begin);
                    await msSolid.CopyToAsync(context.Response.Body);
                });
            });
        }

        private static void AddText(Graphics graphics, int width, int height, Color textColor)
        {
            var text = $"{width}x{height}";
            var fontSize = (int)(0.09 * Math.Min(width, height));
            using var font = new Font("Century Gothic", fontSize);
            using var brush = new SolidBrush(textColor);

            var textSize = graphics.MeasureString(text, font);
            var textPosition = new PointF((width - textSize.Width) / 2, (height - textSize.Height) / 2);
            graphics.DrawString(text, font, brush, textPosition);
        }
    }
}
