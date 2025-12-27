

using SharpCompress.Compressors.Xz;

namespace b1.Main
{
    public interface IProcessAsset
    {
        //two weeks difference measured in days
        public const int TWO_W_D = 14;
        //two months measured in months, I don't know, maybe some asset processor class
        //will use a different date class instead of DateTime, so the steps of taking the EOD data
        //and their units matter because dates are being compared in the context of the implementind class.
        public const int TWO_M_M = 2;
        //same as with months, but with years
        public const int ONE_Y_Y = 1;
        //same as above, again
        public const int FIVE_Y_Y = 5;

        public const string ASSET_EOD_COL = "asseteod";
        //method to get the name of the asset type, serves like a class identifier, this is a stupid method tbh.
        public string AssetTypeName { get; }

        //this method takes a type, because we assume directories are "stocks/.. OR someOtherKind/.."
        public Task Process(string stockName);   
    }
}