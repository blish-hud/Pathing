using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.UI.Controls.TreeView
{
    public class LabelNode : TreeNodeBase
    {
        private Image _iconControl;
        private Label _labelControl;

        private Color _textColor = Color.White;

        public Color TextColor
        {
            get => _textColor;
            set {
                if (!SetProperty(ref _textColor, value) || _iconControl == null) return;

                _labelControl.TextColor = value;

                if (_iconControl != null)
                    _iconControl.Tint = value;
            }
        }

        private readonly string _text;
        private readonly AsyncTexture2D _icon;

        public LabelNode(string text, AsyncTexture2D icon = null)
        {
            _text = text;
            _icon = icon;

            this.ShowBackground   = true;
            this.PanelHeight      = 30;
            BackgroundOpacity     = 0.05f;
            BackgroundOpaqueColor = Color.LightYellow;
        }

        public virtual void Build()
        {
	        UpdateControls();
        }

        public void UpdateControls()
        {
            _labelControl?.Dispose();

            if (_icon != null)
            {
                _iconControl = new Image(_icon)
                {
                    Parent = this,
                    Size = new Point(22, 22),
                    Location = new Point(30, 3),
                    Tint = this.TextColor
                };
            }

            var iconPadding  = _iconControl == null ? 5 : 35;
            var arrowPadding = this.Expandable ? 20 : 0;


            var xPos = iconPadding + arrowPadding;

            _labelControl = new Label 
            {
                Parent         = this,
                Text           = _text,
                Location       = new Point(xPos, 5),
                AutoSizeHeight = true,
                AutoSizeWidth  = true,
                Font           = GameService.Content.DefaultFont16,
                TextColor      = this.TextColor,
                StrokeText     = true
            };
        }

        protected override void DisposeControl()
        {
            _iconControl?.Dispose();
            _labelControl?.Dispose();

            base.DisposeControl();
        }
    }
}
