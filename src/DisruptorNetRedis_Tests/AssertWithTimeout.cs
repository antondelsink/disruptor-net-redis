using System;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DisruptorNetRedis.Tests
{
    internal static class AssertWithTimeout
    {
        internal static void IsTrue(Func<bool> conditional, string msgTimedOut)
        {
            IsTrue(conditional, msgTimedOut, TimeSpan.FromMilliseconds(10));
        }

        internal static void IsTrue(Func<bool> conditional, string msgTimedOut, TimeSpan timeout)
        {
            if (!SpinWait.SpinUntil(conditional, timeout))
            {
                Assert.Fail(msgTimedOut);
            }
        }
    }
}