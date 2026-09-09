using BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Events;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using FluentAssertions;
using Xunit;

namespace BackendAwSmartstay.API.Tests.Profiles.Domain.Aggregates;

public class GuestProfileTests
{
    private readonly PersonName _validName = new("Carlos", "Mendoza");
    private readonly PhoneNumber _validPhone = new("+51987654321");
    private readonly EmailAddress _validEmail = new("carlos@gmail.com");
    private readonly IdentificationDocument _validDni = new(DocumentType.Dni, "12345678");

    [Fact]
    public void Create_WithOnlyRequiredFields_ShouldHaveNullOptionalsAndEmitCreatedEvent()
    {
        // Act
        var guest = new GuestProfile(GuestProfileId.New(), _validName, _validPhone);

        // Assert
        guest.Id.Value.Should().NotBeEmpty();
        guest.UserId.Should().BeNull();
        guest.Email.Should().BeNull();
        guest.Document.Should().BeNull();
        guest.Address.Should().BeNull();
        guest.Status.Should().Be(ProfileStatus.Active);
        guest.DomainEvents.Should().ContainSingle(e => e is GuestProfileCreatedEvent);
    }

    [Fact]
    public void LinkToUser_WhenGuestLacksEmail_ShouldAssignVerifiedEmailAndUserId()
    {
        // Arrange
        var guest = new GuestProfile(GuestProfileId.New(), _validName, _validPhone, email: null);
        var userId = new UserId(1);
        var verifiedEmail = new EmailAddress("carlos.verified@gmail.com");

        // Act
        guest.LinkToUser(userId, verifiedEmail);

        // Assert
        guest.UserId.Should().Be(userId);
        guest.Email.Should().Be(verifiedEmail);
        guest.DomainEvents.Should().ContainSingle(e => e is GuestLinkedToUserEvent);
    }

    [Fact]
    public void LinkToUser_WhenGuestAlreadyHasEmail_ShouldPreserveExistingEmail()
    {
        // Arrange
        var initialEmail = new EmailAddress("carlos.original@gmail.com");
        var guest = new GuestProfile(GuestProfileId.New(), _validName, _validPhone, email: initialEmail);
        var userId = new UserId(1);
        var verifiedEmail = new EmailAddress("carlos.newiam@gmail.com");

        // Act
        guest.LinkToUser(userId, verifiedEmail);

        // Assert
        guest.UserId.Should().Be(userId);
        guest.Email.Should().Be(initialEmail);
    }

    [Fact]
    public void LinkToUser_WhenAlreadyLinked_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var initialUserId = new UserId(1);
        var guest = new GuestProfile(GuestProfileId.New(), _validName, _validPhone, _validEmail, userId: initialUserId);

        // Act
        var act = () => guest.LinkToUser(new UserId(2), new EmailAddress("new@gmail.com"));

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already linked*");
    }

    [Fact]
    public void LinkToUser_WhenProfileIsInactive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var guest = new GuestProfile(GuestProfileId.New(), _validName, _validPhone);
        guest.Deactivate();

        // Act
        var act = () => guest.LinkToUser(new UserId(1), _validEmail);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*inactive*");
    }

    [Fact]
    public void SetIdentification_WhenNoDocumentExists_ShouldSetDocument()
    {
        // Arrange
        var guest = new GuestProfile(GuestProfileId.New(), _validName, _validPhone);

        // Act
        guest.SetIdentification(_validDni);

        // Assert
        guest.Document.Should().Be(_validDni);
    }

    [Fact]
    public void SetIdentification_WhenDocumentAlreadyExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var guest = new GuestProfile(GuestProfileId.New(), _validName, _validPhone, document: _validDni);
        var passport = new IdentificationDocument(DocumentType.Passport, "ABC12345");

        // Act
        var act = () => guest.SetIdentification(passport);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already set*");
    }

    [Fact]
    public void CorrectIdentification_WithValidReasonAndStaffId_ShouldUpdateDocumentAndEmitEvent()
    {
        // Arrange
        var guest = new GuestProfile(GuestProfileId.New(), _validName, _validPhone, document: _validDni);
        var newDocument = new IdentificationDocument(DocumentType.Dni, "87654321");
        var staffUserId = new UserId(99);

        // Act
        guest.CorrectIdentification(newDocument, "Error en dígito verificado", staffUserId);

        // Assert
        guest.Document.Should().Be(newDocument);
        guest.DomainEvents.Should().ContainSingle(e => e is GuestIdentificationCorrectedEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CorrectIdentification_WithEmptyReason_ShouldThrowArgumentException(string reason)
    {
        // Arrange
        var guest = new GuestProfile(GuestProfileId.New(), _validName, _validPhone, document: _validDni);
        var newDocument = new IdentificationDocument(DocumentType.Dni, "87654321");

        // Act
        var act = () => guest.CorrectIdentification(newDocument, reason, new UserId(99));

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CorrectIdentification_WithoutPriorDocument_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var guest = new GuestProfile(GuestProfileId.New(), _validName, _validPhone, document: null);
        var newDocument = new IdentificationDocument(DocumentType.Dni, "87654321");

        // Act
        var act = () => guest.CorrectIdentification(newDocument, "Motivo válido", new UserId(99));

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*SetIdentification*");
    }

    [Fact]
    public void UpdateContactInformation_WhenInactive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var guest = new GuestProfile(GuestProfileId.New(), _validName, _validPhone);
        guest.Deactivate();

        // Act
        var act = () => guest.UpdateContactInformation(new PhoneNumber("+51911111111"), null);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*inactive*");
    }

    [Fact]
    public void DeactivateAndActivate_ShouldToggleStatusCorrectly()
    {
        // Arrange
        var guest = new GuestProfile(GuestProfileId.New(), _validName, _validPhone);

        // Act & Assert
        guest.Deactivate();
        guest.Status.Should().Be(ProfileStatus.Inactive);

        guest.Activate();
        guest.Status.Should().Be(ProfileStatus.Active);
    }
}
