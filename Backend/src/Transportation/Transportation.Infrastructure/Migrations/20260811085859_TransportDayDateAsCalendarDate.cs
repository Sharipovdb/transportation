using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transportation.Infrastructure.Migrations
{
    /// <summary>
    /// A working day is a calendar date, not an instant. Storing it as `timestamptz`
    /// meant the API's `ToUniversalTime()` shifted every logged day back by the server's
    /// UTC offset, so a day logged as the 1st was stored — and displayed, and counted
    /// towards the monthly sheet — as the last day of the previous month.
    ///
    /// The conversion reads each stored instant in the server's own time zone, which is
    /// the zone that produced it, so both shapes of existing row recover the date that
    /// was actually entered: local midnight (written by the API) and UTC midnight
    /// (written by the demo seeders).
    /// </summary>
    public partial class TransportDayDateAsCalendarDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE transportation.transport_days
                    ALTER COLUMN "Date" TYPE date
                    USING ("Date" AT TIME ZONE current_setting('TimeZone'))::date;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE transportation.transport_days
                    ALTER COLUMN "Date" TYPE timestamp with time zone
                    USING "Date"::timestamp AT TIME ZONE current_setting('TimeZone');
                """);
        }
    }
}
