namespace HttpStack.FastCGI;

internal static class Constants
{
    public const int MaxContentLength = 65535;

    public const int MaxFrameSize =
        1 + // Version
        1 + // Type
        2 + // RequestId
        2 + // ContentLength
        1 + // PaddingLength
        1 + // Reserved
        MaxContentLength + // Content
        255; // Padding

    public static class Flags
    {
        public const int KeepConnection = 1;
    }

    public static class Types
    {
        public const byte BeginRequest = 1;
        public const byte AbortRequest = 2;
        public const byte EndRequest = 3;
        public const byte Params = 4;
        public const byte Stdin = 5;
        public const byte Stdout = 6;
        public const byte Stderr = 7;
        public const byte Data = 8;
        public const byte GetValues = 9;
        public const byte GetValuesResult = 10;
        public const byte UnknownType = 11;
    }

    public static class ProtocolStatus
    {
        public const byte RequestComplete = 0;
        public const byte CantMultiplexConnection = 1;
        public const byte Overloaded = 2;
        public const byte UnknownRole = 3;
    }
}
