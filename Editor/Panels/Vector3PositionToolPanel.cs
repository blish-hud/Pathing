using System.Windows.Forms;
using BhModule.Community.Pathing.Entity;
using Blish_HUD;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.Editor.Panels {
    public partial class Vector3PositionToolPanel : UserControl, IAttributeToolPanel {

        private IPathingEntity _pathingEntity;
        private string _activeAttribute;

        public Vector3PositionToolPanel() {
            InitializeComponent();
        }

        public void SetTarget(IPathingEntity pathingEntity, string attribute) {
            _pathingEntity   = pathingEntity;
            _activeAttribute = attribute;
        }

        private void UpdateValue(Vector3 newValue) {
            switch (_activeAttribute) {
                case nameof(StandardMarker.Position):
                    if (_pathingEntity is StandardMarker marker) marker.Position = newValue;
                    break;
            }
        }

        private void bttnMoveHere_Click(object sender, System.EventArgs e) {
            UpdateValue(GameService.Gw2Mumble.PlayerCharacter.Position);
        }
    }
}
