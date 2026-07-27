using Riok.Mapperly.Abstractions;
using Transportation.Application.AbsenceNotice.Models;

namespace Transportation.Application.AbsenceNotice;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class AbsenceNoticeMapper
{
    public partial AbsenceNoticeDto Map(Domain.Entities.AbsenceNotice entity);

    public partial List<AbsenceNoticeDto> Map(List<Domain.Entities.AbsenceNotice> entities);
}