using Dapr.Client;
using Dapr.Core;
using Dapr.Core.Exceptions;
using Dapr.Core.Extensions;
using Dapr.Workflow;

namespace Dapr.Ordering.Api.Activities;

public record PaymentRequest(Guid OrderId, Guid UserId);
public record PaymentResponse(Guid PaymentId);

public class CreatePaymentActivity : WorkflowActivity<PaymentRequest, PaymentResponse?>
{
    private readonly DaprClient _dapr;
    private readonly ILogger<CreatePaymentActivity> _logger;

    public CreatePaymentActivity(DaprClient dapr, ILogger<CreatePaymentActivity> logger)
    {
        _dapr = dapr;
        _logger = logger;
    }

    public override async Task<PaymentResponse?> RunAsync(WorkflowActivityContext context, PaymentRequest input)
    {
        PaymentResponse? payment = null;
        try
        {
            payment = await _dapr.InvokeMethodAsync<PaymentRequest, PaymentResponse>(HttpMethod.Post,
                                                    DaprConstants.Services.PaymentService,
                                                    "api/payments",
                                                    input);

            if (payment is null)
            {
                throw new InvalidOperationException("Payment is null");
            }

            _logger.LogInformation("Successfully retrieved payment: {PaymentId}", payment.PaymentId);
        }
        catch (InvocationException ex)
        {
            if (ex.TryParseErrorResponse(out var response))
            {
                _logger.LogDaprInvocationError(ex, response);
                return null;
            }

            _logger.LogDaprInvocationError(ex);
            throw new UnexpectedErrorException("Cannot read error response from dapr result");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExceptionMessage}", ex.Message);
            throw new UnexpectedErrorException(ex.Message);
        }

        return payment;
    }
}