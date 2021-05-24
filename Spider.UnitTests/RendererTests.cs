using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Spider.UnitTests
{
    public class RendererTests
    {
        [Fact]
        public void IsValidRenderer()
        {
            var renderer = RendererFactory.DefaultRenderer(PolicyFactory.DefaultSelectionPolicy());
            var childLinks = renderer.FindChildLinks(new UrlQueueItem { Url = "https://www.monzo.com" });
            Assert.NotNull(childLinks);
        }
    }
}
