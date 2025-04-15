using System;
using System.Collections.Generic;
using BhModule.Community.Pathing.UI.Models;
using BhModule.Community.Pathing.UI.Tooltips;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Controls.Effects;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BhModule.Community.Pathing.UI.Controls.TreeNodes
{

    public abstract class PathingNode : TreeNodeBase
    {
        protected Image IconControl;
        protected Label LabelControl;

        protected FlowPanel _detailsPanel;

        protected FlowPanel _propertiesPanel;

        private Color _textColor = Color.White;

        public Color TextColor
        {
            get => _textColor;
            set {
                if (!SetProperty(ref _textColor, value) || LabelControl == null) return;
               
                LabelControl.TextColor = _textColor;
            }
        }

        protected List<PathingTexture> IconTextures = new List<PathingTexture>();

        protected int IconPaddingTop = 0;

        protected Point IconSize = new Point(30, 30);

        public bool ShowIconTooltip = true;

        private bool _checkable;
        public bool Checkable {
            get => _checkable;
            set {
                if (!SetProperty(ref _checkable, value)) return;

                if (value) {
                    this._checkbox?.Show();
                }
                else {
                    this._checkbox?.Hide();
                }
            }
        }

        private bool _checked;

        public bool Checked
        {
            get => _checked;
            set {
                if (SetProperty(ref _checked, value) && this._checkbox != null)
                    this._checkbox.Checked = value;
            }
        }

        protected bool CheckDisabled = false;

        protected Checkbox _checkbox;

        private bool _built = false;

        protected bool DoBuildContextMenu { get; set; } = true;
        protected PathingNode(string name)
        {
            this.Name  = name;

            this.EffectBehind = new ScrollingHighlightEffect(this);

            this.ShowBackground = true;
            this.PanelHeight    = 40;
        }

        public virtual void Build() {
            if (_built) return;

            BuildDetailsPanel();

            BuildCheckbox();
            BuildIcon();
            BuildNameLabel();

            BuildPropertiesPanel();

            if(DoBuildContextMenu)
                BuildContextMenu();

            _built = true;
        }

        private void BuildDetailsPanel()
        {
            if (_detailsPanel != null) throw new InvalidOperationException("Requirements panel already exists.");

            _detailsPanel = new FlowPanel()
            {
                Parent         = this,
                FlowDirection  = ControlFlowDirection.LeftToRight,
                Size           = new Point(this.ContentRegion.Width - 220, this.PanelHeight),
                Location       = new Point(28, 1),
                ControlPadding = new Vector2(5, 0),
                CanScroll      = false,
                Tooltip = this.Tooltip,
                ShowTint       = DevMode
            };
        }

        private void BuildPropertiesPanel()
        {
            if (_propertiesPanel != null) throw new InvalidOperationException("Requirements panel already exists.");

            _propertiesPanel = new FlowPanel()
            {
                Parent         = this,
                FlowDirection  = ControlFlowDirection.RightToLeft,
                Size           = new Point(200,              this.PanelHeight),
                Location       = new Point(this.Width - 210, 1),
                ControlPadding = new Vector2(5, 0),
                CanScroll      = false,
                ShowTint       = DevMode 
            };
        }

        protected override void OnResized(ResizedEventArgs e) {
            if(_propertiesPanel != null)
                _propertiesPanel.Left = e.CurrentSize.X - 210;

            base.OnResized(e);
        }

        protected override void OnParentChanged() {
            base.OnParentChanged();

            if (Parent is PathingNode parentNode && !CheckDisabled) 
                CheckDisabled = parentNode.CheckDisabled;
        }

        private void BuildIcon() {
            if (IconTextures.Count <= 0) return;

            var iconContainer = new Panel {
                Parent  = _detailsPanel,
                Size    = new Point(IconSize.X * IconTextures.Count + 5, this.Height),
                Tooltip = (IconTextures.Count > 0 && ShowIconTooltip) ? new Tooltip(new EntityTextureTooltip(IconTextures)) : null
            };

            var leftPos = 0;

            foreach (var item in IconTextures) {
                _ = new Image(item.Icon)
                {
                    Top  = IconPaddingTop,
                    Left = leftPos,
                    Size = IconSize,
                    Tint = item.Tint,
                    Tooltip = iconContainer.Tooltip,
                    Parent  = iconContainer
                };

                leftPos += IconSize.X;
            }
        }

        private void BuildNameLabel()
        {
            LabelControl?.Dispose();

            LabelControl = new Label
            {
                Parent           = _detailsPanel,
                Text             = this.Name,
                Height           = this.PanelHeight,
                Width            = _detailsPanel.Width - 60 - (IconTextures.Count * IconSize.X),
                Font             = GameService.Content.DefaultFont16,
                TextColor        = this.TextColor,
                WrapText         = true,
                StrokeText       = true,
                BasicTooltipText = this.BasicTooltipText
            };
        }

        public EventHandler<CheckChangedEvent> CheckedChanged;

        public void BuildCheckbox()
        {
            if (!this.Checkable) return;

            var checkboxContainer = new Panel
            {
                Parent = _detailsPanel,
                Size   = new Point(this.PanelHeight / 2 + 5, this.PanelHeight),
            };

            this._checkbox = new Checkbox
            {
                Parent  = checkboxContainer,
                Left    = 5,
                Size    = new Point(this.PanelHeight, this.PanelHeight),
                Checked = this.Checked,
                Enabled = !CheckDisabled
            };

            this._checkbox.CheckedChanged += CheckboxOnCheckedChanged;
        }

        private void CheckboxOnCheckedChanged(object sender, CheckChangedEvent e) {
            this.Checked = e.Checked;
            CheckedChanged?.Invoke(sender, e);
        }

        protected virtual void BuildContextMenu() {
            this.Menu?.Dispose();
            this.Menu = new ContextMenuStrip();

            BuildCopyName();

            if(this.Checkable)
                BuildDeselectAdjacentNodes();
        }

        private void BuildCopyName() {
            if (string.IsNullOrWhiteSpace(this.Name)) return;

            var stripItem = new ContextMenuStripItem("Copy Name")
            {
                Parent = this.Menu
            };

            stripItem.Click += (_, _) => CopyToClipboard(this.Name, $"\"{this.Name}\" copied to clipboard");
        }

        private void BuildDeselectAdjacentNodes()
        {
            var stripItem = new ContextMenuStripItem("Deselect Adjacent Categories")
            {
                Parent = this.Menu
            };

            stripItem.Click += (_, _) => {
                DeselectAdjacentNodesExcept(this);
            };
        }

        protected void CopyToClipboard(string value, string message) {
            ClipboardUtil.WindowsClipboardService.SetTextAsync(value).ContinueWith(t => {
                if (!string.IsNullOrWhiteSpace(message) && t.IsCompleted && t.Result)
                {
                    ScreenNotification.ShowNotification(string.Format(message, value),
                                                        ScreenNotification.NotificationType.Info,
                                                        null,
                                                        2);
                }
            });
        }

        public void DeselectAdjacentNodesExcept(PathingNode node)
        {
            foreach (var child in this.Parent.Children) {
                if (!(child is PathingNode childNode) || !childNode.Checkable) continue;

                if (child != node)
                    childNode.Checked = false;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            if (!_built)
                Build();

            base.OnShown(e);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            // If CTRL is held down when clicked, uncheck all adjacent categories except for this one.
            if (GameService.Input.Keyboard.ActiveModifiers.HasFlag(ModifierKeys.Ctrl))
            {
                DeselectAdjacentNodesExcept(this);
                return;
            }

            // Mouse is over checkbox
            if (this._checkbox != null && this._checkbox.AbsoluteBounds.Contains(Control.Input.Mouse.Position))
                return;

            base.OnClick(e);
        }

        protected override void DisposeControl()
        {
            IconControl?.Dispose();
            LabelControl?.Dispose();

            base.DisposeControl();
        }
    }
}
