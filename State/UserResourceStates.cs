using System;
using System.IO;
using System.Threading.Tasks;
using BhModule.Community.Pathing.State.UserResources;
using BhModule.Community.Pathing.State.UserResources.Population.Converters;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Microsoft.Xna.Framework;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace BhModule.Community.Pathing.State {
    public class UserResourceStates : ManagedState {

        private static readonly Logger Logger = Logger.GetLogger<UserResourceStates>();

        private const string POPULATE_FILE = "populate.yaml";
        private const string IGNORE_FILE   = "ignore.yaml";

        private const int INTERVAL_UPDATELOOP = int.MaxValue;

        public PopulationDefaults Population { get; set; }
        public IgnoreDefaults     Ignore     { get; set; }

        public UserResourceStates(IRootPackState rootPackState) : base(rootPackState, INTERVAL_UPDATELOOP) {

        }

        protected override async Task<bool> Initialize() {
            await ExportDefault();
            await LoadState();

            return true;
        }

        private async Task ExportDefault() {
            string userResourceDir = DataDirUtil.GetSafeDataDir(DataDirUtil.COMMON_USER);

            var yamlSerializer = new SerializerBuilder()
                                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                                .WithTypeConverter(new ColorConverter())
                                .Build();

            // populate
            string populateFile = Path.Combine(userResourceDir, POPULATE_FILE);
            if (!File.Exists(populateFile)) {
                try {
                    using var populateWriter = File.CreateText(populateFile);
                    await populateWriter.WriteAsync(yamlSerializer.Serialize(new PopulationDefaults()));
                } catch (Exception e) {
                    Logger.Error(e, $"Failed to write defaults to {POPULATE_FILE}");
                }
            }

            // ignore
            string ignoreFile = Path.Combine(userResourceDir, IGNORE_FILE);
            if (!File.Exists(Path.Combine(userResourceDir, IGNORE_FILE))) {
                try {
                    using var ignoreWriter = File.CreateText(ignoreFile);
                    await ignoreWriter.WriteAsync(yamlSerializer.Serialize(new IgnoreDefaults()));
                } catch (Exception e) {
                    Logger.Error(e, $"Failed to write defaults to {IGNORE_FILE}");
                }
            }
        }

        private async Task LoadState() {
            string userResourceDir = DataDirUtil.GetSafeDataDir(DataDirUtil.COMMON_USER);

            var yamlDeserializer = new DeserializerBuilder()
                                  .WithNamingConvention(HyphenatedNamingConvention.Instance)
                                  .WithTypeConverter(new ColorConverter())
                                  .IgnoreUnmatchedProperties()
                                  .Build();

            // Load population defaults (defines the default values for pathable attributes when a value isn't provided).
            try {
                using var populateReader = File.OpenText(Path.Combine(userResourceDir, POPULATE_FILE));
                this.Population = yamlDeserializer.Deserialize<PopulationDefaults>(await populateReader.ReadToEndAsync());
            } catch (Exception e) {
                Logger.Error(e, $"Failed to read or parse {POPULATE_FILE}.");
                Logger.Warn($"Since {POPULATE_FILE} failed to load, internal defaults will be used instead. Delete it to have it rebuilt.");
            } finally {
                this.Population ??= new PopulationDefaults();
            }

            // Load ignore defaults (defines which maps certain features are ignored on).
            try {
                using var ignoreReader = File.OpenText(Path.Combine(userResourceDir, IGNORE_FILE));
                this.Ignore = yamlDeserializer.Deserialize<IgnoreDefaults>(await ignoreReader.ReadToEndAsync());
            } catch (Exception e) {
                Logger.Error(e, $"Failed to read or parse {IGNORE_FILE}.");
                Logger.Warn($"Since {IGNORE_FILE} failed to load, internal defaults will be used instead.  Delete it to have it rebuilt.");
            } finally {
                this.Ignore ??= new IgnoreDefaults();
            }
        }

        protected override void Unload() {
            this.Population = null;
            this.Ignore     = null;
        }

        public override async Task Reload() {
            Unload();

            await LoadState();
        }

        protected override void Update(GameTime gameTime) { /* NOOP */ }

    }
}
