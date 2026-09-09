using BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using FluentAssertions;
using Xunit;

namespace BackendAwSmartstay.API.Tests.Profiles.Domain.ValueObjects;

public class ValueObjectsTests
{
    [Fact]
    public void InternalStronglyTypedIds_WhenInstantiated_ShouldRetainGuidValue()
    {
        // Arrange
        var raw = Guid.NewGuid();

        // Act
        var guestId = new GuestProfileId(raw);
        var staffId = new StaffProfileId(raw);
        var assignmentId = new AssignmentId(raw);

        // Assert
        guestId.Value.Should().Be(raw);
        staffId.Value.Should().Be(raw);
        assignmentId.Value.Should().Be(raw);
    }

    [Fact]
    public void ExternalReferences_WhenInstantiated_ShouldRetainIntValue()
    {
        // Arrange
        var rawUser = 42;
        var rawTarget = 101;

        // Act
        var userId = new UserId(rawUser);
        var targetId = new TargetId(rawTarget);

        // Assert
        userId.Value.Should().Be(rawUser);
        targetId.Value.Should().Be(rawTarget);
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("87654321")]
    public void IdentificationDocument_WithValidDni_ShouldInstantiate(string number)
    {
        // Act
        var doc = new IdentificationDocument(DocumentType.Dni, number);

        // Assert
        doc.Type.Should().Be(DocumentType.Dni);
        doc.Number.Should().Be(number);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1234567")]      // 7 dígitos
    [InlineData("123456789")]    // 9 dígitos
    [InlineData("1234567A")]    // No numérico
    public void IdentificationDocument_WithInvalidDni_ShouldThrowArgumentException(string number)
    {
        // Act
        var act = () => new IdentificationDocument(DocumentType.Dni, number);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("ABC12345")]
    [InlineData("P9876543")]
    public void IdentificationDocument_WithValidPassport_ShouldInstantiate(string number)
    {
        // Act
        var doc = new IdentificationDocument(DocumentType.Passport, number);

        // Assert
        doc.Type.Should().Be(DocumentType.Passport);
        doc.Number.Should().Be(number);
    }

    [Theory]
    [InlineData("12345")]           // Menor a 6
    [InlineData("1234567890123")]   // Mayor a 12
    public void IdentificationDocument_WithInvalidPassportLength_ShouldThrowArgumentException(string number)
    {
        // Act
        var act = () => new IdentificationDocument(DocumentType.Passport, number);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("+51987654321")]
    [InlineData("987654321")]
    [InlineData("+15551234567")]
    public void PhoneNumber_WithValidFormat_ShouldInstantiate(string phone)
    {
        // Act
        var vo = new PhoneNumber(phone);

        // Assert
        vo.Value.Should().Be(phone);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("phone123")]
    [InlineData("12345")]
    public void PhoneNumber_WithInvalidFormat_ShouldThrowArgumentException(string phone)
    {
        // Act
        var act = () => new PhoneNumber(phone);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("EMP-00001")]
    [InlineData("emp-00104")]
    public void EmployeeCode_WithValidFormat_ShouldNormalizeAndInstantiate(string code)
    {
        // Act
        var vo = new EmployeeCode(code);

        // Assert
        vo.Value.Should().Be(code.Trim().ToUpperInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("EMP-1")]
    [InlineData("STF-00001")]
    [InlineData("EMP-ABCDE")]
    public void EmployeeCode_WithInvalidFormat_ShouldThrowArgumentException(string code)
    {
        // Act
        var act = () => new EmployeeCode(code);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("Recepcionista")]
    [InlineData("  Jefe de Operaciones  ")]
    public void JobPosition_WithValidLength_ShouldTrimAndInstantiate(string position)
    {
        // Act
        var vo = new JobPosition(position);

        // Assert
        vo.Value.Should().Be(position.Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ab")] // Menor a 3 caracteres
    public void JobPosition_WithInvalidLength_ShouldThrowArgumentException(string position)
    {
        // Act
        var act = () => new JobPosition(position);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DateRange_WhenEndDateIsEarlierThanStartDate_ShouldThrowArgumentException()
    {
        // Arrange
        var start = new DateOnly(2026, 9, 10);
        var end = new DateOnly(2026, 9, 5);

        // Act
        var act = () => new DateRange(start, end);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DateRange_WhenEndDateIsEqualToStartDate_ShouldInstantiate()
    {
        // Arrange
        var date = new DateOnly(2026, 9, 10);

        // Act
        var range = new DateRange(date, date);

        // Assert
        range.StartDate.Should().Be(date);
        range.EndDate.Should().Be(date);
        range.Includes(date).Should().BeTrue();
    }

    [Fact]
    public void PersonName_WithValidData_ShouldTrimAndExposeFullName()
    {
        // Act
        var name = new PersonName("  Rosa  ", "  Perez  ");

        // Assert
        name.FirstName.Should().Be("Rosa");
        name.LastName.Should().Be("Perez");
        name.FullName.Should().Be("Rosa Perez");
    }

    [Theory]
    [InlineData("", "Perez")]
    [InlineData("Rosa", "")]
    public void PersonName_WithEmptyValues_ShouldThrowArgumentException(string first, string last)
    {
        // Act
        var act = () => new PersonName(first, last);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EmailAddress_WithValidData_ShouldNormalizeToLowercase()
    {
        // Act
        var email = new EmailAddress("  Test.User@SMARTSTAY.COM  ");

        // Assert
        email.Address.Should().Be("test.user@smartstay.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("user@domain")]
    public void EmailAddress_WithInvalidFormat_ShouldThrowArgumentException(string address)
    {
        // Act
        var act = () => new EmailAddress(address);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
