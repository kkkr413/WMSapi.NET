using Dapper;
using MySql.Data.MySqlClient;
using WMSapi.Model;
using WMSapi.Models;

namespace WMSapi.Repositories
{
    public interface IORepositories_R
    {
        Task<string> pal_upload(pal_tableDTO pal_table);
        Task<IEnumerable<pal_tableDTO>> Pal_dawnload();
    }

    public class IORepositories : IORepositories_R
    {
        private readonly string connectionString;

        public IORepositories(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("WMSConnection");
        }

        public async Task<string> pal_upload(pal_tableDTO pal_table)
        {
            
            try
            {
                using var connection = new MySqlConnection(connectionString);

                var palsql = "INSERT INTO pal_table " +
                "VALUES (@pal_code, @pal_quantity, @ware_code, @loc_code, @itm_code, @itm_name, now())";
                await connection.QueryAsync<pal_tableDTO>(palsql, pal_table);
            }
            catch (MySqlException ex)
            {
                string errmsg = "";
                if (ex.Number == 1062)
                {
                    // 중복 키 예외 처리
                    errmsg = ("중복된 pal_code가 이미 존재합니다.");
                }
                else if (ex.Number == 1451)
                {
                    // 참조 무결성 제약 조건 예외 처리
                    errmsg = ("참조 무결성 제약 조건에 위배됩니다.");
                }
                else
                {
                    // 그 외의 MySqlException 처리
                    Console.WriteLine("MySQL 예외가 발생했습니다: " + ex.Message);
                    errmsg = ("db 오류가 발생했습니다");
                }
                return errmsg;
            }


            return "OK";
        }

        public async Task<IEnumerable<pal_tableDTO>> Pal_dawnload()
        {    
             using var connection = new MySqlConnection(connectionString);
              var palsql = "SELECT * FROM PAL_TABLE";
              return  await connection.QueryAsync<pal_tableDTO>(palsql);
        }
    }
}
