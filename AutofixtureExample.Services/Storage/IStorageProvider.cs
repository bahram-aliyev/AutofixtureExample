using AutofixtureExample.Services.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutofixtureExample.Services.Storage
{
    public interface IStorageProvider<T>
    {
        void Store(IStorageMessageSerializer serializer, INotification<T>[] notifications);
    }


}
