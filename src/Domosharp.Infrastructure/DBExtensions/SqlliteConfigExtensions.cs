using Dapper.FastCrud;

namespace Domosharp.Infrastructure.DBExtensions;

public static class SqlliteConfigExtensions
{
  public static void InitializeMapper()
  {
    OrmConfiguration.DefaultDialect = SqlDialect.SqLite;
  }
}
