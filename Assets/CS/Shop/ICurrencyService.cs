using System;

public interface ICurrencyService
{
    int Gold { get; }
    bool SpendGold(int amount);
    void AddGold(int amount);
    bool HasGold(int amount);

    event Action<int> OnGoldChanged;
}
