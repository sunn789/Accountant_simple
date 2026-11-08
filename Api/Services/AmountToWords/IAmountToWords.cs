namespace Api.Services.AmountToWords;

public interface IAmountToWords
{
    string Convert(decimal amount, string currency = "ریال");
}