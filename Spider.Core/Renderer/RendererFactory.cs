using System;
using System.Collections.Generic;
using System.Text;

namespace Spider
{
    public static class RendererFactory
    {
        public static IHTMLRenderer DefaultRenderer(ISelectionPolicy selectionPolicy)
        {
            return new StaticHtmlParser(selectionPolicy);
        }
    }
}
