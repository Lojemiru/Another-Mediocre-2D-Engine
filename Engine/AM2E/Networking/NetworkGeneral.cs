namespace AM2E.Networking;

public static class NetworkGeneral
{
    public const int MaxGameSequence = 1024;
    public const int HalfMaxGameSequence = MaxGameSequence / 2;
    public const int MAXINPUTS = 32;
    
    public static int SeqDiff(int a, int b)
    {
        return Diff(a, b, HalfMaxGameSequence);
    }
    
    public static int Diff(int a, int b, int halfMax)
    {
        return (a - b + halfMax * 3) % (halfMax * 2) - halfMax;
    }
    
    /*
     Implementation of Modulus that handles negative numbers properly
    */
    public static int Mod(int num, int modulus)
    {
        var r = num % modulus;
        
        return r < 0 ? r + modulus : r;
    }
}