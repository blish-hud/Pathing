using System;
using System.Collections.Generic;
using Neo.IronLua;

namespace BhModule.Community.Pathing.Scripting.Lib; 

public class Debug {

    internal Dictionary<string, object> WatchValues { get; } = new();

    private readonly PathingGlobal _global;

    internal Debug(PathingGlobal global) {
        _global = global;
    }

    public void ShowMessage(string message) {
        Blish_HUD.Controls.ScreenNotification.ShowNotification(message);
    }

    public void Print(string message) {
        _global.ScriptEngine.PushMessage(message, ScriptMessageLogLevel.Info);
    }

    public void Info(string message) => Print(message);

    public void Warn(string message) {
        _global.ScriptEngine.PushMessage(message, ScriptMessageLogLevel.Warn);
    }

    public void Error(string message) {
        _global.ScriptEngine.PushMessage(message, ScriptMessageLogLevel.Error);
    }

    public void Watch(string key, object value) {
        this.WatchValues[key] = value;
    }

    internal void ClearWatch() {
        this.WatchValues.Clear();
    }

    public void ClearWatch(string key) {
        this.WatchValues.Remove(key);
    }

}