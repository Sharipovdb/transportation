using Transportation.Domain.Entities;

namespace Transportation.Application.AbsenceNotice.Models;

public sealed class AbsenceNoticeDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public DateOnly Date { get; set; }
    public AbsenceScope Scope { get; set; }
    public AbsenceType Type { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsNotified { get; set; }
}