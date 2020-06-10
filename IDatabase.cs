using System.Collections.Generic;

internal interface IDatabase
{
    List<StockCheckerII.StockEntity> GetStocks();
}