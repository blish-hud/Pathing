using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.State {
    public class RaidStates : ManagedState {

        private static readonly Logger Logger = Logger.GetLogger<RaidStates>();

        private const double INTERVAL_CHECKACHIEVEMENTS = 300010; // 5 minutes + 10ms

        private double _lastRaidCheck = INTERVAL_CHECKACHIEVEMENTS;

        private HashSet<string> _completedRaids;

        public RaidStates(IRootPackState rootPackState) : base(rootPackState) { }

        public override Task Reload() {
            return Task.CompletedTask;
        }

        public override void Update(GameTime gameTime) {
            UpdateCadenceUtil.UpdateAsyncWithCadence(UpdateRaids, gameTime, INTERVAL_CHECKACHIEVEMENTS, ref _lastRaidCheck);
        }

        protected override Task<bool> Initialize() {
            PathingModule.Instance.Gw2ApiManager.SubtokenUpdated += Gw2ApiManager_SubtokenUpdated;

            return Task.FromResult(true);
        }

        public bool AreRaidsComplete(IEnumerable<string> raids) {
            return _completedRaids != null && _completedRaids.IsSupersetOf(raids);
        }

        private void Gw2ApiManager_SubtokenUpdated(object sender, ValueEventArgs<IEnumerable<TokenPermission>> e) {
            // Clear raid states so that we don't continue to use raid completion info from the last user.
            _completedRaids = null;

            // Reset our check interval so that we check immediately now that we have a new token.
            _lastRaidCheck = INTERVAL_CHECKACHIEVEMENTS;
        }

        private async Task UpdateRaids(GameTime gameTime) {
            if (!this.Running) return;

            try {
                // v2/account/raids requires "account" and "progression" permissions.
                if (PathingModule.Instance.Gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Progression })) {
                    Logger.Debug("Getting user raids from the API.");

                    var raids = await PathingModule.Instance.Gw2ApiManager.Gw2ApiClient.V2.Account.Raids.GetAsync();

                    _completedRaids = raids.Select(raid => raid.ToLowerInvariant()).ToHashSet();

                    Logger.Debug("Loaded {raidCount} completed raids from the API.", _completedRaids.Count());
                } else {
                    Logger.Debug("Skipping raid progress from the API - API key does not give us permission.");
                }
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to load account raid progress.");
            }
        }

        public override Task Unload() {
            PathingModule.Instance.Gw2ApiManager.SubtokenUpdated -= Gw2ApiManager_SubtokenUpdated;

            return Task.CompletedTask;
        }

    }
}
