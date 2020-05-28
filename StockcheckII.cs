using System;

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

            _gui.SetStocks(stocks);

            Console.WriteLine("Hello Test!");

            return true;
        }
    }
}



