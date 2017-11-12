using SchemaExplorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaExplorerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string connString = "Server=192.168.2.10;User Id=sa;Password=abc123!@#;Database=SchemaExplorerTest;Min Pool Size=10;Max Pool Size=50;";
            SqlAzureSchemaProvider provider = new SqlAzureSchemaProvider();
            DatabaseSchema dbSchema = new DatabaseSchema(provider, connString);
            //dbSchema.DeepLoad = true;
            var tables = provider.GetTables(connString, dbSchema);

            foreach (TableSchema item in tables)
            {
                Console.WriteLine($"{item.Name}-{item.Description}");
            }

            Console.ReadKey();

           
            //SqlSchemaProvider provider = new SqlSchemaProvider();
            //DatabaseSchema dbSchema = new DatabaseSchema(provider, connString);
            //var tables = provider.GetTables(connString, dbSchema);

            //foreach (TableSchema item in tables)
            //{
            //    Console.WriteLine($"{item.Name}-{item.Description}");
            //}

            //Console.ReadKey();
        }
    }
}
