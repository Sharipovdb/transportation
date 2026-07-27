using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.Application.TaxiExpense;

public static class TaxiExpenseErrors
{
    public static readonly Error NotFound = new(
        "TaxiExpense.NotFound",
        "The specified taxi expense does not exist."
    );

    public static readonly Error AlreadyExists = new(
        "TaxiExpense.AlreadyExists",
        "The specified taxi expense already exists."
    );

    public static readonly Error InvalidEnumValue = new(
        "TaxiExpense.InvalidEnumValue",
        "One of the specified enum values is invalid."
    );

    public static readonly Error ExpenseNotPending = new(
        "TaxiExpense.ExpenseNotPending",
        "The specified taxi expense is not pending."
    );

    public static readonly Error ExpenseAlreadyApproved = new(
        "TaxiExpense.ExpenseAlreadyApproved",
        "The specified taxi expense already approved."
    );

    public static readonly Error ExpenseAlreadyRejected = new(
        "TaxiExpense.ExpenseAlreadyRejected",
        "The specified taxi expense already rejected."
    );

    public static readonly Error ExpenseNotApproved = new(
        "TaxiExpense.ExpenseNotApproved",
        "The specified taxi expense is not approved."
    );

    public static readonly Error ExpenseMustBeApprovedBeforePayment = new(
        "TaxiExpense.ExpenseMustBeApprovedBeforePayment",
        "The taxi expense must be approved before payment."
    );

    public static readonly Error ExpenseThisLegAlreadyExist = new(
        "TaxiExpense.ExpenseThisLegAlreadyExist",
        "The specified taxi expense this leg already exist."
    );

    public static readonly Error ExpensePaidByNonExistentCrewMember = new(
        "TaxiExpense.TaxiExpensePaidByNonExistentCrewMember",
        "The crew member who paid the taxi fare is not a member of the group."
    );
}