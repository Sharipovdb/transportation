using AutoFixture;
using AutoFixture.AutoNSubstitute;

namespace Transportation.UnitTests.Utils;
public static class FixtureExtensions
{
    public static IFixture WithAutoNSubstitutions(this IFixture fixture)
        => fixture.Customize(new AutoNSubstituteCustomization());

    public static IFixture WithAutoNSubstitutionsAutoPopulatedProperties(this IFixture fixture)
        => fixture.Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
}