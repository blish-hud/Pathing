using System;
using System.Threading.Tasks;
using System.Threading;
using BhModule.Community.Pathing.UI.Controls.TreeView;
using BhModule.Community.Pathing.UI.Presenter;
using BhModule.Community.Pathing.Utility;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Blish_HUD;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.UI.Views {
    public class CategoryTreeView : View {
        private static readonly Logger    _logger = Logger.GetLogger<CategoryTreeView>();
        private                 FlowPanel RepoFlowPanel { get; set; }

        public TreeView TreeView { get; private set; }

        private TextBox    _searchBox;

        private Label _searchStatusLabel;

        private LoadingSpinner _loadingSpinner;

        private readonly PathingModule _module;

        private CancellationTokenSource _cancellationTokenSource;

        public PathingCategory TargetCategory { get; set; }

        public CategoryTreeView(PathingModule module) {
            _module = module;

            this.WithPresenter(new CategoryTreePresenter(this, module));
        }

        protected override void Build(Container buildPanel) {
            this._searchBox = new TextBox {
                PlaceholderText = "Search markers or insert path...",
                Parent = buildPanel,
                Location = new Point(0, 10),
                Width = buildPanel.ContentRegion.Width - 25,
            };

            this._searchBox.TextChanged += SearchBoxTextChanged;

            this._searchStatusLabel = new Label
            {
                Text = "No categories found...",
                Size = new Point(100, 20),
                Font = GameService.Content.DefaultFont18,
                AutoSizeHeight = true,
                AutoSizeWidth  = true,
                Location       = new Point(buildPanel.Width / 2 - 120, buildPanel.Height / 2 - 80),
                Parent         = buildPanel,
                Visible        = false,
            };

            this.RepoFlowPanel = new CustomFlowPanel {
                Size       = new Point(buildPanel.ContentRegion.Width, buildPanel.ContentRegion.Height - _searchBox.Bottom - 5),
                Top        = _searchBox.Bottom + 5,
                CanScroll  = true,
                ShowBorder = true,
                Parent     = buildPanel
            };

            this.TreeView = new TreeView(_module.PackInitiator) {
                HeightSizingMode = SizingMode.AutoSize,
                Size             = new Point(RepoFlowPanel.Width, RepoFlowPanel.Height),
                Parent           = RepoFlowPanel
            };

            this._loadingSpinner = new LoadingSpinner
            {
                Parent   = buildPanel,
                Location = new Point(buildPanel.Width / 2 - 75, buildPanel.Height / 2 - 75),
                Size     = new Point(55, 55)
            };

            this.TreeView.NodeLoadingStarted += (_, _) => {
                _cancellationTokenSource?.Cancel();

                SetLoading(true);
                ResetSearch();
            };

            this.TreeView.NodesLoadedFinished += (_, _) => {
                SetLoading(false);
            };
        }

        public void SetLoading(bool loading)
        {
            this._loadingSpinner.Visible = loading;
        }

        private void SearchBoxTextChanged(object sender, EventArgs e) {
            if (Presenter is CategoryTreePresenter presenter) {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();

                TreeView.RemoveNodeHighlights();
                _searchStatusLabel.Visible = false;

                if (_searchBox.Text.StartsWith(".")) {
                    TreeView.NavigateToPath(_searchBox.Text);
                    return;
                }

                if (string.IsNullOrWhiteSpace(_searchBox.Text)) {
                    presenter.DoUpdateView();
                    return;
                }


                TreeView.ClearChildNodes();
                SetLoading(true);

                Task.Run(async () => {
                    await ExecuteSearch(_searchBox.Text, _cancellationTokenSource.Token);
                }, _cancellationTokenSource.Token);
            }
        }

        public void NavigateToCategory(PathingCategory category) {
            ResetSearch();

            this.TreeView?.LoadNodes();
            this.TreeView?.NavigateToPath(category.GetPath());
        }

        private static readonly SemaphoreSlim _searchSemaphore = new SemaphoreSlim(1, 1); // Limit to 1 concurrent search

        private async Task ExecuteSearch(string input, CancellationToken cancellationToken, bool forceShowAll = false) {
            await _searchSemaphore.WaitAsync(cancellationToken);

            try {
                
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(200, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                var searchResult = await TreeView.SearchAsync(input, cancellationToken, forceShowAll);

                cancellationToken.ThrowIfCancellationRequested();

                var showAllSkippedNode = TreeView.SetSearchResults(searchResult.categories, _module.PackInitiator.PackState, searchResult.skipped);

                cancellationToken.ThrowIfCancellationRequested();

                if (showAllSkippedNode != null)
                {
                    showAllSkippedNode.LeftMouseButtonReleased += async (_, _) => {
                        cancellationToken.ThrowIfCancellationRequested();

                        await ExecuteSearch(input, cancellationToken, true);
                    };
                }

                _searchStatusLabel.Visible = searchResult.categories.Count <= 0;
                SetLoading(false);
            }
            catch (OperationCanceledException _)
            {
                //Triggered when a new search is executed
            }
            catch (Exception ex)
            {
                _logger.Error($"Category search failed with error: {ex.Message}");
            }
            finally
            {
                _searchSemaphore.Release();
            }
        }

        public void ResetSearch() {
            _searchBox.Text            = string.Empty;
        }
    }
}
