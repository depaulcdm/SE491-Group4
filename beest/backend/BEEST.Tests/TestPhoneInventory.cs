namespace BEEST.Tests;

internal static class TestPhoneInventory
{
    public static PhoneInventory CreateDefault()
    {
        var englishPhonesPath = Path.GetTempFileName();
        var spanishPhonesPath = Path.GetTempFileName();

        File.WriteAllLines(englishPhonesPath, new[]
        {
            "AE\tvowel",
            "AH\tvowel",
            "EH\tvowel",
            "ER\tvowel",
            "IH\tvowel",
            "IY\tvowel",
            "OW\tvowel",
            "HH\taspirate",
            "K\tstop",
            "L\tliquid",
            "T\tstop",
            "S\tfricative",
            "W\tstop",
            "D\tstop",
            "NG\tnasal",
        });

        File.WriteAllLines(spanishPhonesPath, new[]
        {
            "A\tvowel",
            "O\tvowel",
            "U\tvowel",
            "G\tstop",
            "K\tstop",
            "L\tliquid",
            "M\tnasal",
            "N\tnasal",
            "D\tstop",
            "T\tstop",
        });

        return new PhoneInventory(englishPhonesPath, spanishPhonesPath);
    }
}
