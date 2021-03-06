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
                doRating(stock);
            }
        }

        private void doRating(StockEntity stock)
        {
            if (stock.Percentage >= 80.0)
            {
                stock.Rating = StockEntity.Rate.A;
                return;
            }
            else if (stock.Percentage >= 50.0)
            {
                stock.Rating = StockEntity.Rate.B;
                return;
            }
            stock.Rating = StockEntity.Rate.C;
        }

        private void Evaluate(StockEntity stock)
        {
            double percentage = 0;

            percentage = PercentageOfEarningCorrelation(stock);
            percentage += PercentageOfEarningGrowth(stock);
            percentage += PercentageOfDividendGrowth(stock);
            percentage += PercentageOfDividendYearsNotCutted(stock);
            percentage += PercentageOfPayoutRatio(stock);

            stock.Percentage = Math.Round(100.0 * percentage / 5.0, 1);
        }

        private double PercentageOfEarningCorrelation(StockEntity stock)
        {
            double minimalCorrelation = 0.0;
            double maximalCorrelation = 1.0;

            return CalcPercentage(minimalCorrelation, maximalCorrelation, stock.EarningCorrelation);
        }

        private double PercentageOfEarningGrowth(StockEntity stock)
        {
            double minimalGrowth = 0.0;
            double maximalGrowth = 5.0;

            return CalcPercentage(minimalGrowth, maximalGrowth, stock.EarningGrowthTreeYears);
        }

        private double PercentageOfDividendGrowth(StockEntity stock)
        {
            double minimalGrowth = 0.0;
            double maximalGrowth = 5.0;

            return CalcPercentage(minimalGrowth, maximalGrowth, stock.DividendGrowthThreeYears);
        }

        private double PercentageOfDividendYearsNotCutted(StockEntity stock)
        {
            int minimalYears = 0;
            int maximalYears = 10;

            return CalcPercentage((double)minimalYears, (double)maximalYears,
             (double)stock.NumYearsDividendNotReduced);
        }

        private double PercentageOfPayoutRatio(StockEntity stock)
        {
            var percentage0 = CalcPercentage(0.0, 50.0, stock.PayoutRatio);
            var percentage1 = 1.0 - CalcPercentage(50.0, 100.0, stock.PayoutRatio);

            var best = Math.Min(percentage0, percentage1);
            return best;
        }

        private double CalcPercentage(double minExpected, double maxExpected, double actualValue)
        {
            if (actualValue <= minExpected)
            {
                return 0.0;
            }
            else if (actualValue >= maxExpected)
            {
                return 1.0;
            }
            else if ((maxExpected - minExpected) < 1.0e-6)
            {
                return 0.0;
            }
            else
            {
                double m = 1.0 / (maxExpected - minExpected);
                var debug = m * actualValue - m * minExpected;
                return m * actualValue - m * minExpected;
            }
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
            for (int i = 1; i < stock.GetYearData().Count; i++)
            {
                if (stock.GetYearData()[i - 1].Dividend <= stock.GetYearData()[i].Dividend)
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

            // calculate three years dividend growth
            x0 = stock.GetYearData()[stock.GetYearData().Count - 4].Dividend;
            x1 = stock.GetYearData()[stock.GetYearData().Count - 1].Dividend;
            stock.DividendGrowthThreeYears = CompoundAnnualGrowthRate(x1, x0, 3);

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
            if (Math.Abs(bb) < 1.0e-6 || n < 1)
            {
                return Math.Round(-2.0);
            }

            double quotient = eb / bb;
            if (quotient < 0.0)
            {
                quotient = (-1.0 * quotient);
                return Math.Round(-1.0 * Math.Pow(quotient, 1.0 / n) - 1.0, 1);
            }

            double cagr = Math.Pow(quotient, 1.0 / n) - 1.0;
            return Math.Round(cagr * 100.0, 1);
        }
    }
}