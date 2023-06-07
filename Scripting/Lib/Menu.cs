using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Controls;
using Neo.IronLua;

namespace BhModule.Community.Pathing.Scripting.Lib {
    public class Menu {

        public string                Name     { get; }
        public Func<Menu, LuaResult> OnClick  { get; }
        public bool                  CanCheck { get; set; }
        public bool                  Checked  { get; set; }
        public string                Tooltip { get; set; }

        private readonly List<Menu> _menus = new();
        public IReadOnlyCollection<Menu> Menus => _menus.AsReadOnly();

        public Menu(string name, Func<Menu, LuaResult> onClick, bool canCheck = false, bool @checked = false, string tooltip = null) {
            this.Name     = name;
            this.OnClick  = onClick;
            this.CanCheck = canCheck;
            this.Checked  = @checked;
            this.Tooltip  = tooltip;
        }

        public Menu Add(string name, Func<Menu, LuaResult> onClick) {
            return Add(name, onClick, false, false);
        }

        public Menu Add(string name, Func<Menu, LuaResult> onClick, bool canCheck, bool @checked) {
            return Add(name, onClick, canCheck, @checked, null);
        }

        public Menu Add(string name, Func<Menu, LuaResult> onClick, bool canCheck, bool @checked, string tooltip) {
            foreach (var menu in _menus) {
                if (string.Equals(menu.Name, name, StringComparison.OrdinalIgnoreCase)) {
                    // Menu with this name already exists - just return it.
                    return menu;
                }
            }

            var newMenu = new Menu(name, onClick, canCheck, @checked, tooltip);
            _menus.Add(newMenu);

            return newMenu;
        }

        public void Remove(Menu menuRef) {
            _menus.Remove(menuRef);
        }

        internal ContextMenuStripItem BuildMenu() {
            var menu = new ContextMenuStripItem() {
                Text             = this.Name,
                CanCheck         = this.CanCheck,
                Checked          = this.Checked,
                BasicTooltipText = this.Tooltip
            };

            menu.Click += (_, _) => {
                this.Checked = menu.Checked;
                this.OnClick?.Invoke(this);
            };

            if (_menus.Any()) {
                var subMenu = new ContextMenuStrip();

                foreach (var item in _menus) {
                    subMenu.AddMenuItem(item.BuildMenu());
                }

                menu.Submenu = subMenu;
            }

            return menu;
        }

    }
}
