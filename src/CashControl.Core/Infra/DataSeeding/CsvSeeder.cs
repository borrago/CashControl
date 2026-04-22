using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace CashControl.Core.Infra.DataSeeding;

[ExcludeFromCodeCoverage]
public abstract class CsvSeeder(string tableName,
    DbContext context,
    Assembly seederAssembly,
    string? pkName = null,
    char? separator = null) : IDatabaseSeeder
{
    private readonly DbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly Assembly _seederAssembly = seederAssembly ?? throw new ArgumentNullException(nameof(seederAssembly));

    private readonly string _pkName = pkName ?? "Id";
    private readonly char _separator = separator ?? ';';
    private readonly string _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var tempTableName = $"#TEMP_{_tableName}";

        var manifest = _seederAssembly.GetManifestResourceNames()
            .FirstOrDefault(s => s.EndsWith($".{_tableName}.csv"));

        if (string.IsNullOrEmpty(manifest)) throw new FileNotFoundException("Arquivo csv não encontrado!");

        var dataTable = new DataTable();

        await using var connection = new SqlConnection(_context.Database.GetConnectionString());
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = $"SELECT TOP 1 * FROM {_tableName}";

        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            dataTable.Load(reader);
        }

        var commandCreateTempTable = connection.CreateCommand();
        commandCreateTempTable.CommandText =
            $"SELECT * INTO {tempTableName} FROM {_tableName} WHERE 1 = 2";
        await commandCreateTempTable.ExecuteNonQueryAsync(cancellationToken);

        List<string> headersCsv;

        using (var bulkCopy = new SqlBulkCopy(connection))
        {
            dataTable.Locale = CultureInfo.GetCultureInfo("pt-BR");
            bulkCopy.DestinationTableName = tempTableName;
            bulkCopy.EnableStreaming = true;
            bulkCopy.BatchSize = 10000;
            bulkCopy.BulkCopyTimeout = 0;
            bulkCopy.NotifyAfter = 100;

            dataTable = AddDataFromCsvFile(dataTable, manifest, out headersCsv);

            await bulkCopy.WriteToServerAsync(dataTable, DataRowState.Added, cancellationToken);
        }

        await MergeTables(tempTableName, dataTable, connection, headersCsv, cancellationToken);
    }

    private async Task MergeTables(
        string tempTableName,
        DataTable dataTable,
        DbConnection connection,
        List<string> headersCsv,
        CancellationToken cancellationToken)
    {
        var colName = headersCsv.Count > 0 ? headersCsv : (from DataColumn column in dataTable.Columns select column.ColumnName).ToList();

        var commandMerge = connection.CreateCommand();
        commandMerge.CommandText =
            @$"MERGE {_tableName} AS Destino
                        USING
                            {tempTableName} AS Origem ON Origem.{_pkName} = Destino.{_pkName}
                        -- Registro existe nas 2 tabelas
                        WHEN MATCHED THEN
                            UPDATE SET
                                {string
                                .Join(',', colName
                                    .Where(c => !c.Equals(_pkName, StringComparison.OrdinalIgnoreCase))
                                    .Select(c => $"Destino.{c} = Origem.{c}").ToArray())}

                        -- Registro não existe no destino.Vamos inserir.
                        WHEN NOT MATCHED THEN
                            INSERT ( {string.Join(',', colName.ToArray())} )
                            VALUES ( {string.Join(',', colName.Select(c => $"Origem.{c}").ToArray())} )
                    ;";
        await commandMerge.ExecuteNonQueryAsync(cancellationToken);
    }

    private DataTable AddDataFromCsvFile(DataTable dataTable, string manifest, out List<string> headersCsv)
    {
        var stream = _seederAssembly.GetManifestResourceStream(manifest);

        if (stream == null)
            throw new InvalidOperationException();

        using var reader = new StreamReader(stream, Encoding.UTF8);

        var i = 0;
        var headers = new Dictionary<int, string>();

        while (!reader.EndOfStream)
        {
            var row = reader.ReadLine();
            if (!string.IsNullOrEmpty(row))
            {
                var rowValues = row.Split(_separator);

                if (i == 0)
                    GenerateHeaders(headers, rowValues);
                else
                    GenerateDataRow(dataTable, headers, rowValues);
            }

            i++;
        }

        headersCsv = headers.Select(p => p.Value).ToList();

        return dataTable;
    }

    private static void GenerateDataRow(DataTable dataTable, IReadOnlyDictionary<int, string> headers, IReadOnlyList<string> rowValues)
    {
        var dr = dataTable.NewRow();
        for (var i = 0; i < rowValues.Count; i++)
            dr[headers[i]] = string.IsNullOrEmpty(rowValues[i]) ? DBNull.Value : rowValues[i];
        dataTable.Rows.Add(dr);
    }

    private static void GenerateHeaders(IDictionary<int, string> headers, IReadOnlyList<string> rowValues)
    {
        for (var i = 0; i < rowValues.Count; i++)
            headers.Add(i, rowValues[i]);
    }
}