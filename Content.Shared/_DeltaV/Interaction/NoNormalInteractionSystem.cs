using Content.Shared.Interaction.Events;

namespace Content.Shared._DeltaV.Interaction
{
    public sealed class NoNormalInteractionSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<NoNormalInteractionComponent, InteractionAttemptEvent>(OnInteractionAttempt);
        }

        private void OnInteractionAttempt(EntityUid uid, NoNormalInteractionComponent component, InteractionAttemptEvent args)
        {
            args.Cancelled = true;
        }
    }
}
