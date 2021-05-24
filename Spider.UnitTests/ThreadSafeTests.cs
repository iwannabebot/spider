using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Spider.UnitTests
{
    public class ThreadSafeTests
    {
        [Fact]
        public void NotNullCheck()
        {
            Assert.NotNull(ThreadSafeFactory.PosionQueue());
            Assert.NotNull(ThreadSafeFactory.UrlQueue());
            Assert.NotNull(ThreadSafeFactory.FrontQueue);
            Assert.NotNull(ThreadSafeFactory.BackUrlQueue);
            Assert.NotNull(ThreadSafeFactory.BackPosionQueue);
            Assert.NotNull(ThreadSafeFactory.BackQueue);
        }
    }
}
