using System;
using BackendAwSmartstay.API.IAM.Application.Internal.CommandServices;
using BackendAwSmartstay.API.IAM.Application.Internal.Configuration;
using BackendAwSmartstay.API.IAM.Application.OutboundServices;
using BackendAwSmartstay.API.IAM.Domain.Repositories;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.API.IAM.Infrastructure.Hashing.BCrypt.Services;
using BackendAwSmartstay.API.IAM.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.API.IAM.Infrastructure.Tokens.Opaque;
using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.BddTests.Support;

public class IamTestContext
{
    public AppDbContext DbContext { get; }
    public UserRepository UserRepository { get; }
    public RefreshTokenRepository RefreshTokenRepository { get; }
    public AccountTokenRepository AccountTokenRepository { get; }
    public MfaRecoveryCodeRepository MfaRecoveryCodeRepository { get; }
    public UnitOfWork UnitOfWork { get; }
    public HashingService HashingService { get; }
    public SecureTokenGenerator SecureTokenGenerator { get; }
    public FakeTokenService TokenService { get; }
    public FakeAccountNotificationService Notifications { get; }
    public FakeDomainEventDispatcher EventDispatcher { get; }
    public FakeSecretProtector SecretProtector { get; }
    public FakeBreachedPasswordChecker BreachedPasswordChecker { get; }
    public UserScopeService UserScopeService { get; }
    public RoleAuthorizationService RoleAuthorizationService { get; }
    public IOptions<AccountSecuritySettings> SecuritySettings { get; }
    public IOptions<MfaSettings> MfaSettings { get; }
    public AccountTokenIssuer AccountTokenIssuer { get; }
    public SessionIssuer SessionIssuer { get; }
    public NewPasswordValidator NewPasswordValidator { get; }
    public AuthenticationCommandService AuthenticationCommandService { get; }
    public UserCommandService UserCommandService { get; }
    public MfaCommandService MfaCommandService { get; }

    public IamTestContext(AppDbContext dbContext)
    {
        DbContext = dbContext;
        UserRepository = new UserRepository(dbContext);
        RefreshTokenRepository = new RefreshTokenRepository(dbContext);
        AccountTokenRepository = new AccountTokenRepository(dbContext);
        MfaRecoveryCodeRepository = new MfaRecoveryCodeRepository(dbContext);

        HashingService = new HashingService();
        SecureTokenGenerator = new SecureTokenGenerator();
        TokenService = new FakeTokenService();
        Notifications = new FakeAccountNotificationService();
        EventDispatcher = new FakeDomainEventDispatcher();
        SecretProtector = new FakeSecretProtector();
        BreachedPasswordChecker = new FakeBreachedPasswordChecker();

        UnitOfWork = new UnitOfWork(dbContext, EventDispatcher);

        UserScopeService = new UserScopeService();
        RoleAuthorizationService = new RoleAuthorizationService(UserScopeService);

        SecuritySettings = Options.Create(new AccountSecuritySettings());
        MfaSettings = Options.Create(new MfaSettings());

        var timeProvider = TimeProvider.System;

        AccountTokenIssuer = new AccountTokenIssuer(
            AccountTokenRepository,
            SecureTokenGenerator,
            SecuritySettings,
            timeProvider);

        SessionIssuer = new SessionIssuer(
            RefreshTokenRepository,
            SecureTokenGenerator,
            TokenService,
            UnitOfWork,
            SecuritySettings);

        NewPasswordValidator = new NewPasswordValidator(
            BreachedPasswordChecker,
            NullLogger<NewPasswordValidator>.Instance);

        AuthenticationCommandService = new AuthenticationCommandService(
            UserRepository,
            RefreshTokenRepository,
            AccountTokenIssuer,
            TokenService,
            HashingService,
            SecureTokenGenerator,
            RoleAuthorizationService,
            Notifications,
            EventDispatcher,
            UnitOfWork,
            NewPasswordValidator,
            SessionIssuer,
            SecuritySettings,
            timeProvider,
            NullLogger<AuthenticationCommandService>.Instance);

        UserCommandService = new UserCommandService(
            UserRepository,
            RefreshTokenRepository,
            AccountTokenIssuer,
            Notifications,
            timeProvider,
            HashingService,
            RoleAuthorizationService,
            UserScopeService,
            NewPasswordValidator,
            SessionIssuer,
            UnitOfWork);

        MfaCommandService = new MfaCommandService(
            UserRepository,
            MfaRecoveryCodeRepository,
            RefreshTokenRepository,
            RoleAuthorizationService,
            SecretProtector,
            HashingService,
            Notifications,
            SessionIssuer,
            UnitOfWork,
            SecuritySettings,
            MfaSettings,
            timeProvider);
    }
}
