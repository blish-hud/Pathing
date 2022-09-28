using System;
using System.Linq;
using System.Windows.Forms;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.Utility;
using Microsoft.Xna.Framework;
using Neo.IronLua;

namespace BhModule.Community.Pathing.Scripting.Console {
    internal class WatchTreeNode : TreeNode {

        public string ObjectName { get; set; }

        private TreeNode _loadingNode;

        public WatchTreeNode(string objectName) {
            this.ObjectName = objectName;
        }

        private void UpdateValueName(object luaObject) {
            this.Text = $"{this.ObjectName}: {LuaObjectToString(luaObject)}";
        }

        private void UpdateTableName(LuaTable luaTable) {
            this.Text = $"{this.ObjectName} [{luaTable.Values.Count}]";
        }

        private static string LuaObjectToString(object luaObject) {
            return luaObject switch {
                null => "nil",
                StandardMarker marker => $"Marker[{marker.Guid.ToBase64String()}]",
                Guid guid => guid.ToBase64String(),
                _ => luaObject.ToString()
            };
        }

        private WatchTreeNode CreateOrUpdateNode(string objectName, int targetIndex) {
            WatchTreeNode existingNode = null;

            foreach (var node in this.Nodes) {
                if (node is WatchTreeNode wtn && wtn.ObjectName == objectName) {
                    existingNode = wtn;
                    break;
                }
            }

            if (existingNode == null) {
                existingNode = new WatchTreeNode(objectName);
                this.Nodes.Insert(targetIndex, existingNode);
            }

            return existingNode;
        }

        public void Refresh(object luaObject) {
            if (luaObject is not LuaTable luaTable) {
                // We don't have anything inside of us.
                this.Nodes.Clear();

                UpdateValueName(luaObject);
                return;
            }

            UpdateTableName(luaTable);

            if (this.IsExpanded) {
                // Remove the "Loading..." node.
                this.Nodes.Remove(_loadingNode);

                int index          = 0;
                var remainingNodes = this.Nodes.OfType<WatchTreeNode>().ToList();
                foreach (var value in luaTable) {
                    var node = CreateOrUpdateNode(LuaObjectToString(value.Key), index);
                    node.Refresh(value.Value);

                    // Prevent this node from getting cleaned up.
                    remainingNodes.Remove(node);

                    index++;
                }

                foreach (var orphanNode in remainingNodes) {
                    this.Nodes.Remove(orphanNode);
                }
            } else if (luaTable.Values.Any() && this.Nodes.Count == 0) {
                // Ensure we have our loading node.
                _loadingNode = new TreeNode("Loading...");
                this.Nodes.Add(_loadingNode);
            } else if (!luaTable.Values.Any() && this.Nodes.Count > 0) {
                this.Nodes.Remove(_loadingNode);
            }
        }

    }
}
