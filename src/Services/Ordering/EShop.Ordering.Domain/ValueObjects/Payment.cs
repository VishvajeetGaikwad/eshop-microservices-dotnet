namespace EShop.Ordering.Domain.ValueObjects;

public class Payment
{
    public string CardName { get; private set; } = default!;
    public string CardNumber { get; private set; } = default!;
    public string Expiration { get; private set; } = default!;
    public string Cvv { get; private set; } = default!;
    public int PaymentMethod { get; private set; }

    protected Payment() { }

    private Payment(string cardName, string cardNumber, string expiration, string cvv, int paymentMethod)
    {
        CardName = cardName;
        CardNumber = cardNumber;
        Expiration = expiration;
        Cvv = cvv;
        PaymentMethod = paymentMethod;
    }

    public static Payment Of(string cardName, string cardNumber, string expiration, string cvv, int paymentMethod)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cardName);
        ArgumentException.ThrowIfNullOrWhiteSpace(cardNumber);

        return new Payment(cardName, cardNumber, expiration, cvv, paymentMethod);
    }
}
