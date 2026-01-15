using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._starcup.MKC.Components

{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class FluidEjectorComponent : Component
    {
        /// <summary>
        ///     The next time that the fluid ejector will attempt to begin the ejection process.
        /// </summary>
        [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
        public TimeSpan NextUpdate;

        /// <summary>
        ///     The amount of time it takes for fluids to be ejected once detected in the body.
        /// </summary>
        [DataField]
        public TimeSpan EjectionTime = TimeSpan.FromSeconds(15);
    }
}
