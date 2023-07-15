namespace EPR.Antivirus.Data.Enums;

public enum ScanResult
{
    AwaitingProcessing = 1,
    Success = 2,
    FileInaccessible = 3,
    Quarantined = 4,
    FailedToVirusScan = 5,
}