using System; 
using BhModule.Community.Pathing.Utility;
using Microsoft.Xna.Framework;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace BhModule.Community.Pathing.State.UserResources.Population.Converters {
    public class ColorConverter : IYamlTypeConverter {

        public bool Accepts(Type type) {
            return type == typeof(Color);
        }

        public object? ReadYaml(IParser parser, Type type) {
            string rawIn = parser.Consume<Scalar>().Value;

            return new ValueOnlyAttribute(rawIn).GetValueAsColor();
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type) {
            string rawOut = ((Color)(value ?? Color.White)).PackedValue.ToString("X");

            emitter.Emit(new Scalar(AnchorName.Empty, TagName.Empty, rawOut));
        }

    }
}
