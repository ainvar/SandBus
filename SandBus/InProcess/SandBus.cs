
namespace Ainvar.Bus.InProcess
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    public class SandBus : ISandBus
    {
        private int maxDegreeOfParallelism = 8;

        private readonly bool _saveHistory;
        protected ConcurrentDictionary<Guid, Stack<IDispatch>> dispatches = new ConcurrentDictionary<Guid, Stack<IDispatch>>();
        private readonly ConcurrentDictionary<Guid, IDisposable> _subscriptions = new ConcurrentDictionary<Guid, IDisposable>();

        private readonly BroadcastBlock<IDispatch> _broadcast = new BroadcastBlock<IDispatch>(dispatch => dispatch);

        private Int64 _dCount = 0;

        static SandBus()
        {

        }

        private SandBus(bool saveHistory = false)
        {
            _saveHistory = saveHistory;
        }

        public static SandBus Instance { get; } = new SandBus();

        public Int64 DispatchesCount
        {
            get
            {
                return _dCount;
            }
        }

        public async Task<bool> DispatchAsync(IDispatch dispatch, CancellationToken cancellationToken)
        {
            _dCount++;
            dispacthAll();
            saveHistory(dispatch);
            return await _broadcast.SendAsync(dispatch, cancellationToken);
        }

        public async Task<bool> DispatchAsync(IDispatch dispatch)
        {
            _dCount++;
            dispacthAll();
            saveHistory(dispatch);
            return await _broadcast.SendAsync(dispatch);
        }

        public void DispatchAndForget(IDispatch dispatch)
        {
            _dCount++;
            dispacthAll();
            saveHistory(dispatch);
            _broadcast.SendAsync(dispatch);
        }

        public void Dispatch(IDispatch dispatch)
        {
            _dCount++;
            dispacthAll();
            saveHistory(dispatch);

            _broadcast.Post(dispatch);
        }

        public Guid Subscribe(Action<IDispatch> handlerAction)
        {
            var handler = new ActionBlock<IDispatch>(
            dispatch => handlerAction(dispatch),
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }
            );

            var subscription = _broadcast.LinkTo(
            handler,
            new DataflowLinkOptions { PropagateCompletion = true },
            dispatch => dispatch is IDispatch
            );

            return addSubscription(subscription);
        }

        public IDisposable SubscribeAndGetSubscription(Action<IDispatch> handlerAction)
        {
            var handler = new ActionBlock<IDispatch>(
            dispatch => handlerAction(dispatch),
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }
            );

            var subscription = _broadcast.LinkTo(
            handler,
            new DataflowLinkOptions { PropagateCompletion = true },
            dispatch => dispatch is IDispatch
            );

            return subscription;
        }

        public List<Guid> SubscribeActionByType(IDictionary<Type, Action<IDispatch>> behaviours)
        {
            List<Guid> guidSubscriptions = new List<Guid>();
            foreach (var type in behaviours.Keys)
            {
                var handler = new ActionBlock<IDispatch>(
                dispatch => behaviours[type](dispatch),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }
                );

                var subscription = _broadcast.LinkTo(
                handler,
                new DataflowLinkOptions { PropagateCompletion = true },
                dispatch => dispatch.DispatchOwnerType == type
                );

                guidSubscriptions.Add(addSubscription(subscription));
            }

            return guidSubscriptions;
        }

        public List<Guid> SubscribeActionByOwnerGuid(IDictionary<Guid, Action<IDispatch>> behaviours)
        {
            List<Guid> guidSubscriptions = new List<Guid>();
            foreach (var guid in behaviours.Keys)
            {
                var handler = new ActionBlock<IDispatch>(
                dispatch => behaviours[guid](dispatch),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }
                );

                var subscription = _broadcast.LinkTo(
                handler,
                new DataflowLinkOptions { PropagateCompletion = true },
                dispatch => dispatch.DispatchOwner == guid
                );

                guidSubscriptions.Add(addSubscription(subscription, guid));
            }

            return guidSubscriptions;
        }

        public Guid SubscribeActionByOwnerGuid(Guid guid, Action<IDispatch> behaviour)
        {
            var handler = new ActionBlock<IDispatch>(
            dispatch => behaviour(dispatch),
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }
            );

            var subscription = _broadcast.LinkTo(
            handler,
            new DataflowLinkOptions { PropagateCompletion = true },
            dispatch => dispatch.DispatchOwner == guid
            );

            return addSubscription(subscription, guid);

        }

        public void SubscribeActionByOwnerGuidS(ICollection<Guid> guids, Action<IDispatch> behaviour)
        {
            var handler = new ActionBlock<IDispatch>(
            dispatch => behaviour(dispatch),
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }
            );

            foreach (var guid in guids)
            {
                var subscription = _broadcast.LinkTo(
                handler,
                new DataflowLinkOptions { PropagateCompletion = true },
                dispatch => dispatch.DispatchOwner == guid
                );

                addSubscription(subscription, guid);
            }

        }

        public void Unsubscribe(Guid subscriptionId)
        {
            IDisposable subscription;
            if (_subscriptions.TryRemove(subscriptionId, out subscription))
            {
                subscription.Dispose();
            }
        }

        public void Unsubscribe(params Guid[] subscriptionIds)
        {
            IDisposable subscription;
            foreach (var id in subscriptionIds)
            {
                if (_subscriptions.TryRemove(id, out subscription))
                {
                    subscription.Dispose();
                }
            }
        }

        public void ResetSubscriptions()
        {
            _subscriptions.Clear();
        }

        private Guid addSubscription(IDisposable subscription)
        {
            var subscriptionId = Guid.NewGuid();
            _subscriptions.TryAdd(subscriptionId, subscription);
            return subscriptionId;
        }

        private Guid addSubscription(IDisposable subscription, Guid key)
        {
            _subscriptions.TryAdd(key, subscription);
            return key;
        }

        private void saveHistory(IDispatch dispatch)
        {
            if (_saveHistory)
                dispatches.GetOrAdd(dispatch.DispatchOwner, new Stack<IDispatch>().PushAndReturn<IDispatch>(dispatch));
        }

        private void dispacthAll()
        {
            if (_dCount % 500000 == 0)
                _broadcast.Receive();
        }

        public IObservable<IDispatch> GetDispatching()
        {
            return _broadcast.AsObservable<IDispatch>();
        }

        public void Receive()
        {
            _broadcast.Receive();
        }
    }

    public enum ContentType
    {
        SQL,
        TXT,
        JSON
    }

    public static class StackTools
    {
        public static Stack<T> PushAndReturn<T>(this Stack<T> myStack, T toPush)
        {
            myStack.Push(toPush);
            return myStack;
        }
    }
}
