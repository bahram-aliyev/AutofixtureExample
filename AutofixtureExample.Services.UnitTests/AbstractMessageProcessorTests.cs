using AutoFixture;
using AutoFixture.Xunit2;
using AutofixtureExample.Services.Messaging;
using AutofixtureExample.Services.Storage;
using AutofixtureExample.Services.Transaction;
using AutofixtureExample.TestUtil;
using FakeItEasy;
using FluentAssertions;
using SemanticComparison.Fluent;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AutofixtureExample.Services.UnitTests.AbstractMessageProcessorTests
{
    #region Customizations

    public class AbstractMessageProcessorCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var cancellation = new CancellationToken();
            fixture.Inject(cancellation);

            var notifications = fixture.CreateMany<INotification<long>>().ToArray();
            fixture.Inject(notifications);

            var notificationChannel = fixture.Freeze<INotificationChannel<long>>();
            A.CallTo(() => notificationChannel.Receive(cancellation))
                .Returns(notifications);

            var tr = fixture.Create<NotificationTransaction>();
            fixture.Inject(tr);
            var tracker = fixture.Freeze<ITransactionTracker<long>>();
            A.CallTo(() => tracker.CreateTransaction())
                .Returns(tr);
        }
    }

    #endregion

    public class WhenGettingMessages
    {
        [Theory, AutoFakeItEasyData(typeof(AbstractMessageProcessorCustomization))]
        public async Task ItShouldStoreTheExpectedMessagesWhenNoExceptionIsThrown(
                    [Frozen] IStorageProvider<long> storageProvider,
                    [Frozen] IStorageMessageSerializer serializer,
                    CancellationToken cancellation,
                    NotificationTransaction expectedTransaction,
                    INotification<long>[] expectedNotifications,
                    INotificationChannel<long> channel,
                    AbstractMessageProcessor<long> sut)
        {
            // Arrange

            // Act
            await sut.GetMessages(cancellation);

            // Assert
            A.CallTo(() => storageProvider.Store(serializer, expectedNotifications)).MustHaveHappened(Repeated.Exactly.Once)
                .Then(A.CallTo(() => channel.Commit(expectedNotifications, expectedTransaction)).MustHaveHappened(Repeated.Exactly.Once));
        }

        [Theory, AutoFakeItEasyData(typeof(AbstractMessageProcessorCustomization))]
        public async Task ItShouldReturnTheExpectedResultWhenNoExceptionIsThrown(
                    NotificationTransaction expectedTransaction,
                    AbstractMessageProcessor<long> sut)
        {
            // Arrange
            var expected =
                    new MessageEnvelope("Notifications Commited", expectedTransaction)
                    .AsSource()
                    .OfLikeness<MessageEnvelope>()
                    .CreateProxy();

            // Act
            var actual = await sut.GetMessages(CancellationToken.None);

            // Assert
            expected.Should().Be(actual);
        }

        [Theory, AutoFakeItEasyData(typeof(AbstractMessageProcessorCustomization))]
        public async Task ItShouldRollbackTheExpectedMessagesWhenAnExceptionIsThrown(
                    [Frozen]IStorageProvider<long> storageProvider,
                    NotificationTransaction expectedTransaction,
                    INotification<long>[] expectedNotifications,
                    INotificationChannel<long> channel,
                    AbstractMessageProcessor<long> sut)
        {
            // Arrange
            A.CallTo(() => storageProvider.Store(
                                A<IStorageMessageSerializer>._,
                                A<INotification<long>[]>._))
                .Throws<Exception>();

            // Act
            await sut.GetMessages(CancellationToken.None);

            // Assert
            A.CallTo(() => channel.Commit(A<INotification<long>[]>._, A<NotificationTransaction>._)).MustNotHaveHappened();
            A.CallTo(() => channel.Rollback(expectedNotifications, expectedTransaction)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Theory, AutoFakeItEasyData(typeof(AbstractMessageProcessorCustomization))]
        public async Task ItShouldReturnTheExpectedResultWhenAnExceptionIsThrown(
                    [Frozen]IStorageProvider<long> storageProvider,
                    NotificationTransaction expectedTransaction,
                    Exception exception,
                    AbstractMessageProcessor<long> sut)
        {
            // Arrange
            A.CallTo(() => storageProvider.Store(
                                A<IStorageMessageSerializer>._,
                                A<INotification<long>[]>._))
                .Throws(exception);

            var likeness =
                    new MessageEnvelope(exception.Message, expectedTransaction)
                    .AsSource()
                    .OfLikeness<MessageEnvelope>();

            // Act
            var actual = await sut.GetMessages(CancellationToken.None);

            // Assert
            likeness.ShouldEqual(actual);
        }

        [Theory, AutoFakeItEasyData(typeof(AbstractMessageProcessorCustomization))]
        public async Task ItShouldNotCommitNotificationsIfReceivedAnEmptyCollectionOfNotifications(
                    INotificationChannel<long> channel,
                    AbstractMessageProcessor<long> sut)
        {
            // Arrange
            A.CallTo(() => channel.Receive(A<CancellationToken>._))
                .Returns(Enumerable.Empty<INotification<long>>().ToArray());

            // Act
            await sut.GetMessages(CancellationToken.None);

            // Assert
            A.CallTo(() => channel.Commit(A<INotification<long>[]>._, A<NotificationTransaction>._))
                .MustNotHaveHappened();
        }

        [Theory, AutoFakeItEasyData(typeof(AbstractMessageProcessorCustomization))]
        public async Task ItShouldReturnTheExpectedResultsIfReceivedAnEmptyCollectionOfNotifications(
                    INotificationChannel<long> channel,
                    AbstractMessageProcessor<long> sut)
        {
            // Arrange
            A.CallTo(() => channel.Receive(A<CancellationToken>._))
                .Returns(Enumerable.Empty<INotification<long>>().ToArray());

            var likeness =
                    new MessageEnvelope(null, null)
                    .AsSource()
                    .OfLikeness<MessageEnvelope>();

            // Act
            var actual = await sut.GetMessages(CancellationToken.None);

            // Assert
            likeness.ShouldEqual(actual);
        }
    }
}
