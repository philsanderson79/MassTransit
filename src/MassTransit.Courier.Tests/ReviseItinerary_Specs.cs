﻿// Copyright 2007-2014 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.Courier.Tests
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using NUnit.Framework;
    using Shouldly;
    using TestFramework;
    using Testing;


    [TestFixture]
    public class When_an_itinerary_is_revised :
        ActivityTestFixture
    {
        [Test]
        public async void Should_complete_the_additional_item()
        {
            Guid trackingNumber = Guid.NewGuid();

            ActivityTestContext testActivity = GetActivityContext<TestActivity>();
            ActivityTestContext reviseActivity = GetActivityContext<ReviseItineraryActivity>();

            Task<ConsumeContext<RoutingSlipCompleted>> completed =
                SubscribeHandler<RoutingSlipCompleted>(context => (context.Message.TrackingNumber == trackingNumber));

            Task<ConsumeContext<RoutingSlipActivityCompleted>> testActivityCompleted = SubscribeHandler<RoutingSlipActivityCompleted>(
                context => context.Message.TrackingNumber == trackingNumber && context.Message.ActivityName.Equals(testActivity.Name));

            Task<ConsumeContext<RoutingSlipActivityCompleted>> reviseActivityCompleted = SubscribeHandler<RoutingSlipActivityCompleted>(
                context => context.Message.TrackingNumber == trackingNumber && context.Message.ActivityName.Equals(reviseActivity.Name));

            var builder = new RoutingSlipBuilder(trackingNumber);
            builder.AddActivity(reviseActivity.Name, reviseActivity.ExecuteUri, new
            {
                Value = "Time to add a new item!",
            });

            await Bus.Execute(builder.Build());

            await completed;
            await testActivityCompleted;
            await reviseActivityCompleted;
        }

        [Test]
        public async void Should_continue_with_the_source_itinerary()
        {
            Guid trackingNumber = Guid.NewGuid();

            ActivityTestContext testActivity = GetActivityContext<TestActivity>();
            ActivityTestContext reviseActivity = GetActivityContext<ReviseItineraryActivity>();

            Task<ConsumeContext<RoutingSlipCompleted>> completed =
                SubscribeHandler<RoutingSlipCompleted>(context => (context.Message.TrackingNumber == trackingNumber));

            Task<ConsumeContext<RoutingSlipActivityCompleted>> testActivityCompleted = SubscribeHandler<RoutingSlipActivityCompleted>(
                context => context.Message.TrackingNumber == trackingNumber && context.Message.ActivityName.Equals(testActivity.Name));

            Task<ConsumeContext<RoutingSlipActivityCompleted>> reviseActivityCompleted = SubscribeHandler<RoutingSlipActivityCompleted>(
                context => context.Message.TrackingNumber == trackingNumber && context.Message.ActivityName.Equals(reviseActivity.Name));

            var builder = new RoutingSlipBuilder(trackingNumber);
            builder.AddActivity(reviseActivity.Name, reviseActivity.ExecuteUri, new
            {
                Value = "Time to add a new item!",
            });

            await Bus.Execute(builder.Build());

            await completed;
            await reviseActivityCompleted;
            ConsumeContext<RoutingSlipActivityCompleted> testActivityResult = await testActivityCompleted;

            testActivityResult.Message.GetArgument<string>("Value").ShouldBe("Added");
        }

        [Test]
        public async void Should_immediately_complete_an_empty_list()
        {
            Guid trackingNumber = Guid.NewGuid();

            ActivityTestContext testActivity = GetActivityContext<TestActivity>();
            ActivityTestContext reviseActivity = GetActivityContext<ReviseToEmptyItineraryActivity>();

            Task<ConsumeContext<RoutingSlipCompleted>> completed =
                SubscribeHandler<RoutingSlipCompleted>(context => (context.Message.TrackingNumber == trackingNumber));

            Task<ConsumeContext<RoutingSlipActivityCompleted>> testActivityCompleted = SubscribeHandler<RoutingSlipActivityCompleted>(
                context => context.Message.TrackingNumber == trackingNumber && context.Message.ActivityName.Equals(testActivity.Name));

            Task<ConsumeContext<RoutingSlipActivityCompleted>> reviseActivityCompleted = SubscribeHandler<RoutingSlipActivityCompleted>(
                context => context.Message.TrackingNumber == trackingNumber && context.Message.ActivityName.Equals(reviseActivity.Name));

            var builder = new RoutingSlipBuilder(trackingNumber);
            builder.AddActivity(reviseActivity.Name, reviseActivity.ExecuteUri, new
            {
                Value = "Time to remove any remaining items!",
            });
            builder.AddActivity(testActivity.Name, testActivity.ExecuteUri, new
            {
                Value = "Hello",
            });

            await Bus.Execute(builder.Build());

            await completed;
            await reviseActivityCompleted;

            testActivityCompleted.Wait(TimeSpan.FromSeconds(3)).ShouldBe(false);
        }

        protected override void SetupActivities()
        {
            AddActivityContext<TestActivity, TestArguments, TestLog>(() => new TestActivity());
            AddActivityContext<ReviseToEmptyItineraryActivity, TestArguments, TestLog>(
                () => new ReviseToEmptyItineraryActivity());
            AddActivityContext<ReviseWithNoChangeItineraryActivity, TestArguments, TestLog>(
                () => new ReviseWithNoChangeItineraryActivity());

            ActivityTestContext testActivity = GetActivityContext<TestActivity>();
            AddActivityContext<ReviseItineraryActivity, TestArguments, TestLog>(
                () =>
                    new ReviseItineraryActivity(
                        x => x.AddActivity(testActivity.Name, testActivity.ExecuteUri, new {Value = "Added"})));
        }
    }
}