using System.Threading.Tasks;
using System.Windows.Forms;
using BhModule.Community.Pathing.Entity;

namespace BhModule.Community.Pathing.Editor {
    public partial class MarkerEditWindow : Form {

        private static MarkerEditWindow _activeWindow;

        public static async Task SetMarker(StandardMarker marker) {
            if (_activeWindow == null) {
                (new MarkerEditWindow()).Show();
            }

            _activeWindow.propertyGrid1.SelectedObject = marker;
        }

        public MarkerEditWindow() {
            _activeWindow?.Dispose();
            _activeWindow = this;

            InitializeComponent();
        }
    }
}
