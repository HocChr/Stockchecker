using System.Collections.Generic;

namespace StockCheckerII
{
    public class StockEntity
    {
        List<YearDataSet> _yearData = new List<YearDataSet>();
        string _name;

        string _remarks = "";

        Rate _rating = Rate.C;

        public StockEntity(string name)
        {
            _name = name;
        }

        public enum Rate
        {
            A,
            B,
            C
        }

        public class YearDataSet
        {
            public YearDataSet() { }
            public YearDataSet(YearDataSet other)
            {
                Dividend = other.Dividend;
                Earning = other.Earning;
                Year = other.Year;
            }
            public double Dividend { get; set; }
            public double Earning { get; set; }
            public int Year { get; set; }
        }

        public Rate Rating
        {
            get { return _rating; }
            set { _rating = value; }
        }
        public string Name
        {
            get
            {
                // Return the actual name if it is not null.
                return this._name ?? string.Empty;
            }
            set
            {
                // Set the employee name field.
                this._name = value;
            }
        }

        public int Score { get; set; }
        public double EarningCorrelation { get; set; }
        public double EarningGrowthTreeYears { get; set; }
        public double EarningGrowthLastYear { get; set; }
        public int NumYearsDividendNotReduced { get; set; }
        public double DividendGrowthFiveYears { get; set; }
        public double DividendGrowthOneYear { get; set; }
        public double PayoutRatio { get; set; }

        public List<YearDataSet> GetYearData()
        {
            _yearData.Sort((x, y) => x.Year.CompareTo(y.Year));
            return _yearData;
        }
        public void SetYearData(List<YearDataSet> value)
        {
            _yearData = value;
            _yearData.Sort((x, y) => x.Year.CompareTo(y.Year));
        }
        public void AddRemark(string remark)
        {
            _remarks += remark + "; ";
        }

        public string GetRemarks() => _remarks;
    }
}