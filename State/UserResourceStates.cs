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

        /// <summary>
        /// <inheritdoc cref="AdvancedDefaults"/>
        /// </summary>
        public AdvancedDefaults Advanced { get; set; }

        /// <summary>
        /// <inheritdoc cref="PopulationDefaults"/>
        /// </summary>
        public PopulationDefaults Population { get; set; }

        /// <summary>
        /// <inheritdoc cref="IgnoreDefaults"/>
        /// </summary>
        public IgnoreDefaults Ignore { get; set; }

        /// <summary>
        /// <inheritdoc cref="StaticValues"/>
        /// </summary>
        public StaticValues Static { get; set; }

        public UserResourceStates(IRootPackState rootPackState) : base(rootPackState) { /* NOOP */ }

        protected override async Task<bool> Initialize() {
            await ExportAllDefault();
            await LoadAllStates();

            return true;
        }

        private async Task ExportDefaultState<T>(string statePath, ISerializer yamlSerializer, T defaultExport) {
            if (!File.Exists(statePath)) {
                try {
                    using var stateWriter = File.CreateText(statePath);
                    await stateWriter.WriteAsync(yamlSerializer.Serialize(defaultExport));
                } catch (Exception e) {
                    Logger.Error(e, $"Failed to write state defaults to {statePath}");
                }
            }
        }

        private async Task ExportAllDefault() {
            string userResourceDir = DataDirUtil.GetSafeDataDir(DataDirUtil.COMMON_USER);

            var yamlSerializer = new SerializerBuilder()
                                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                                .WithTypeConverter(new ColorConverter())
                                .Build();

            await ExportDefaultState(Path.Combine(userResourceDir, AdvancedDefaults.FILENAME),   yamlSerializer, new AdvancedDefaults());
            await ExportDefaultState(Path.Combine(userResourceDir, PopulationDefaults.FILENAME), yamlSerializer, new PopulationDefaults());
            await ExportDefaultState(Path.Combine(userResourceDir, IgnoreDefaults.FILENAME),     yamlSerializer, new IgnoreDefaults());
            await ExportDefaultState(Path.Combine(userResourceDir, StaticValues.FILENAME),       yamlSerializer, new StaticValues());
        }

        private async Task<T> LoadState<T>(string statePath, IDeserializer yamlDeserializer, Func<T> returnOnError) where T : class {
            T result = null;

            try {
                using var stateReader = File.OpenText(statePath);
                result = yamlDeserializer.Deserialize<T>(await stateReader.ReadToEndAsync());
            } catch (Exception e) {
                Logger.Warn(e, $"Failed to read or parse {statePath}.  As a result, internal defaults will be used instead.  Delete it to have it rebuilt.");
            }

            return result ?? returnOnError();
        }

        private async Task LoadAllStates() {
            string userResourceDir = DataDirUtil.GetSafeDataDir(DataDirUtil.COMMON_USER);

            var yamlDeserializer = new DeserializerBuilder()
                                  .WithNamingConvention(HyphenatedNamingConvention.Instance)
                                  .WithTypeConverter(new ColorConverter())
                                  .IgnoreUnmatchedProperties()
                                  .Build();

            this.Advanced   = await LoadState(Path.Combine(userResourceDir, AdvancedDefaults.FILENAME),   yamlDeserializer, () => new AdvancedDefaults());
            this.Population = await LoadState(Path.Combine(userResourceDir, PopulationDefaults.FILENAME), yamlDeserializer, () => new PopulationDefaults());
            this.Ignore     = await LoadState(Path.Combine(userResourceDir, IgnoreDefaults.FILENAME),     yamlDeserializer, () => new IgnoreDefaults());
            this.Static     = await LoadState(Path.Combine(userResourceDir, StaticValues.FILENAME),       yamlDeserializer, () => new StaticValues());
        }

        public override Task Unload() {
            this.Population = null;
            this.Ignore     = null;
            this.Static     = null;

            return Task.CompletedTask;
        }

        public override async Task Reload() {
            await LoadAllStates();
        }

        public override void Update(GameTime gameTime) { /* NOOP */ }

    }
}
