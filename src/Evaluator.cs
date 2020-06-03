using System.Collections.Generic;
using System;

namespace StockCheckerII
{
    internal class Evaluator
    {
        public void CalculateAndEvaluate(List<StockEntity> stocks)
        {
            foreach (StockEntity stock in stocks)
            {
                if (!Calculate(stock)) continue;
                Evaluate(stock);
            }
        }

        private void Evaluate(StockEntity stock)
        {
            int score = 0;

            if(stock.EarningGrowthTreeYears > 1.0e-6) score++;
            if(stock.EarningGrowthLastYear > 1.0e-6) score++;
            if(stock.EarningCorrelation >= 0.7) score++;
            if(stock.DividendGrowthOneYear > 1.0e-6) score++;
            if(stock.NumYearsDividendNotReduced >= 10 ||
             stock.NumYearsDividendNotReduced == stock.GetYearData().Count) score++;

            stock.Score = score;
        }

        // --- functions for calculating ---------------------------------------

        private bool Calculate(StockEntity stock)
        {
            if (!CheckBeforeCalculation(stock)) return false;
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
            if (stock.GetYearData().Count < 5) return false;

            // calculate three years earnings growth
            var x0 = stock.GetYearData()[stock.GetYearData().Count - 4].Earning;
            var x1 = stock.GetYearData()[stock.GetYearData().Count - 1].Earning;
            stock.EarningGrowthTreeYears = CompoundAnnualGrowthRate(x1, x0, 3);

            // calculate one year earnings growth
            x0 = stock.GetYearData()[stock.GetYearData().Count - 2].Earning;
            x1 = stock.GetYearData()[stock.GetYearData().Count - 1].Earning;
            stock.EarningGrowthLastYear = CompoundAnnualGrowthRate(x1, x0, 1);

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

        // returns true is Data is valid. adds Remark otherwise.
        private bool CheckBeforeCalculation(StockEntity stock)
        {
            if (stock.GetYearData().Count < 8)
            {
                stock.AddRemark("Keine Auswertung aufgrund zu weniger Daten");
                return false;
            }

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