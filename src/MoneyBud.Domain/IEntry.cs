namespace MoneyBud.Domain;

/// <summary>
/// Anything recorded in the ledger with an id and a date: an <see cref="Expense"/>, an
/// <see cref="Income"/>, a <see cref="Transfer"/>, a <see cref="BalanceCorrection"/> or a
/// <see cref="Movement"/>.
///
/// <para><b>The five share one id counter, and the id is the order of recording.</b> A backing
/// change draws from it too, without being an entry (<see cref="EntryMark"/>). That order is
/// what decides, on the day of a balance correction, whether an entry dated that day is already in
/// it (arc42 §12, <i>Accounts and net worth</i>). A change keeps the id, so a changed entry keeps
/// the moment it was first recorded.</para>
/// </summary>
public interface IEntry
{
    int Id { get; }

    DateOnly Date { get; }
}
