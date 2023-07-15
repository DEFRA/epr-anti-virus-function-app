namespace EPR.Antivirus.Application.Tests.Extensions;

using System.ComponentModel;
using Application.Extensions;
using Data.Enums;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class FileTypeExtensionsTests
{
    [TestMethod]
    [DataRow(FileType.Pom, null)]
    [DataRow(FileType.CompanyDetails, SubmissionSubType.CompanyDetails)]
    [DataRow(FileType.Brands, SubmissionSubType.Brands)]
    [DataRow(FileType.Partnerships, SubmissionSubType.Partnerships)]
    public void ToSubmissionSubType_ReturnsCorrectMapping(FileType fileType, SubmissionSubType? submissionSubType)
    {
        // Arrange

        // Act
        var result = fileType.ToSubmissionSubType();

        // Assert
        result.Should().Be(submissionSubType);
    }

    [TestMethod]
    public void ToSubmissionSubType_WhenUnknownFileType_ThrowsNotImplementedException()
    {
        // Arrange
        const FileType fileType = (FileType)100;

        var act = () => fileType.ToSubmissionSubType();

        // Act

        // Assert
        act.Should().ThrowExactly<InvalidEnumArgumentException>();
    }
}