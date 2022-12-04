using QuestPDF.Fluent;

namespace ApplicationCore.Features.CNC.Services.Domain.PDF;

public static class StyleExtensions {

    public static TextSpanDescriptor FontWeight(this TextSpanDescriptor text, int weight) => weight switch {
        100 => text.Thin(),
        200 => text.ExtraLight(),
        300 => text.Light(),
        400 => text.NormalWeight(),
        500 => text.Medium(),
        600 => text.SemiBold(),
        700 => text.Bold(),
        800 => text.ExtraBold(),
        900 => text.Black(),
        1000 => text.ExtraBlack(),
        _ => text.NormalWeight()
    };

    public static TextSpanDescriptor WithStyle(this TextSpanDescriptor text, SectionFontStyle style) =>
        text.FontSize(style.Size)
            .FontColor(style.Color)
            .FontFamily(style.Family)
            .FontWeight(style.Weight);

}