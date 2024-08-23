using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domosharp.Infrastructure.Tests.Fakes
{
  internal static class FakeDBConnectionFactory
  {
    public static string ConnectionString { get => $"Data Source={Guid.NewGuid()};Mode=Memory;Cache=Shared"; }
    public static IDbConnection GetConnection()
    {
      var connection = new SQLiteConnection(ConnectionString);
      connection.Open();
      return connection;
    }
  }
}
