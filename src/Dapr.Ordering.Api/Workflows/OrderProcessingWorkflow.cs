using Dapr.Ordering.Api.Activities;
using Dapr.Ordering.Api.Entities.Events;
using Dapr.Workflow;

namespace Dapr.Ordering.Api.Workflows;

public record OrderProcessingResult(bool Processed);

public class OrderProcessingWorkflow : Workflow<UserCheckoutAcceptedIntegrationEvent, OrderProcessingResult>
{
    public override async Task<OrderProcessingResult> RunAsync(WorkflowContext context, UserCheckoutAcceptedIntegrationEvent input)
    {
        await context.CallActivityAsync(nameof(NotifyActivity), new Notification("User checkout event accepted, starting order processing workflow."));

        // create order
        OrderResponse? orderResponse = null;
        await SafeInvokeAsync(context, async () =>
        {
            orderResponse = await context.CallActivityAsync<OrderResponse?>(nameof(CreateOrderActivity), new OrderRequest(input));
        });

        // order is null, returning null
        if (orderResponse is null) return new(false);

        // create payment record
        PaymentResponse? paymentResponse = null;
        await SafeInvokeAsync(context, async () =>
        {
            paymentResponse = await context.CallActivityAsync<PaymentResponse?>(nameof(CreatePaymentActivity),
                                                                                          new PaymentRequest(orderResponse!.DTO.OrderId, orderResponse!.DTO.UserId));
        });

        // payment creation failed, returning null
        if (paymentResponse is null) return new(false);

        // set paymentId to the order
        var setPaymentSuccess = await SafeInvokeAsync(context, async () =>
        {
            await context.CallActivityAsync(nameof(UpdateOrderWithPaymentActivity), new UpdateOrderRequest(orderResponse.DTO.OrderId, paymentResponse.PaymentId));
        });

        // update order failed, returning null
        if (!setPaymentSuccess) return new(false);

        // success
        await context.CallActivityAsync(nameof(NotifyActivity), new Notification("Order workflow processing is finished."));
        return new(true);
    }

    private async Task<bool> SafeInvokeAsync(WorkflowContext context, Func<Task> asyncCode)
    {
        try
        {
            await asyncCode();
            return true;
        }
        catch(Exception e)
        {
            await context.CallActivityAsync(nameof(NotifyActivity), new Notification(e.Message));
        }

        return false;
    }
}