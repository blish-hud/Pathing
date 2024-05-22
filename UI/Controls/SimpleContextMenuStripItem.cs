using Blish_HUD.Controls;
using Blish_HUD.Input;
using System;

namespace BhModule.Community.Pathing.UI.Controls {
    internal class SimpleContextMenuStripItem : ContextMenuStripItem {

        private Action _onClick = null;
        private Action<bool> _onCheck = null;

        public SimpleContextMenuStripItem(string text, Action onClick) {
            this.Text = text;
            
            _onClick = onClick;
        }

        public SimpleContextMenuStripItem(string text, Action<bool> onCheck, bool isChecked) {
            this.Text = text;
            this.CanCheck = true;
            this.Checked = isChecked;

            _onCheck = onCheck;
        }

        protected override void OnClick(MouseEventArgs e) {
            base.OnClick(e);

            if (_onClick != null) {
                _onClick();
            }
        }

        protected override void OnCheckedChanged(CheckChangedEvent e) {
            base.OnCheckedChanged(e);

            if (_onCheck != null) {
                _onCheck(e.Checked);
            }
        }

    }
}
