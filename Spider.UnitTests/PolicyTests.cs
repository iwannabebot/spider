using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Spider.UnitTests
{
    public class PolicyTests
    {
        [Fact]
        public void SelectionPolicy()
        {
            var shouldBeTrue = PolicyFactory.DefaultSelectionPolicy().CanIGoThere(new UrlItem
            {
                Url = "http://www.google.com"
            }, "www.google.com/mylink");
            var shouldBeFalse = PolicyFactory.DefaultSelectionPolicy().CanIGoThere(new UrlItem
            {
                Url = "http://www.google.com"
            }, "www.facebook.com/mylink");
            Assert.True(shouldBeTrue);
            Assert.False(shouldBeFalse);
            shouldBeTrue = PolicyFactory.GetSelectionPolicy(true).CanIGoThere(new UrlItem
            {
                Url = "http://www.google.com"
            }, "www.facebook.com/mylink");
            Assert.True(shouldBeTrue);
        }

        [Fact]
        public void PolitenessPolicy()
        {
            var politenessPolicy = PolicyFactory.DefaultPolitenessPolicy();
            var shouldBeTrue = politenessPolicy.CanIGoThere(new UrlItem
            {
                Url = "http://www.google.com"
            }, out long _);
            var shouldBeFalse = politenessPolicy.CanIGoThere(new UrlItem
            {
                Url = "http://www.google.com/local/place/reviews/sasas"
            }, out long _);
            Assert.True(shouldBeTrue);
            Assert.False(shouldBeFalse);
        }
    }
}
