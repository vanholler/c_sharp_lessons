
//Создайте набор классов для системы управления банковскими 
//счетами.
//Банковские счета содержатся в банке.
//У банковского счета есть уникальный номер и баланс.
//На счет можно положить деньги, можно снять деньги


// number int
// balance int  ( set get ) 
internal class Program
{
    public static void Main(string[] args)
    {
        // создаем банк 
        // счет создаем
        // 2 операции

        var bank = new Bank();
        var userAccount = bank.CreateAccout();

        userAccount.Withdrow(-300);
        userAccount.Withdrow(300);
        userAccount.Deposite(500);
        userAccount.Withdrow(300);
        userAccount.Withdrow(300);

    }
}

internal class Bank
{
    private List<ClientAcccout> _clientAccounts = new();

    public ClientAcccout CreateAccout()
    {
        var account = new ClientAcccout();
        _clientAccounts.Add(account);
        return account;
    }

    // хорошо ещё искать аккаунт по номеру.
}


internal class ClientAcccout
{
    private int _balance; // dec
    private static int _numberAccount;

    public ClientAcccout()
    {
        _numberAccount++;
        NumberAccount = _numberAccount;
    }
    public int Balance
    {
        get { return _balance; }
        set { _balance = value; }
    }
    public int NumberAccount { get; private set; }

    public bool Deposite(int deposite)
    {
        if (deposite < 0)
        {
            Console.WriteLine("Отрицательная сумма!");
            return false;
        }

        Balance += deposite;
        Console.WriteLine($"Деньги добавлены на счет! Ваш остаток: {Balance}");
        return true;
    }


    public bool Withdrow(int withdrow)
    {
        if (withdrow < 0)
        {
            Console.WriteLine("Не верная введенная сумма!");
            return false;
        }
        if (Balance - withdrow < 0)
        {
            Console.WriteLine("Не хватает суммы на балансе!");
            return false;
        }
        Balance -= withdrow;
        Console.WriteLine($"Деньги сняты со счета! Ваш остаток: {Balance}");
        return true;
    }
}