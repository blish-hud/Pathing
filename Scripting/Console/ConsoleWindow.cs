using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Humanizer;

namespace BhModule.Community.Pathing.Scripting.Console {
    public partial class ConsoleWindow : Form {

        private readonly PathingModule _module;

        public ConsoleWindow() {
            InitializeComponent();
        }

        public ConsoleWindow(PathingModule pathingModule) : this() {
            _module = pathingModule;
        }

        private int _lastLogMessage = 0;

        private readonly Dictionary<ScriptMessageLogLevel, Color> _logLevelColors = new() {
            { ScriptMessageLogLevel.System, Color.SlateGray },
            { ScriptMessageLogLevel.Info, Color.Black },
            { ScriptMessageLogLevel.Warn, Color.Orange },
            { ScriptMessageLogLevel.Error, Color.IndianRed }
        };

        private WatchTreeNode CreateOrUpdateNode(string objectName) {
            WatchTreeNode existingNode = null;

            foreach (var node in this.tvWatchWindow.Nodes) {
                if (node is WatchTreeNode wtn && wtn.ObjectName == objectName) {
                    existingNode = wtn;
                    break;
                }
            }

            if (existingNode == null) {
                existingNode = new WatchTreeNode(objectName);
                this.tvWatchWindow.Nodes.Add(existingNode);
            }

            return existingNode;
        }

        private void tOutputPoll_Tick(object sender, EventArgs e) {
            // Update reload button enabled.
            btnReloadPacks.Enabled = !_module.PackInitiator.IsLoading;

            // Update frametime.
            tsslScriptFrameTime.Text = _module.ScriptEngine.FrameExecutionTime.Humanize();
            tsslScriptFrameTime.ForeColor = _module.ScriptEngine.FrameExecutionTime.TotalMilliseconds switch {
                > 3 => Color.IndianRed,
                > 1 => Color.Orange,
                _ => Color.Black
            };

            // Update watch window.
            foreach (var watchValue in _module.ScriptEngine.Global.Debug.WatchValues) {
                var node = CreateOrUpdateNode(watchValue.Key);
                node.Refresh(watchValue.Value);
            }

            // Update script output.
            if (_lastLogMessage > _module.ScriptEngine.OutputMessages.Count) {
                _lastLogMessage = 0;
            }

            for (; _lastLogMessage < _module.ScriptEngine.OutputMessages.Count; _lastLogMessage++) {
                var newMessage = _module.ScriptEngine.OutputMessages[_lastLogMessage];

                string metaLine = $"[{newMessage.Timestamp} | {newMessage.Source}] ";

                AppendScriptConsoleOutput(metaLine, _logLevelColors[ScriptMessageLogLevel.System]);
                AppendScriptConsoleOutput($"{newMessage.Message.Replace(Environment.NewLine, Environment.NewLine + new string(' ', metaLine.Length))}", _logLevelColors[newMessage.LogLevel], true);
            }
        }

        private void AppendScriptConsoleOutput(string text, Color color, bool addNewLine = false) {
            rtbOutput.SuspendLayout();
            rtbOutput.SelectionColor = color;
            rtbOutput.AppendText(addNewLine
                                     ? $"{text}{Environment.NewLine}"
                                     : text);
            rtbOutput.ScrollToCaret();
            rtbOutput.ResumeLayout();
        }

        private async void btnCopyOutput_Click(object sender, EventArgs e) {
            try {
                await Blish_HUD.ClipboardUtil.WindowsClipboardService.SetTextAsync(rtbOutput.Text);
            } catch { /* NOOP */ }
        }

        private void btnClearOutput_Click(object sender, EventArgs e) {
            rtbOutput.Clear();
        }

        private void btnReloadPacks_Click(object sender, EventArgs e) {
            btnReloadPacks.Enabled = false;
            _module.PackInitiator.ReloadPacks();
        }

        private void tsClearWatchList_Click(object sender, EventArgs e) {
            _module.ScriptEngine.Global.Debug.ClearWatch();
        }

        private void tsWatchGlobal_Click(object sender, EventArgs e) {
            _module.ScriptEngine.EvalScript("Debug:Watch(\"_G\", _G)");
        }

        private void tsWatchCustom_Click(object sender, EventArgs e) {
            var input = new InputDiag("Specify the global variable to watch:", "Watch");

            if (input.ShowDialog() == DialogResult.OK) {
                _module.ScriptEngine.EvalScript($"Debug:Watch(\"{input.UserInput.Replace("\"", "")}\", {input.UserInput})");
            }
        }
    }
}
