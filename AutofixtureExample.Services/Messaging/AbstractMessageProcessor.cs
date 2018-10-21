using AutofixtureExample.Services.Storage;
using AutofixtureExample.Services.Transaction;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutofixtureExample.Services.Messaging
{
    public class AbstractMessageProcessor<T>
    {
        readonly INotificationChannel<T> _notificationChannel;
        readonly IStorageProvider<T> _storageProvider;
        readonly IStorageMessageSerializer _messageSerializer;
        readonly ITransactionTracker<T> _transactionTracker;
        readonly IAsbtractMqFactory<T> _mqFactory;

        public AbstractMessageProcessor(
                    INotificationChannel<T> notificationChannel,
                    IStorageProvider<T> storageProvider,
                    IStorageMessageSerializer messageSerializer,
                    ITransactionTracker<T> transactionTracker,
                    IAsbtractMqFactory<T> mqFactory)
        {
            _notificationChannel = notificationChannel ?? throw new ArgumentNullException(nameof(notificationChannel));
            _storageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));
            _messageSerializer = messageSerializer ?? throw new ArgumentNullException(nameof(messageSerializer));
            _transactionTracker = transactionTracker ?? throw new ArgumentNullException(nameof(transactionTracker));
            _mqFactory = mqFactory ?? throw new ArgumentNullException(nameof(mqFactory));

        }

        public async Task<MessageEnvelope> GetMessages(CancellationToken cancellation)
        {
            // very lame representation of what this method could look like
            var notifications = await _notificationChannel.Receive(cancellation);
            if (!notifications.Any())
                return new MessageEnvelope();

            var transaction = _transactionTracker.CreateTransaction();

            try
            {
                _storageProvider.Store(_messageSerializer, notifications);
                _notificationChannel.Commit(notifications, transaction);
                return new MessageEnvelope("Notifications Commited", transaction);
            }
            catch (Exception ex)
            {
                _notificationChannel.Rollback(notifications, transaction);
                return new MessageEnvelope(ex.Message, transaction);

            }
            
        }
    }

    public class MessageEnvelope
    {
        public MessageEnvelope(
                    string message = null,
                    NotificationTransaction transaction = null)
        {
            Message = message;
            Transaction = transaction;
        }

        public string Message { get; }

        public NotificationTransaction Transaction { get; set; }
    }
}