namespace MediaNest.Web {
    public static class Utility {
        public static string GenerateSixDigitCode() {
            int hash = BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0);
            int sixDigit = Math.Abs(hash % 1000000);
            return sixDigit.ToString("000000");
        }
    }
}
