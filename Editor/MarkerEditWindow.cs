using System.Threading.Tasks;
using System.Windows.Forms;
using BhModule.Community.Pathing.Editor.Entity;
using BhModule.Community.Pathing.Entity;
using Blish_HUD;

namespace BhModule.Community.Pathing.Editor {
    public partial class MarkerEditWindow : Form {

        private static MarkerEditWindow _activeWindow;

        public static async Task SetMarker(StandardMarker marker) {
            if (_activeWindow == null) {
                (new MarkerEditWindow()).Show(ActiveForm);
            }

            _activeWindow.propertyGrid1.SelectedObject = marker;

            
            GameService.Graphics.World.AddEntity(new TranslateTool(marker));
        }

        public MarkerEditWindow() {
            _activeWindow?.Dispose();
            _activeWindow   = this;

            InitializeComponent();
        }
    }
}
