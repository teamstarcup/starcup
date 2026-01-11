using Robust.Shared.Prototypes;

namespace Content.Shared.Bible.Components
{
    // starcup: made shared
    [RegisterComponent]
    public sealed partial class BibleUserComponent : Component
    {
        /// <summary>
        /// starcup: If the bible user has sanctified an item, they may only be able to sanctify
        /// another item of the same prototype should the original ever be destroyed.
        /// </summary>
        [DataField]
        public EntProtoId? SanctifiedArchetype;
    }
}
