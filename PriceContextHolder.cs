using System.Collections.Concurrent;
using b1.Main;
using Microsoft.VisualBasic;

namespace b1.Main
{
    public sealed class PriceContextHolder
    {
        private static PriceContextHolder _instance = new PriceContextHolder();
        private ConcurrentDictionary<string, PriceContext> ContextMap;
        static PriceContextHolder(){}
        private PriceContextHolder()
        {
            ContextMap = new ConcurrentDictionary<string, PriceContext>();
        }
        public static PriceContextHolder GetInstance()
        {
            return _instance;
        }
        internal ICollection<PriceContext> GetAllContexts()
        {
            return ContextMap.Values;
        }
        internal PriceContext? GetContext(string assetType)
        {
            if (ContextMap.TryGetValue(assetType, out var res))
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
            ContextMap.AddOrUpdate(assetType, ctx, (assetType, prev) => ctx);
        }
    }
}