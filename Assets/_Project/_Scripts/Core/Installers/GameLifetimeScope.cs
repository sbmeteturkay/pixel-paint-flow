using MessagePipe;
using PaintFlow.Features.ItemRequester;
using PaintFlow.Features.Level;
using PaintFlow.Features.QueueLane;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PaintFlow.Core.Installers
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private LevelLoader _levelLoader;
        [SerializeField] private QueueLaneFeature _queueLaneFeature;
        [SerializeField] private ThrownItemBuffer _thrownItemBuffer;
        [SerializeField] private ItemRequesterFeature _itemRequesterFeature;

        protected override void Configure(IContainerBuilder builder)
        {
            MessagePipeOptions options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<QueueLaneItemPoppedEvent>(options);

            RegisterIfNotNull(builder, _levelLoader);
            RegisterIfNotNull(builder, _queueLaneFeature);
            RegisterIfNotNull(builder, _thrownItemBuffer);
            RegisterIfNotNull(builder, _itemRequesterFeature);
        }

        private static void RegisterIfNotNull<T>(IContainerBuilder builder, T component)
            where T : Component
        {
            if (component != null)
            {
                builder.RegisterComponent(component);
            }
        }
    }
}
