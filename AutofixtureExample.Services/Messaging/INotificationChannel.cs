using System.Threading;
using System.Threading.Tasks;
using AutofixtureExample.Services.Transaction;

namespace AutofixtureExample.Services.Messaging
{
    public interface INotificationChannel<T>
    {
        Task<INotification<T>[]> Receive(CancellationToken cancellation);

        void Rollback(INotification<T>[] notifications, NotificationTransaction tr);

        void Commit(INotification<T>[] notifications, NotificationTransaction tr);

    }
}
