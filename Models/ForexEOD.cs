
using MongoDB.Bson.Serialization.Attributes;

namespace b1.Models
{
    [BsonKnownTypes(typeof(ForexEOD))]
    public class ForexEOD : AssetEOD
    {

        public ForexEOD(string shortName,DateTime date, double open, double close, double low, double high) :
         base(shortName,date, open, close, low, high)
        {}
    }
}