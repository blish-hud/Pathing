using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using Color = Microsoft.Xna.Framework.Color;

namespace BhModule.Community.Pathing.Editor.TypeEditors {
    public class ColorEditor : UITypeEditor {

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
            return UITypeEditorEditStyle.None;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context) {
            return true;
        }

        public override void PaintValue(PaintValueEventArgs e) {
            var color = (Color)e.Value;

            var convertedColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);

            e.Graphics.FillRectangle(new SolidBrush(convertedColor), e.Bounds);
        }

    }
}
