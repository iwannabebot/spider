namespace Spider
{
    public interface ISelectionPolicy
    {
        bool CrossDomain { get; set; }
        bool CanIGoThere(UrlItem from, string to);
    }

    public class SelectionPolicy : ISelectionPolicy
    {
        public bool CrossDomain { get; set; }

        public bool CanIGoThere(UrlItem from, string to)
        {
            if (CrossDomain)
                return true;
            else
            {
                if (!(to.StartsWith($"https://{from.Host}") || to.StartsWith($"http://{from.Host}") || to.StartsWith($"//{from.Host}") || to.StartsWith($"{from.Host}")))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
