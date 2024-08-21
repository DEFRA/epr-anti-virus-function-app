namespace EPR.Antivirus.Application.Extensions;

using System.ComponentModel;
using Data.Enums;

public static class FileTypeExtensions
{
    public static SubmissionSubType? ToSubmissionSubType(this FileType fileType)
    {
        return fileType switch
        {
            FileType.Brands => SubmissionSubType.Brands,
            FileType.Partnerships => SubmissionSubType.Partnerships,
            FileType.CompanyDetails => SubmissionSubType.CompanyDetails,
            FileType.Pom => null,
            FileType.Subsidiaries => null,
            _ => throw new InvalidEnumArgumentException(),
        };
    }
}