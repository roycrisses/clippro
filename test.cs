using System;
public class Test {
    public void M(int m) {
        Span<int> s = m < 256 ? stackalloc int[m] : new int[m];
    }
}
