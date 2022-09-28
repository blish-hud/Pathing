using System.Windows.Forms;

namespace BhModule.Community.Pathing.Scripting.Console {
    public partial class InputDiag : Form {

        public string UserInput => textBox1.Text;

        public InputDiag() {
            InitializeComponent();
        }

        public InputDiag(string prompt, string title) : this() {
            lblDiagDescription.Text = prompt;
            this.Text = title;
        }

        private void btnOk_Click(object sender, System.EventArgs e) {
            this.DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, System.EventArgs e) {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
