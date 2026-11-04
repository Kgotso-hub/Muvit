using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Muvit.Application.Interfaces.Services;
using Muvit.Domain.ValueObjects;

namespace Muvit.Infrastructure.PaymentGateways;

public sealed class PayFastPaymentGateway : IPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly string _merchantId;
    private readonly string _merchantKey;
    private readonly string _passphrase;
    private readonly string _processUrl;
    private readonly string _apiBaseUrl;
    private readonly string _returnUrl;
    private readonly string _cancelUrl;
    private readonly string _notifyUrl;

    public PayFastPaymentGateway(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _merchantId = configuration["PayFast:MerchantId"]!;
        _merchantKey = configuration["PayFast:MerchantKey"]!;
        _passphrase = configuration["PayFast:Passphrase"] ?? string.Empty;
        _processUrl = configuration["PayFast:ProcessUrl"]!;
        _apiBaseUrl = configuration["PayFast:ApiBaseUrl"]!;
        _returnUrl = configuration["PayFast:ReturnUrl"]!;
        _cancelUrl = configuration["PayFast:CancelUrl"]!;
        _notifyUrl = configuration["PayFast:NotifyUrl"]!;
    }

    public Task<PaymentGatewayResult> ChargeAsync(Guid paymentId, Money amount, string paymentMethodToken)
    {
        // paymentMethodToken carries the customer's email address for this gateway —
        // PayFast has no client-side card token step; the customer enters card details
        // on PayFast's own hosted page after being redirected there.
        var fields = new List<KeyValuePair<string, string>>
        {
            new("merchant_id", _merchantId),
            new("merchant_key", _merchantKey),
            new("return_url", _returnUrl),
            new("cancel_url", _cancelUrl),
            new("notify_url", _notifyUrl),
            new("email_address", paymentMethodToken),
            new("m_payment_id", paymentId.ToString()),
            new("amount", amount.Amount.ToString("F2", CultureInfo.InvariantCulture)),
            new("item_name", "Muvit booking payment"),
        };

        var signature = BuildSignature(fields);
        var redirectUrl = $"{_processUrl}?{BuildQueryString(fields)}&signature={signature}";

        // Nothing has actually been charged yet — the customer still has to complete
        // payment on PayFast's hosted page, and the real outcome only arrives later via
        // the ITN webhook (M5). RequiresConfirmation tells PaymentService to leave the
        // Payment at Pending rather than marking it Completed/Failed now.
        return Task.FromResult(new PaymentGatewayResult(false, redirectUrl, null, RequiresConfirmation: true));
    }

    public async Task<PaymentGatewayResult> RefundAsync(string gatewayReference, Money amount)
    {
        // gatewayReference here is expected to be PayFast's own pf_payment_id, captured
        // once the ITN confirms a completed payment — not our own Payment.Id, and not the
        // redirect URL ChargeAsync returned. That capture step is M5 work.
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/refunds/{gatewayReference}")
        {
            Content = JsonContent.Create(new { amount = (int)Math.Round(amount.Amount * 100, MidpointRounding.AwayFromZero) })
        };
        request.Headers.Add("merchant-id", _merchantId);
        request.Headers.Add("version", "v1");
        request.Headers.Add("timestamp", DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:sszzz"));
        // PayFast's REST API also expects a signature header derived from merchant id,
        // passphrase, and timestamp — confirm the exact header name/format against
        // PayFast's own developer docs before relying on this against the real sandbox.

        var response = await _httpClient.SendAsync(request);

        return response.IsSuccessStatusCode
            ? new PaymentGatewayResult(true, gatewayReference, null)
            : new PaymentGatewayResult(false, null, $"PayFast refund failed with status {(int)response.StatusCode}.");
    }

    private string BuildSignature(IEnumerable<KeyValuePair<string, string>> fields)
    {
        var parameterString = BuildQueryString(fields);

        if (!string.IsNullOrEmpty(_passphrase))
            parameterString += $"&passphrase={PayFastEncode(_passphrase)}";

        var hash = MD5.HashData(Encoding.UTF8.GetBytes(parameterString));
        return Convert.ToHexStringLower(hash);
    }

    private static string BuildQueryString(IEnumerable<KeyValuePair<string, string>> fields)
        => string.Join("&", fields
            .Where(f => !string.IsNullOrEmpty(f.Value))
            .Select(f => $"{f.Key}={PayFastEncode(f.Value)}"));

    private static string PayFastEncode(string value)
        => Uri.EscapeDataString(value).Replace("%20", "+");
}