namespace ASAM.MDF.Libary.Types
{
    public enum SignalTypeV4 : byte
    {
        Unsigned_LE,    // Unsigned Integer LE Byte Order
        Unsigned_BE,    // Unsigned Integer BE Byte Order
        Signed_LE,      // Signed Integer LE Byte Order
        Signed_BE,      // Signed Integer BE Byte Order
        Float_LE,       // Float (IEEE 754) LE Byte Order
        Float_BE,       // Float (IEEE 754) BE Byte Order
        String,         // String (ISO-8859-1 Latin), NULL terminated)
        UTF8,           // String (UTF8-encoded), NULL terminated)
        UTF16_LE,       // String (UTF16-LE Byte order), NULL terminated)
        UTF16_BE,       // String (UTF16-BE Byte order), NULL terminated)
        Bytes,          // Byte array
        Sample,         // MIME sample
        Stream,         // MIME stream
        CODate,         // CANOpen Date
        COTime,         // CANOpen Time
        Complex,        // Complex number
    }
}
