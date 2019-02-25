using System;
using Telepathy;

namespace TelepathyLink.Core
{
    public class Class1
    {
        public Class1()
        {
            var server = new Server();
            server.Start(8080);
        }
    }
}
