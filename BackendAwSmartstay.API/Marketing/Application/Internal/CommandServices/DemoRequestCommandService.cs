using BackendAwSmartstay.API.Marketing.Application.Internal.Configuration;
using BackendAwSmartstay.API.Marketing.Application.OutboundServices;
using BackendAwSmartstay.API.Marketing.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Marketing.Domain.Model.Commands;
using BackendAwSmartstay.API.Marketing.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Marketing.Domain.Repositories;
using BackendAwSmartstay.API.Marketing.Domain.Services;
using BackendAwSmartstay.API.Shared.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.Marketing.Application.Internal.CommandServices;

public class DemoRequestCommandService(
    IDemoRequestRepository demoRequestRepository,
    IDemoRequestNotificationService notifications,
    IUnitOfWork unitOfWork,
    IOptions<DemoRequestSettings> settings,
    TimeProvider timeProvider,
    ILogger<DemoRequestCommandService> logger) : IDemoRequestCommandService
{
    public async Task<DemoRequest> Handle(SubmitDemoRequestCommand command)
    {
        var contact = new ContactDetails(command.FirstName, command.LastName, command.Email, command.Phone);
        var request = DemoRequest.Submit(contact, command.HotelName, command.JobTitle, command.AccommodationType,
            command.RoomsRange, command.ReferralSource, command.Profile, command.Message, timeProvider.GetUtcNow());

        await demoRequestRepository.AddAsync(request);
        await unitOfWork.CompleteAsync();

        await notifications.SendConfirmationAsync(request);
        await notifications.NotifySalesTeamAsync(request);
        return request;
    }

    public async Task<int> Handle(SendDemoFollowUpsCommand command)
    {
        var now = timeProvider.GetUtcNow();
        var waiting = await demoRequestRepository.FindWaitingSinceAsync(now - settings.Value.FollowUpAfter);
        var due = waiting.Where(request => request.IsDueForFollowUp(now, settings.Value.FollowUpAfter)).ToList();
        if (due.Count == 0) return 0;

        // Mark first and commit: a second run (or a retry of the cron job) never sends the reminder twice.
        var followedUp = due.Where(request => request.MarkFollowedUp(now)).ToList();
        await unitOfWork.CompleteAsync();

        foreach (var request in followedUp)
            await notifications.SendFollowUpAsync(request);

        logger.LogInformation("Demo follow-up sent to {Count} request(s).", followedUp.Count);
        return followedUp.Count;
    }
}
