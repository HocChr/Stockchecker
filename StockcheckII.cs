using System;
using System.Collections.Generic;
using System.Linq;


namespace StockCheckerII
{
    public class Stockcheck
    {
        IDatabase _database;
        IGui _gui;

        public Stockcheck(IDatabase database, IGui gui)
        {
            _database = database;
            _gui = gui;
        }

        public bool Run()
        {
            var stocks = _database.GetStocks();

            Evaluator evaluator = new Evaluator();
            evaluator.CalculateAndEvaluate(stocks);

            stocks = stocks.OrderByDescending(x => x.Score).ThenByDescending(x => x.DividendGrowthFiveYears).ToList();
            _gui.SetStocks(stocks);

            return true;
        }
    }
}



