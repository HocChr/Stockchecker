using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System;

namespace StockcheckIIDatabase
{
    internal class SQLDatabase : IDatabase
    {
        SQLiteConnection _sQLiteConnection;
        SQLiteDataAdapter _sQLiteAdapter;

        public SQLDatabase(string fullPath)
        {
            _sQLiteConnection = new SQLiteConnection("Data Source=" + fullPath);
        }
        public List<StockCheckerII.StockEntity> GetStocks()
        {
            List<StockCheckerII.StockEntity> stockList = new List<StockCheckerII.StockEntity>();
            GetTableList(stockList);

            foreach (var stockEntity in stockList)
            {
                GetStockEntity(GetTable(stockEntity.Name), stockEntity);
            }

            return stockList;
        }

        // Konvertiert ein DataTable in ein StockEntity
        private void GetStockEntity(in DataTable table, StockCheckerII.StockEntity entity)
        {
            var yearDataSet = entity.GetYearData();

            foreach (DataRow row in table.Rows)
            {
                StockCheckerII.StockEntity.YearDataSet dataSet = new StockCheckerII.StockEntity.YearDataSet();
                dataSet.Year = Convert.ToInt32(row["year"]);
                dataSet.Earning = Convert.ToDouble(row["earning_per_share"]);
                dataSet.Dividend = Convert.ToDouble(row["div_per_share"]);
                yearDataSet.Add(dataSet);
            }
            entity.SetYearData(yearDataSet);
        }

        private DataTable GetTable(string tablename)
        {
            _sQLiteConnection.Open();
            DataTable table;
            try
            {
                _sQLiteAdapter = new SQLiteDataAdapter("SELECT * FROM [" + tablename + "]", _sQLiteConnection);
                table = new DataTable();
                _sQLiteAdapter.Fill(table);
                new SQLiteCommandBuilder(_sQLiteAdapter);
            }
            catch (System.Exception)
            {
                _sQLiteConnection.Close();
                return null;
            }
            _sQLiteConnection.Close();
            return table;
        }

        private void GetTableList(in List<StockCheckerII.StockEntity> stocks)
        {
            _sQLiteConnection.Open();
            using (DataTable mTables = _sQLiteConnection.GetSchema("Tables"))
            {
                for (int i = 0; i < mTables.Rows.Count; i++)
                {
                    stocks.Add(new StockCheckerII.StockEntity(
                        mTables.Rows[i].ItemArray[mTables.Columns.IndexOf("TABLE_NAME")].ToString()));
                }
            }
            _sQLiteConnection.Close();
        }
    }
}
