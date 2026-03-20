

using System.Data;
using Dapper;

namespace MfpDashboard.Data;

/// <summary>
/// Teaches Dapper how to read/write System.DateOnly to/from PostgreSQL DATE columns.
/// Register once at startup via SqlMapper.AddTypeHandler(new DateOnlyTypeHandler()).
/// </summary>
public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value  = value.ToDateTime(TimeOnly.MinValue);
    }

    public override DateOnly Parse(object value) => value switch
    {
        DateTime dt   => DateOnly.FromDateTime(dt),
        DateOnly d    => d,
        _             => DateOnly.FromDateTime(Convert.ToDateTime(value))
    };
}