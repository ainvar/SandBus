
namespace Ainvar.Bus.InProcess
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    public interface ISandBus
    {
        Task<bool> DispatchAsync(IDispatch dispatch);
        Task<bool> DispatchAsync(IDispatch dispatch, CancellationToken cancellationToken);
        void Dispatch(IDispatch dispatch);
        Guid Subscribe(Action<IDispatch> handlerAction);
        void Unsubscribe(Guid subscriptionId);
    }
}
