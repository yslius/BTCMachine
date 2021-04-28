namespace BTCMachine
{
    public enum ErrorType
    {
        Success,
        InsufficientBalance,
        TimeOut,
        CommunicationError,
        Pending,
        InvalidAddress,
        InvalidPrice,
        NoAccount,
        ConfigFileNotFound,
        UpLimit,
        DownLimit,
        TempUnavailable,
        AccountRestrict,
        IllegalComment,
        MerchantAccount,
        TransferImpossible,
        AnotherTransferProgressing,
        LowAmount,
        InvalidAccountInfo,
        InvalidParam,
        UnknownError,
    }
}
