using TmfLib.Prototype;

namespace BhModule.Community.Pathing.State.UserResources.Population.Converters {
    public readonly struct ValueOnlyAttribute : IAttribute {

        public string Name  => string.Empty;
        public string Value { get; }

        public ValueOnlyAttribute(string value) {
            this.Value = value;
        }

    }
}
