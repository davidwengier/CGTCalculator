namespace CGTCalculator;

internal record Transaction(int Id, DateOnly Date, TransactionType Type, decimal Quantity, decimal Amount);
