using System.Globalization;

namespace Api.Services.AmountToWords;

public class PersianAmountToWords : IAmountToWords
{
    private static readonly string[] Units =
    {
        "صفر", "یک", "دو", "سه", "چهار",
        "پنج", "شش", "هفت", "هشت", "نه",
        "ده", "یازده", "دوازده", "سیزده", "چهارده",
        "پانزده", "شانزده", "هفده", "هجده", "نوزده"
    };

    private static readonly string[] Tens =
    {
        "", "", "بیست", "سی", "چهل",
        "پنجاه", "شصت", "هفتاد", "هشتاد", "نود"
    };

    private static readonly string[] Hundreds =
    {
        "", "یکصد", "دویست", "سیصد", "چهارصد",
        "پانصد", "ششصد", "هفتصد", "هشتصد", "نهصد"
    };

    private static readonly string[] Scales =
    {
        "",
        "هزار",
        "میلیون",
        "میلیارد",
        "تریلیون",
        "کوادریلیون"
    };

    public string Convert(decimal amount, string currency = "ریال")
    {
        if (amount < 0)
            return "منفی " + Convert(Math.Abs(amount), currency);

        var integerPart = decimal.Truncate(amount);

        if (integerPart > 999_999_999_999_999_999m)
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "مبلغ خارج از محدوده قابل تبدیل است."
            );

        var number = (long)integerPart;

        var result = ConvertInteger(number);

        return $"{result} {currency}".Trim();
    }

    private static string ConvertInteger(long number)
    {
        if (number == 0)
            return Units[0];

        var parts = new List<string>();
        var scaleIndex = 0;

        while (number > 0)
        {
            var group = (int)(number % 1000);

            if (group > 0)
            {
                var groupText = ConvertGroup(group);

                if (scaleIndex > 0)
                    groupText += " " + Scales[scaleIndex];

                parts.Insert(0, groupText);
            }

            number /= 1000;
            scaleIndex++;
        }

        return string.Join(" و ", parts);
    }

    private static string ConvertGroup(int number)
    {
        var parts = new List<string>();

        var hundred = number / 100;
        var remainder = number % 100;

        if (hundred > 0)
            parts.Add(Hundreds[hundred]);

        if (remainder > 0)
        {
            if (remainder < 20)
            {
                parts.Add(Units[remainder]);
            }
            else
            {
                var ten = remainder / 10;
                var unit = remainder % 10;

                parts.Add(Tens[ten]);

                if (unit > 0)
                    parts.Add(Units[unit]);
            }
        }

        return string.Join(" و ", parts);
    }
}