// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.AzureServiceBusTransport.Transport
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using Transports.Metrics;
    using Util;


    public class MessageSessionAsyncHandlerFactory :
        IMessageSessionAsyncHandlerFactory
    {
        readonly NamespaceContext _context;
        readonly IPublishEndpointProvider _publishEndpointProvider;
        readonly ISessionReceiver _receiver;
        readonly ISendEndpointProvider _sendEndpointProvider;
        readonly ITaskSupervisor _supervisor;
        readonly IDeliveryTracker _tracker;

        public MessageSessionAsyncHandlerFactory(NamespaceContext context, ITaskSupervisor supervisor, ISessionReceiver receiver, IDeliveryTracker tracker,
            ISendEndpointProvider sendEndpointProvider, IPublishEndpointProvider publishEndpointProvider)
        {
            _context = context;
            _supervisor = supervisor;
            _receiver = receiver;
            _tracker = tracker;
            _sendEndpointProvider = sendEndpointProvider;
            _publishEndpointProvider = publishEndpointProvider;
        }

        public IMessageSessionAsyncHandler CreateInstance(MessageSession session, BrokeredMessage message)
        {
            return new MessageSessionAsyncHandler(_context, _supervisor, _receiver, session, _tracker, _sendEndpointProvider, _publishEndpointProvider);
        }

        public void DisposeInstance(IMessageSessionAsyncHandler handler)
        {
            var disposable = handler as IDisposable;
            disposable?.Dispose();
        }
    }
}