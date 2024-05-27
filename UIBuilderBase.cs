using FrooxEngine;
using QuantityX;
using BaseX;
namespace NovrX.UIX
{
    public class UIBuilderBase
    {

       
    }


    public static class RadiantUI_Constants
    {
        public const string LINE_GRID_CELL_TRANSPARENT = "LineGridCell_Transparent";

        public const string LINE_GRID_CELL_TRANSPARENT_OUTLINE = "LineGridCell_Transparent_Outline";

        public const float GRID_CELL_SIZE = 32f;

        public const float GRID_PADDING = 8f;

        public static color TEXT_COLOR => color.White;

        public static color BUTTON_COLOR => new color(0.08f);

        public static color HIGHLIGHT_COLOR => color.FromHexCode("#0049C6");

        public static string HEADING_HEX => "#22B2FF";

        public static string LABEL_HEX => "#72CEFF";

        public static color HEADING_COLOR => color.FromHexCode(HEADING_HEX);

        public static color LABEL_COLOR => color.FromHexCode(LABEL_HEX);

        public static void SetupDefaultStyle(UIBuilder ui, bool padButtonText = false)
        {
            ui.Style.TextColor = TEXT_COLOR;
            ui.Style.ButtonColor = BUTTON_COLOR;
            ui.Style.ButtonSpriteColor = color.White;
            ui.Style.DisabledColor = new color(0.1f, 0.08f, 0.08f);
            ui.Style.DisabledAlpha = 0.25f;
            if (padButtonText)
            {
                ui.Style.ButtonTextPadding = 4f;
            }
        }

       

       
    }
}
