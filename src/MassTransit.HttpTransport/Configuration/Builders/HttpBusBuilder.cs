﻿// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.HttpTransport.Builders
{
    using System;
    using BusConfigurators;
    using Clients;
    using GreenPipes;
    using MassTransit.Builders;
    using MassTransit.Pipeline;
    using MassTransit.Pipeline.Filters;
    using MassTransit.Pipeline.Observables;
    using MassTransit.Pipeline.Pipes;
    using Specifications;
    using Transport;
    using Transports;


    public class HttpBusBuilder :
        BusBuilder
    {
        readonly HttpReceiveEndpointSpecification _busEndpointSpecification;
        readonly BusHostCollection<HttpHost> _hosts;

        public HttpBusBuilder(BusHostCollection<HttpHost> hosts,
            IConsumePipeFactory consumePipeFactory,
            ISendPipeFactory sendPipeFactory,
            IPublishPipeFactory publishPipeFactory)
            : base(consumePipeFactory, sendPipeFactory, publishPipeFactory, hosts)
        {
            _hosts = hosts;

            _busEndpointSpecification = new HttpReceiveEndpointSpecification(_hosts[0], "", ConsumePipe);

            foreach (var host in hosts.Hosts)
            {
                var factory = new HttpReceiveEndpointFactory(this, host);

                host.ReceiveEndpointFactory = factory;
            }
        }

        public BusHostCollection<HttpHost> Hosts => _hosts;

        public override IPublishEndpointProvider PublishEndpointProvider => _busEndpointSpecification.PublishEndpointProvider;

        public override ISendEndpointProvider SendEndpointProvider => _busEndpointSpecification.SendEndpointProvider;

        protected override void PreBuild()
        {
            _busEndpointSpecification.Apply(this);
        }

        protected override Uri GetInputAddress()
        {
            //TODO: Is this the best approach?
            var addy = _busEndpointSpecification.InputAddress;
            var urb = new UriBuilder(addy);
            urb.Scheme = "reply";
            return urb.Uri;
        }

        protected override IConsumePipe GetConsumePipe()
        {
            return CreateConsumePipe();
        }

        protected override ISendTransportProvider CreateSendTransportProvider()
        {
            var receivePipe = CreateReceivePipe();

            return new HttpSendTransportProvider(_hosts, receivePipe, new ReceiveObservable());
        }

        protected IReceivePipe CreateReceivePipe()
        {
            //            AddRescueFilter(builder);

            var endpointBuilder = new HttpReceiveEndpointBuilder(_hosts[0], ConsumePipe, this);

            IPipe<ReceiveContext> receivePipe = Pipe.New<ReceiveContext>(x =>
            {
                x.UseFilter(new DeserializeFilter(endpointBuilder.MessageDeserializer, ConsumePipe));
            });

            return new ReceivePipe(receivePipe, ConsumePipe);
        }
    }
}