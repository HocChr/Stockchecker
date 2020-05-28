using System.Collections.Generic;

namespace StockCheckerII
{
    public class StockEntity
    {
        List<YearDataSet> _yearData = new List<YearDataSet>();

        public enum Rate
        {
            A,
            B,
            C
        }

        public class YearDataSet
        {
            public double Dividend { get; set; }
            public double Earning { get; set; }
            public int Year { get; set; }
        }

        public Rate Rating { get; set; }
        public string Name { get; set; }
        public double EarningCorrelation { get; set; }
        public double EarningGrowthTreeYears { get; set; }
        public double EarningGrowthLastYear{ get; set; }
        public double PayoutRatio{ get; set; }
        internal List<YearDataSet> YearData
        {
            get
            {
                _yearData.Sort((x, y) => x.Year.CompareTo(y.Year));
                return _yearData;
            }

            set
            {
                _yearData = value;
                _yearData.Sort((x, y) => x.Year.CompareTo(y.Year));
            }
        }
    }
}