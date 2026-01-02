using System.Collections.Concurrent;
using b1.Main;
using Microsoft.VisualBasic;

namespace b1.Services
{
    public class PriceContextHolder
    {
        private ConcurrentDictionary<string, PriceContext> _contextMap;

        public PriceContextHolder()
        {
            _contextMap = new ConcurrentDictionary<string, PriceContext>();
        }
        internal ICollection<PriceContext> GetAllContexts()
        {
            return _contextMap.Values;
        }
        internal PriceContext? GetContext(string assetType)
        {
            if (_contextMap.TryGetValue(assetType, out var res))
            {
                return res;
            }
            return null;
        }
        internal void AddContext(string assetType, PriceContext ctx)
        {
            if (assetType == null || assetType == "" || ctx == null ||
                 assetType.StartsWith(" "))
            {
                throw new Exception("Bad AddContext arguments, check for proper name and a non null context");
            }
            _contextMap.AddOrUpdate(assetType, ctx, (assetType, prev) => ctx);
        }
    }
}