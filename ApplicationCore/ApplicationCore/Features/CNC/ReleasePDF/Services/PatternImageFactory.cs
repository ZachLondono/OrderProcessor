using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ApplicationCore.Features.CNC.GCode.Domain;
using ApplicationCore.Features.CNC.ReleasePDF.PDFModels;
using Image = System.Drawing.Image;

namespace ApplicationCore.Features.CNC.ReleasePDF.Services;

public class PatternImageFactory {

    public static byte[] CreatePatternImage(string imagePath, TableOrientation orientation, double sheetWidth, double sheetLength, IEnumerable<ImageText> text) {

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            // TODO: use skia sharp for cross platform support
            return Array.Empty<byte>();
        } else {

            try {
                var bitmap = GetBitmapFromMetaFile(imagePath);

                if (orientation == TableOrientation.Rotated) {
                    bitmap.RotateFlip(RotateFlipType.Rotate90FlipY);
                    // When rotating a bitmap that changes it's dimensions drawing can only be done within the original bitmaps dimension, so it must be saved and created as a new bitmap
                    using var stream = new MemoryStream();
                    bitmap.Save(stream, ImageFormat.Png);
                    var bitmap2 = new Bitmap(stream);
                    bitmap.Dispose();
                    bitmap = bitmap2;
                }

                AddTextToBitmap(bitmap, text, sheetWidth, sheetLength);
                return GetBitmapData(bitmap);
            } catch {

                float fontSize = 14;
                using var font = new Font("Tahoma", fontSize, FontStyle.Regular);

                string errtext = $"Can not load image: '{imagePath}'";

                int width = 2000;
                int height = 200;
                using var img = new Bitmap(width, height);
                using var cg = Graphics.FromImage(img);

                var textSize = cg.MeasureString(errtext, font);
                var drawPoint = new PointF(width / 2 - textSize.Width / 2, height / 2 - textSize.Height / 2); ;
                var rect = new RectangleF(drawPoint, textSize);
                cg.DrawString(errtext, font, Brushes.Black, rect);
                cg.Flush();

                return GetBitmapData(img);

            }

        }

    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    static void AddTextToBitmap(Bitmap bitmap, IEnumerable<ImageText> patternTexts, double sheetWidth, double sheetLength) {

        double mmToPxScaleX = bitmap.Width / sheetLength;
        double mmToPxScaleY = bitmap.Height / sheetWidth;

        float fontSize = 8;
        using var font = new Font("Tahoma", fontSize, FontStyle.Bold);

        foreach (ImageText text in patternTexts) {

            using var cg = Graphics.FromImage(bitmap);
            cg.SmoothingMode = SmoothingMode.AntiAlias;
            cg.InterpolationMode = InterpolationMode.HighQualityBicubic;
            cg.PixelOffsetMode = PixelOffsetMode.HighQuality;

            double x = text.Location.X * mmToPxScaleX;
            double y = bitmap.Height - (text.Location.Y * mmToPxScaleY);

            var textSize = cg.MeasureString(text.Text, font);

            var drawPoint = new PointF((float)x - textSize.Width / 2, (float)y - textSize.Height / 2);
                        
            var rect = new RectangleF(drawPoint, textSize);
            cg.FillRectangle(Brushes.White, rect);
            cg.DrawString(text.Text, font, Brushes.Black, rect);
            cg.Flush();

        }

    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    private static Bitmap TrimBitmapWhiteSpace(Bitmap bitmap) {
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    static Bitmap GetBitmapFromMetaFile(string path) {

        using Metafile? img = Image.FromFile(path) as Metafile;
        if (img is null) return new Bitmap(0, 0);

        MetafileHeader header = img.GetMetafileHeader();
        float scale = header.DpiX / 96f;

        var bitmap = new Bitmap((int)(scale * img.Width / header.DpiX * 100), (int)(scale * img.Height / header.DpiY * 100));
        using var g = Graphics.FromImage(bitmap);

        g.Clear(Color.White);
        g.ScaleTransform(scale, scale);
        g.DrawImage(img, 0, 0);

        return TrimBitmapWhiteSpace(bitmap);

    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    static byte[] GetBitmapData(Bitmap bitmap) {
        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

}
