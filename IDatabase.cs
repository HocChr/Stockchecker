using System.Collections.Generic;

public interface IDatabase
{
    List<StockCheckerII.StockEntity> GetStocks();
}