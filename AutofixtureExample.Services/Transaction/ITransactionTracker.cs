using System;
using System.Collections.Generic;
using System.Text;

namespace AutofixtureExample.Services.Transaction
{
    public interface ITransactionTracker<T>
    {
        NotificationTransaction CreateTransaction();
    }

    public class NotificationTransaction
    {

    }
}
