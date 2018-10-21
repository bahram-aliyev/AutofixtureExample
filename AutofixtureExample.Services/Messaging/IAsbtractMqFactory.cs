using System;
using System.Collections.Generic;
using System.Text;

namespace AutofixtureExample.Services.Messaging
{
    public interface IAsbtractMqFactory<T>
    {
        void CreateTransaction();
    }
}
