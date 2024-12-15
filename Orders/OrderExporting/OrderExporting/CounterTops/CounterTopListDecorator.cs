using OrderExporting.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using System.Drawing;
using System.Drawing.Imaging;

namespace OrderExporting.CounterTops;

public class CounterTopListDecorator : IDocumentDecorator {

    public required CounterTopList CounterTopList { get; set; }

    public void Decorate(IDocumentContainer container) {

        for (int i = 0; i < CounterTopList.CounterTops.Length; i++) {

            var counterTop = CounterTopList.CounterTops[i];

            container.Page(p => {

                p.Size(PageSizes.Letter);
                p.Margin(30);


                p.Header()
                    .AlignCenter()
                    .Text(t => {

                        t.Span(CounterTopList.OrderName)
                          .Bold()
                          .FontSize(26);

                        t.EmptyLine();

                        t.Span($"Counter Top{(CounterTopList.CounterTops.Length <= 1 ? "" : $" {i + 1} out of {CounterTopList.CounterTops.Length}")}")
                          .Bold()
                          .FontSize(26);

                    });

                p.Content()
                  .PaddingTop(30)
                  .Column(col => {

                      col.Item()
                         .AlignCenter()
                         .Text(counterTop.Finish)
                         .Bold()
                         .FontSize(26);

                      col.Item()
                         .AlignCenter()
                         .Text($"{counterTop.Width.AsMillimeters():0} x {counterTop.Length.AsMillimeters():0}")
                         .Bold()
                         .FontSize(26);

                      var imgData = CreateImage(counterTop, 500, 500);
                      using var ms1 = new MemoryStream(imgData);
                      var bm = new Bitmap(ms1);
                      var trimmed = TrimBitmapWhiteSpace(bm);

                      using var ms2 = new MemoryStream();
                      trimmed.Save(ms2, ImageFormat.Png);

                      col.Item()
                         .PaddingTop(30)
                         .Image(ms2.ToArray(), ImageScaling.FitArea);

                  });

            });

        }


    }

    public static byte[] CreateImage(CounterTop counter, int imageWidth, int imageHeight) {

        var info = new SKImageInfo(imageWidth, imageHeight);
        using var surface = SKSurface.Create(info);

        using var paint = new SKPaint() {
            Color = new SKColor(0, 0, 0),
            StrokeWidth = 5,
            IsStroke = true
        };

        using var font = new SKFont() {
            Size = 64.0f
        };

        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        float centerX = imageWidth / 2.0f;
        float centerY = imageHeight / 2.0f;

        float margin = 50; // Must be at least enough to show the finished sides
        float partWidth = (float) counter.Length.AsMillimeters();
        float partHeight = (float) counter.Width.AsMillimeters();

        var scale = Math.Min( (imageWidth - 2 * margin) / partWidth, (imageHeight - 2 * margin) / partHeight );
        Console.WriteLine(scale);

        partWidth *= scale;
        partHeight *= scale;

        var rectangle = new SKRect {

            Size = new SKSize() {
                Width = partWidth,
                Height = partHeight
            },
            
            Location = new SKPoint(centerX - partWidth / 2, centerY - partHeight / 2)

        };

        canvas.DrawRect(rectangle, paint);

        float textPadding = 10;
        var textWidth = font.MeasureText("F", paint);
        var textHeight = font.Metrics.CapHeight; // Should somehow get the actual size for the specific string;

        List<SKPoint> textPoints = [];
        if (counter.FinishedLeft) {
            textPoints.Add(new SKPoint() {
                X = centerX - rectangle.Size.Width / 2 - textWidth - textPadding,
                Y = centerY + textHeight / 2
            });
        }

        if (counter.FinishedTop) {
            textPoints.Add(new SKPoint() {
                X = centerX - textWidth / 2,
                Y = centerY - rectangle.Size.Height / 2 - textPadding
            });
        }

        if (counter.FinishedBottom) {
            textPoints.Add(new SKPoint() {
                X = centerX - textWidth / 2,
                Y = centerY + rectangle.Size.Height / 2 + textHeight + textPadding
            });
        }

        if (counter.FinishedRight) {
            textPoints.Add(new SKPoint() {
                X = centerX + rectangle.Size.Width / 2 + textPadding,
                Y = centerY + textHeight / 2
            });
        }

        paint.IsStroke = false;
        foreach (var point in textPoints) {
            canvas.DrawText("F",
                            point,
                            font,
                            paint);
        }

        using var snapshot = surface.Snapshot();
        using var data = snapshot.Encode(SKEncodedImageFormat.Png, 80);
        return data.ToArray();

    }

    private Bitmap TrimBitmapWhiteSpace(Bitmap bitmap) {
        int w = bitmap.Width;
        int h = bitmap.Height;

        bool IsAllWhiteRow(int row) {
            for (int i = 0; i < w; i++) {
                if (bitmap.GetPixel(i, row).R != 255) {
                    return false;
                }
            }
            return true;
        }

        bool IsAllWhiteColumn(int col) {
            for (int i = 0; i < h; i++) {
                if (bitmap.GetPixel(col, i).R != 255) {
                    return false;
                }
            }
            return true;
        }

        int leftMost = 0;
        for (int col = 0; col < w; col++) {
            if (IsAllWhiteColumn(col)) leftMost = col + 1;
            else break;
        }

        int rightMost = w - 1;
        for (int col = rightMost; col > 0; col--) {
            if (IsAllWhiteColumn(col)) rightMost = col - 1;
            else break;
        }

        int topMost = 0;
        for (int row = 0; row < h; row++) {
            if (IsAllWhiteRow(row)) topMost = row + 1;
            else break;
        }

        int bottomMost = h - 1;
        for (int row = bottomMost; row > 0; row--) {
            if (IsAllWhiteRow(row)) bottomMost = row - 1;
            else break;
        }

        if (rightMost == 0 && bottomMost == 0 && leftMost == w && topMost == h) {
            return bitmap;
        }

        int croppedWidth = rightMost - leftMost + 1;
        int croppedHeight = bottomMost - topMost + 1;

        try {
            Bitmap target = new Bitmap(croppedWidth, croppedHeight);
            using (Graphics g = Graphics.FromImage(target)) {
                g.DrawImage(bitmap,
                    new RectangleF(0, 0, croppedWidth, croppedHeight),
                    new RectangleF(leftMost, topMost, croppedWidth, croppedHeight),
                    GraphicsUnit.Pixel);
            }
            return target;
        } catch (Exception ex) {
            throw new Exception(string.Format("Values are top={0} bottom={1} left={2} right={3}", topMost, bottomMost, leftMost, rightMost), ex);
        }
    }

}
