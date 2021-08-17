using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using BhModule.Community.Pathing.Editor.Entity;
using BhModule.Community.Pathing.Editor.Panels;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using Blish_HUD;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.Editor {
    public partial class MarkerEditWindow : Form {

        private static MarkerEditWindow _activeWindow;

        public static void SetPathingEntity(IPackState packState, IPathingEntity pathingEntity) {
            if (_activeWindow == null || _activeWindow.IsDisposed) _activeWindow = new MarkerEditWindow();

            if (!_activeWindow.Visible) {
                _activeWindow.Show(ActiveForm);
            }

            //_activeWindow.splitContainer1.Panel2.Controls.Clear();

            _activeWindow.ActivePathingEntity = pathingEntity;
            _activeWindow.PackState           = packState;
        }

        public static void SetPathingCategory(IPackState packState, PathingCategory pathingCategory) {
            if (_activeWindow == null || _activeWindow.IsDisposed) _activeWindow = new MarkerEditWindow();

            _activeWindow ??= new MarkerEditWindow();

            if (!_activeWindow.Visible) {
                _activeWindow.Show(ActiveForm);
            }

            //_activeWindow.splitContainer1.Panel2.Controls.Clear();

            _activeWindow.pgPathingAttributeEditor.SelectedObject = new PathingCategoryEditWrapper(pathingCategory);
            _activeWindow.PackState                               = packState;
        }

        public static void SetPropertyPanel<T>(IPathingEntity pathingEntity, string attribute, T newPanel) where T : UserControl, IAttributeToolPanel {
            //_activeWindow.splitContainer1.Panel2.Controls.Clear();

            //newPanel.Parent = _activeWindow.splitContainer1.Panel2;
            //newPanel.SetTarget(pathingEntity, attribute);
        }

        private IPathingEntity _activePathingEntity;
        private IPackState     _packState;

        public IPathingEntity ActivePathingEntity {
            get => _activePathingEntity;
            private set {
                if (Equals(_activePathingEntity, value)) return;

                _activePathingEntity = value;
                UpdateViewForPathable();
            }
        }

        public IPackState PackState {
            get => _packState;
            private set {
                if (Equals(_packState, value)) return;

                _packState = value;
                UpdateViewForPackState();
            }
        }

        private CategoryTree _tvCategoryListing;

        public MarkerEditWindow() {
            _activeWindow?.Dispose();
            _activeWindow = this;

            InitializeComponent();
        }

        private void UpdateViewForPathable() {
            pgPathingAttributeEditor.SelectedObject = this.ActivePathingEntity;

            if (this.ActivePathingEntity is StandardMarker marker) {
                GameService.Graphics.World.AddEntity(new TranslateTool(marker));
            }
        }

        private void UpdateViewForPackState() {
            if (_tvCategoryListing == null) return;

            _tvCategoryListing.PackState = this.PackState;
        }

        private void MarkerEditWindow_Shown(object sender, System.EventArgs e) {
            _tvCategoryListing = new CategoryTree() {
                Parent        = splitContainer2.Panel1,
                Anchor        = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left,
                Size          = new Size(splitContainer2.Panel1.Width, splitContainer2.Panel1.Height - tsPackToolBar.Bottom),
                Location      = new Point(0, tsPackToolBar.Bottom),
                ImageList     = ilEntityTreeIcons,
                ShowRootLines = true,
                HideSelection = false,
            };

            UpdateViewForPackState();
        }
    }
}
