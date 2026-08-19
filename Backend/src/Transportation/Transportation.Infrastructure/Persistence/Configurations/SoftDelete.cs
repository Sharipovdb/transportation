namespace Transportation.Infrastructure.Persistence.Configurations;

internal static class SoftDelete
{
    /// <summary>
    /// Index filter that leaves soft-deleted rows out of a uniqueness rule.
    ///
    /// Nothing is really removed from this database — rows are flagged instead — so an
    /// unfiltered unique index keeps enforcing the name, plate or period of a record the
    /// user has already deleted, and re-creating it fails with a constraint violation.
    /// </summary>
    public const string NotDeleted = """
                                     "IsDeleted" = false
                                     """;
}
