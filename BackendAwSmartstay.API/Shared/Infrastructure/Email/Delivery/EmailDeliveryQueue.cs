using System.Threading.Channels;
using BackendAwSmartstay.API.Shared.Application.OutboundServices;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Delivery;

/// <summary>In-process queue between the request that accepts an e-mail and the worker that delivers it.</summary>
public sealed class EmailDeliveryQueue
{
    private readonly Channel<EmailMessage> _channel = Channel.CreateBounded<EmailMessage>(
        new BoundedChannelOptions(1000) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true });

    public ValueTask EnqueueAsync(EmailMessage message, CancellationToken cancellationToken) =>
        _channel.Writer.WriteAsync(message, cancellationToken);

    public IAsyncEnumerable<EmailMessage> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
