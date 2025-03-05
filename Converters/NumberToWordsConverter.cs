using System;
using System.Collections.Generic;
using System.Text;

namespace InvoicingApp.Converters
{
    public static class NumberToWordsConverter
    {
        private static readonly string[] units = { "zero", "jeden", "dwa", "trzy", "cztery", "pięć", "sześć", "siedem", "osiem", "dziewięć", "dziesięć", "jedenaście", "dwanaście", "trzynaście", "czternaście", "piętnaście", "szesnaście", "siedemnaście", "osiemnaście", "dziewiętnaście" };
        private static readonly string[] tens = { "", "", "dwadzieścia", "trzydzieści", "czterdzieści", "pięćdziesiąt", "sześćdziesiąt", "siedemdziesiąt", "osiemdziesiąt", "dziewięćdziesiąt" };
        private static readonly string[] hundreds = { "", "sto", "dwieście", "trzysta", "czterysta", "pięćset", "sześćset", "siedemset", "osiemset", "dziewięćset" };
        private static readonly string[] thousands = { "", "tysiąc", "tysiące", "tysięcy" };
        private static readonly string[] millions = { "", "milion", "miliony", "milionów" };

        public static string ConvertToWords(decimal number, string currency = "PLN")
        {
            // Check if the number is zero
            if (number == 0)
                return "zero " + GetCurrencyName(currency, 0);

            // Split into integer and decimal parts
            int integerPart = (int)Math.Floor(number);
            int decimalPart = (int)Math.Round((number - integerPart) * 100);

            // Convert integer part to words
            string result = ConvertIntegerToWords(integerPart);

            // Add currency name
            result += " " + GetCurrencyName(currency, integerPart);

            // Convert decimal part to words if it exists
            if (decimalPart > 0)
            {
                result += " " + decimalPart.ToString("00") + "/100";
            }

            return result;
        }

        private static string ConvertIntegerToWords(int number)
        {
            if (number == 0)
                return "zero";

            StringBuilder result = new StringBuilder();

            // Process millions
            if (number >= 1000000)
            {
                int millionCount = number / 1000000;
                result.Append(ConvertHundreds(millionCount));
                result.Append(" ");
                result.Append(GetPolishForm(millionCount, millions));
                number %= 1000000;

                if (number > 0)
                    result.Append(" ");
            }

            // Process thousands
            if (number >= 1000)
            {
                int thousandCount = number / 1000;

                if (thousandCount > 1)
                    result.Append(ConvertHundreds(thousandCount));

                if (thousandCount > 0)
                {
                    if (thousandCount > 1)
                        result.Append(" ");

                    result.Append(GetPolishForm(thousandCount, thousands));
                }

                number %= 1000;

                if (number > 0)
                    result.Append(" ");
            }

            // Process hundreds, tens and units
            if (number > 0)
                result.Append(ConvertHundreds(number));

            return result.ToString().Trim();
        }

        private static string ConvertHundreds(int number)
        {
            if (number == 0)
                return "";

            StringBuilder result = new StringBuilder();

            // Process hundreds
            if (number >= 100)
            {
                result.Append(hundreds[number / 100]);
                number %= 100;

                if (number > 0)
                    result.Append(" ");
            }

            // Process tens and units
            if (number > 0)
            {
                if (number < 20)
                {
                    result.Append(units[number]);
                }
                else
                {
                    result.Append(tens[number / 10]);

                    if (number % 10 > 0)
                    {
                        result.Append(" ");
                        result.Append(units[number % 10]);
                    }
                }
            }

            return result.ToString();
        }

        private static string GetPolishForm(int number, string[] forms)
        {
            if (number == 1)
                return forms[1];

            if (number % 10 >= 2 && number % 10 <= 4 && (number % 100 < 10 || number % 100 > 20))
                return forms[2];

            return forms[3];
        }

        private static string GetCurrencyName(string currency, int number)
        {
            switch (currency.ToUpper())
            {
                case "PLN":
                    if (number == 1)
                        return "złoty";
                    else if (number % 10 >= 2 && number % 10 <= 4 && (number % 100 < 10 || number % 100 > 20))
                        return "złote";
                    else
                        return "złotych";
                case "EUR":
                    if (number == 1)
                        return "euro";
                    else
                        return "euro";
                case "USD":
                    if (number == 1)
                        return "dolar";
                    else if (number % 10 >= 2 && number % 10 <= 4 && (number % 100 < 10 || number % 100 > 20))
                        return "dolary";
                    else
                        return "dolarów";
                default:
                    return currency;
            }
        }
    }
}