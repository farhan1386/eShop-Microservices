using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.EventBus
{
    public interface IEventBus
    {
        Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
    }
}
