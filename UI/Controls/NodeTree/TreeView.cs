using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.UI.Controls.TreeNodes;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.UI.Controls.TreeView
{
    public class TreeView : Container
    {
        private Control _scrollToChildControl = null;

        private PathingCategoryNode _rootNode;
        private static readonly Logger _logger = Logger.GetLogger<TreeView>();

        public  PackInitiator          PackInitiator  { get; private set; }
        public  IList<TreeNodeBase>    AllBaseNodes   { get; }      = new List<TreeNodeBase>();
        public  IList<TreeNodeBase>    ChildBaseNodes { get; }      = new List<TreeNodeBase>();
        private IList<PathingCategory> AllCategories  { get; set; } = new List<PathingCategory>();

        public event EventHandler<EventArgs> NodeLoadingStarted;
        public event EventHandler<EventArgs> NodesLoadedFinished;

        public TreeView(PackInitiator packInitiator) {
            PackInitiator                                                                   =  packInitiator;
        }

        public void AddNode(TreeNodeBase node) {
            if(!AllBaseNodes.Contains(node))
                AllBaseNodes.Add(node);
        }

        public void RemoveNode(TreeNodeBase node) {
            if(AllBaseNodes.Contains(node))
                AllBaseNodes.Remove(node);
        }

        public void SetPackInitiator(PackInitiator packInitiator) {
            if (packInitiator == null || PackInitiator == packInitiator) return;

            PackInitiator = packInitiator;

            PackInitiator.PackState.UserConfiguration.GlobalPathablesEnabled.SettingChanged += GlobalPathablesEnabledOnSettingChanged;
        }

        protected override void OnChildAdded(ChildChangedEventArgs e)
        {
            if (!(e.ChangedChild is TreeNodeBase newChild)) return;

            AddNode(newChild);
            this.ChildBaseNodes.Add(newChild);

            ReflowChildLayout(this.ChildBaseNodes);

            base.OnChildAdded(e);
        }

        protected override void OnChildRemoved(ChildChangedEventArgs e)
        {
            if (e.ChangedChild is TreeNodeBase newChild) {
                RemoveNode(newChild);
                this.ChildBaseNodes.Remove(newChild);
            }

            base.OnChildRemoved(e);
        }

        public override void RecalculateLayout() {
            try {
                ReflowChildLayout(ChildBaseNodes);
            } catch (Exception ex) {
                //Investigate why collection is sometimes modified during reflow
                _logger.Warn($"Could not recalculate TreeView layout: {ex.Message}");
            }
           
            base.RecalculateLayout();
        }

        private int ReflowChildLayout(IList<TreeNodeBase> containerChildren)
        {
            var lastBottom =  0;

            var children = containerChildren.Where(c => c.Visible).ToList();

            foreach (var child in children)
            {
                child.Location = new Point(0, lastBottom);

                lastBottom = child.Bottom;
            }
            
            return lastBottom;
        }

        public void ClearChildNodes()
        {
            var controlsQueue = new Queue<Control>(this.ChildBaseNodes);

            while (controlsQueue.Count > 0)
            {
                var control = controlsQueue.Dequeue();

                control.Parent = null;
                control.Dispose();
            }
        }

        public void LoadNodes() {
            NodeLoadingStarted?.Invoke(this, EventArgs.Empty);

            ClearChildNodes();
            AllBaseNodes.Clear();

            var rootCategory = PackInitiator.GetAllMarkersCategories();

            if (rootCategory == null) return;

            _rootNode?.Dispose();

            if (rootCategory.Count(c => c.LoadedFromPack) <= 0) return; //No packs installed

            _rootNode = new PathingCategoryNode(PackInitiator.PackState, rootCategory, false)
            {
                Name   = "All Markers",
                Width  = this.Width - 30,
                Parent = this
            };

            _rootNode.Checked = PackInitiator.PackState.UserConfiguration.GlobalPathablesEnabled.Value;

            _rootNode.CheckedChanged += (_, e) => {
                if(PackInitiator?.PackState != null)
                    PackInitiator.PackState.UserConfiguration.GlobalPathablesEnabled.Value = e.Checked;
            };

            _rootNode.Expand();

            AllCategories = CategoryUtil.FlattenCategories(rootCategory).ToList();

            this.NodesLoadedFinished?.Invoke(this, EventArgs.Empty);
        }

        private void GlobalPathablesEnabledOnSettingChanged(object sender, ValueChangedEventArgs<bool> e) {
            _rootNode.Checked = PackInitiator.PackState.UserConfiguration.GlobalPathablesEnabled.Value; ;
        }

        public async Task<(List<PathingCategory> categories, int skipped)> SearchAsync(string input, CancellationToken cancellationToken = default, bool forceShowAll = false)
        {
            if (string.IsNullOrWhiteSpace(input)) {
                await Task.CompletedTask;
                return (new List<PathingCategory>(), 0);
            }

            IEnumerable<PathingCategory> filteredResults = null;

            var skipped = 0;

            cancellationToken.ThrowIfCancellationRequested();

            string normalizedInput = input.Replace(" ", "");

            var results = AllCategories
                         .AsParallel()
                         .WithCancellation(cancellationToken)
                         .Where(c =>
            {
                if (string.IsNullOrWhiteSpace(c.DisplayName) || string.IsNullOrWhiteSpace(c.Name)) return false;

                string normalizedDisplayName = c.DisplayName?.Replace(" ", "");
                string normalizedName        = c.Name.Replace(" ", "");

                cancellationToken.ThrowIfCancellationRequested();

                return (normalizedDisplayName != null && normalizedDisplayName.IndexOf(normalizedInput, StringComparison.OrdinalIgnoreCase) >= 0) ||
                       normalizedName.IndexOf(normalizedInput, StringComparison.OrdinalIgnoreCase) >= 0;
            }).ToList();

            (filteredResults, skipped) = results.FilterCategories(PackInitiator.PackState, forceShowAll);

            return (filteredResults.ToList(), skipped);
        }

        public void RemoveNodeHighlights() {
            foreach (var node in AllBaseNodes) {
                if (node.Highlighted) node.Highlighted = false;
            }
        }

        public LabelNode SetSearchResults(IList<PathingCategory> categories, IPackState packState, int skipped = 0)
        {
            ClearChildNodes();

            LabelNode showAllSkippedCategories = null;

            if (skipped > 0 && packState.UserConfiguration.PackShowWhenCategoriesAreFiltered.Value) {
                showAllSkippedCategories = new LabelNode($"{skipped} hidden (click to show)", AsyncTexture2D.FromAssetId(358463))
                {
                    Clickable        = true,
                    Width            = this.Parent.Width - 14,
                    TextColor        = Color.LightYellow,
                    BasicTooltipText = string.Format(Strings.Info_HiddenCategories, PackInitiator.PackState.UserConfiguration.PackEnableSmartCategoryFilter.DisplayName),
                    Parent           = this
                };
            }
            
            foreach (var category in categories)
            {
                var node = new PathingCategoryNode(packState, category, false)
                {
                    Width       = this.Width - 30,
                    IsSearchResult = true,
                    Parent      = this
                };

                node.Active = !packState.CategoryStates.GetNamespaceInactive(category.Namespace);
            }

            return showAllSkippedCategories;
        }
        public void NavigateToPath(string path) {
            if(_rootNode == null) return;

            PathingCategoryNode currentNode = _rootNode;

            RemoveNodeHighlights();
            var splitPath = path.Split('.');


            var pathItemsWithIndex = splitPath
               .Select((pathItem, index) => new { pathItem, index, isLast = index == splitPath.Length - 1 });

            foreach (var item in pathItemsWithIndex) {
                if (string.IsNullOrWhiteSpace(item.pathItem)) continue;

                var categoryResult = currentNode
                                     .PathingCategory?
                                     .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n?.Name) && n.Name.Equals(item.pathItem));

                if (categoryResult == null || !categoryResult.LoadedFromPack) return;

                var baseNodes = currentNode.ChildBaseNodes ?? _rootNode?.ChildBaseNodes;

                if (baseNodes == null) 
                    return;

                var tempCurrentNode = baseNodes.OfType<PathingCategoryNode>().FirstOrDefault(n => n.PathingCategory == categoryResult);

                tempCurrentNode ??= new PathingCategoryNode(PackInitiator.PackState, categoryResult, false)
                {
                    Width  = currentNode.Width - 30,
                    Parent = currentNode
                };
                
                currentNode = tempCurrentNode;

                if (!item.isLast) {
                    currentNode.Expand();
                }
            }

            if (currentNode == null) return;

            currentNode.Highlighted = true;
            _scrollToChildControl    = currentNode;

            if (_scrollToChildControl != null && this.Parent is CustomFlowPanel parentPanel)
            {
                parentPanel.ScrollToChild(_scrollToChildControl, _scrollToChildControl.Height);
            }
        }

        public void UpdateCheckedState(PathingCategory category, bool active) {
            var node = AllBaseNodes
                      .OfType<PathingCategoryNode>()
                      .FirstOrDefault(n => n.PathingCategory == category);

            if (node != null) {
                node.Checked = active;
            }
        }

        protected override void OnResized(ResizedEventArgs e) {
            base.OnResized(e);

            if (_scrollToChildControl != null && this.Parent is CustomFlowPanel parentPanel)
            {
                parentPanel.ScrollToChild(_scrollToChildControl, _scrollToChildControl.Height);
                _scrollToChildControl = null;
            }
        }

        protected override void DisposeControl() {
            PackInitiator.PackState.UserConfiguration.GlobalPathablesEnabled.SettingChanged -= GlobalPathablesEnabledOnSettingChanged;
            
            base.DisposeControl();
        }

    }
}
