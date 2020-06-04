using System.Collections.Generic;
using System;

namespace StockCheckerII
{
    internal class Evaluator
    {
        public void CalculateAndEvaluate(List<StockEntity> stocks)
        {
            CalculateAndEvaluateThreeTimes(stocks);
        }

        private void CalculateAndEvaluateThreeTimes(List<StockEntity> stocks)
        {
            // Um das Rating über die letzten 3 Jahre gemittelt zu errechnen, muss für das aktuelle Jahr,
            // das Jahr davor und das Vor-Vor-Jahr die Berechnung und Bewertung durchgeführt werden.

            foreach (StockEntity stock in stocks)
            {
                if (stock.GetYearData().Count < 8)
                {
                    stock.AddRemark("Keine Auswertung aufgrund zu weniger Daten");
                    continue;
                }

                // berechne und evaluiere das vorletzte Jahr
                bool result = CalculateAndEvaluateYearBeforeLastYear(stock);
                if (!result)
                {
                    continue;
                }
                int scoreSum = stock.Score;

                // berechne und evaluiere das vorige Jahr
                result = CalculateAndEvaluateLastYear(stock);
                if (!result)
                {
                    continue;
                }
                scoreSum += stock.Score;

                // berechne und evaluiere das aktuelle Jahr
                if (!Calculate(stock)) continue;
                Evaluate(stock);

                // Führe zuletzt die Berechnung des durchschnittl. ratings aus
                stock.Score += scoreSum;
                doRating(stock);
            }
        }

        private void doRating(StockEntity stock)
        {
            if (stock.Score >= 14)
            {
                stock.Rating = StockEntity.Rate.KAUFEN;
                return;
            }
            if (stock.Score >= 6)
            {
                stock.Rating = StockEntity.Rate.HALTEN;
                return;
            }
            stock.Rating = StockEntity.Rate.VERKAUFEN;
        }

        private bool CalculateAndEvaluateLastYear(StockEntity stock)
        {
            // Entferne das letzte Jahr, aber speichere es vorher,
            // um es zum Schluss wieder zurück zu setzen
            StockEntity.YearDataSet dataSetLastYear =
             new StockEntity.YearDataSet(stock.GetYearData()[stock.GetYearData().Count - 1]);
            stock.GetYearData().RemoveAt(stock.GetYearData().Count - 1);

            bool result = Calculate(stock);
            if (result)
            {
                Evaluate(stock);
            }

            stock.GetYearData().Add(dataSetLastYear);

            return result;
        }

        private bool CalculateAndEvaluateYearBeforeLastYear(StockEntity stock)
        {
            // Entferne das letzte als auch das vorletzte Jahr, aber speichere sie vorher,
            // um sie zum Schluss wieder zurück zu setzen
            StockEntity.YearDataSet dataSetLastYear =
             new StockEntity.YearDataSet(stock.GetYearData()[stock.GetYearData().Count - 1]);
            stock.GetYearData().RemoveAt(stock.GetYearData().Count - 1);

            StockEntity.YearDataSet dataSetLastLastYear =
             new StockEntity.YearDataSet(stock.GetYearData()[stock.GetYearData().Count - 1]);
            stock.GetYearData().RemoveAt(stock.GetYearData().Count - 1);

            bool result = Calculate(stock);
            if (result)
            {
                Evaluate(stock);
            }

            stock.GetYearData().Add(dataSetLastYear);
            stock.GetYearData().Add(dataSetLastLastYear);

            return result;
        }

        private void Evaluate(StockEntity stock)
        {
            int score = 0;

            if (stock.EarningGrowthLastYear > 1.0e-6) score++;
            if (stock.EarningCorrelation >= 0.7) score++;
            if (stock.DividendGrowthOneYear > 1.0e-6) score++;
            if (stock.DividendPaidThisYear) score++;
            if (stock.PayoutRatio <= 75) score++;

            stock.Score = score;
        }

        // --- functions for calculating ---------------------------------------

        private bool Calculate(StockEntity stock)
        {
            if (!SetEarningCorrelationFactor(stock)) return false;
            if (!SetEarningAndDividendGrowth(stock)) return false;
            if (!SetPayoutRatio(stock)) return false;

            SetNumYearsDividendNotReduced(stock);

            return true;
        }

        private void SetNumYearsDividendNotReduced(StockEntity stock)
        {
            int numYears = 0;
            for (int i = stock.GetYearData().Count; i > 1; i--)
            {
                if (stock.GetYearData()[i - 1].Dividend >= stock.GetYearData()[i - 2].Dividend)
                {
                    numYears++;
                }
            }
            stock.NumYearsDividendNotReduced = numYears;
        }
        private bool SetPayoutRatio(StockEntity stock)
        {
            // calculates the "cumulated" payout ratio
            if (stock.GetYearData().Count < 3) return false;

            double sumEarnings = stock.GetYearData()[stock.GetYearData().Count - 3].Earning
            + stock.GetYearData()[stock.GetYearData().Count - 2].Earning
            + stock.GetYearData()[stock.GetYearData().Count - 1].Earning;

            double sumDividends = stock.GetYearData()[stock.GetYearData().Count - 3].Dividend
            + stock.GetYearData()[stock.GetYearData().Count - 2].Dividend
            + stock.GetYearData()[stock.GetYearData().Count - 1].Dividend;

            if (sumEarnings > 1.0e-6)
            {
                stock.PayoutRatio = Math.Round(100.0 * sumDividends / sumEarnings, 1);
            }

            return true;
        }

        private bool SetEarningAndDividendGrowth(StockEntity stock)
        {
            if (stock.GetYearData().Count < 6) return false;

            // calculate three years earnings growth
            var x0 = stock.GetYearData()[stock.GetYearData().Count - 4].Earning;
            var x1 = stock.GetYearData()[stock.GetYearData().Count - 1].Earning;
            stock.EarningGrowthTreeYears = CompoundAnnualGrowthRate(x1, x0, 3);

            // calculate one year earnings growth
            x0 = stock.GetYearData()[stock.GetYearData().Count - 2].Earning;
            x1 = stock.GetYearData()[stock.GetYearData().Count - 1].Earning;
            stock.EarningGrowthLastYear = CompoundAnnualGrowthRate(x1, x0, 1);

            // calculate dividend paid in this year
            stock.DividendPaidThisYear = stock.GetYearData()[stock.GetYearData().Count - 1].Dividend > 1.0e-6;

            // calculate five years dividend growth
            x0 = stock.GetYearData()[stock.GetYearData().Count - 6].Dividend;
            x1 = stock.GetYearData()[stock.GetYearData().Count - 1].Dividend;
            stock.DividendGrowthFiveYears = CompoundAnnualGrowthRate(x1, x0, 5);

            // calculate one year dividend growth
            x0 = stock.GetYearData()[stock.GetYearData().Count - 2].Dividend;
            x1 = stock.GetYearData()[stock.GetYearData().Count - 1].Dividend;
            stock.DividendGrowthOneYear = CompoundAnnualGrowthRate(x1, x0, 1);

            return true;
        }

        private bool SetEarningCorrelationFactor(in StockEntity stock)
        {
            double xDach = 0.0;
            double tDach = 0.0;
            double pXTdach = 0.0;
            double dXDachsqrt = 0.0;
            double dTDachsqrt = 0.0;
            double r = -1.0;

            // ---- Prepare and check ---------------------
            var _list = stock.GetYearData();
            if (_list.Count == 0)
            {
                stock.AddRemark("Korrelationsberechnung: Abbruch. Zu wenig Daten");
                return false;
            }
            // --- do calculation -------------------------
            foreach (var item in _list)
            {
                xDach = xDach + item.Earning;
                tDach = tDach + item.Year;
            }
            xDach = xDach / _list.Count;
            tDach = tDach / _list.Count;
            // --------------------------------------------
            foreach (var item in _list)
            {
                var tmp1 = item.Year - tDach;
                var tmp2 = item.Earning - xDach;
                pXTdach = pXTdach + tmp1 * tmp2;
                dXDachsqrt = dXDachsqrt + tmp2 * tmp2;
                dTDachsqrt = dTDachsqrt + tmp1 * tmp1;
            }
            // --------------------------------------------
            if (Math.Sqrt(dXDachsqrt) * Math.Sqrt(dTDachsqrt) > 0.1)
            {
                r = pXTdach / (Math.Sqrt(dXDachsqrt) * Math.Sqrt(dTDachsqrt));
                stock.EarningCorrelation = Math.Round(r, 2);
            }
            return true;
        }

        // compound annual growth rate : CAGR = (EB / BB)^(1 / N) - 1
        // EB = Ending Balance
        // BB = Beginning Balance
        // N = Number of years
        private double CompoundAnnualGrowthRate(double eb, double bb, int n)
        {
            if (Math.Abs(bb) < 0.01 || n < 1)
            {
                return -2;
            }

            double quotient = eb / bb;
            if (quotient < 0.0)
            {
                quotient = (-1 * quotient);
                return -1 * Math.Pow(quotient, 1 / n) - 1;
            }

            double cagr = Math.Pow(quotient, 1.0 / n) - 1.0;
            return Math.Round(cagr * 100.0, 1);
        }
    }
}