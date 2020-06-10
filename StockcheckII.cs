using System.Collections.Generic;
using System.Linq;

namespace StockCheckerII
{
    public class Stockcheck
    {
        IDatabase _database;
        public Stockcheck(string path)
        {
            _database = new StockcheckIIDatabase.SQLDatabase(path);
        }

        public List<StockCheckerII.StockEntity> GetStocks()
        {
            var stocks = _database.GetStocks();

            Evaluator evaluator = new Evaluator();
            evaluator.CalculateAndEvaluate(stocks);
            stocks = stocks.OrderByDescending(x => x.Score).ThenByDescending(x => x.DividendGrowthFiveYears).ToList();

            return stocks;
        }
    }
}



