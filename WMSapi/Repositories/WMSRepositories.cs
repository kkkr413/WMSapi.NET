using MySql.Data.MySqlClient;
using WMSapi.Model;
using Dapper;
using MySqlX.XDevAPI.Common;
using WMSapi.Models;

namespace WMSapi.Repositories
{


    public interface WMSRepositories_R// 분리패턴사용
    {
        Task<IEnumerable<pal_tableDTO>> SELE_PAL(string itm_code ,string st_data , string ed_data);
        Task<IEnumerable<object>> PAL_LOG();
        Task<IEnumerable<pal_tableDTO>> LOCregistration(string itm_code);
        Task<IEnumerable<ID_PASS_DTO>> LOGINserchID(string ID);
        Task<string> CRATE_ID(string userid, string pas, string us_name);
        Task<string> serch_WH_NAME(string warehouse_code);
        Task<string> loc_codecrate(string WH_NAME);
        Task<string> loc_crate(loc_CrateDTO Params);
        Task<string> loc_del(string loc_code);// ?
        Task<string> loc_move(string pal_code, string where_move_loc);
        Task<string> ware_move(string pal_code, string where_move);
        Task<string> palNOcrate();
        Task<string> Manualregistration(ManualregistrationDTO DTOparams);
        Task<string> inventory_adj(string pal_code, string how_many);
        Task<IEnumerable<object>> all_qal(string itm_code);
        Task<IEnumerable<object>> Salestatus(string st_data, string ed_data);
        Task<int> SELECT_NOW_quantity(string PAL_CODE);
        Task<string> salesNOcrate();
        Task<string> salesINSERT(salesINSERTDTO DTOparams);
        Task<bool> LOGSET(string pal_code, string WHERE_MOVE, string reason);
    }



    public class WMSRepositories : WMSRepositories_R
    {
        private readonly string connectionString;

        public WMSRepositories(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("WMSConnection");
        }

       //test public string connectionString = "server=192.168.50.174;port=3306;database=wesdhy_wms;user=wesdhy;password=!!Gqq11ee33;";
    

        public async Task<IEnumerable<pal_tableDTO>> SELE_PAL(string itm_code, string st_data, string ed_data)//서비스에서 날짜형식 걸러내야함
        {
            using var connection = new MySqlConnection(connectionString);
            
                var sql = "SELECT pal_code, pal_quantity, itm_code, itm_name, pal_in_data, loc_code AS loc_code, ware_code FROM pal_table WHERE loc_code IS NOT NULL";
                if (itm_code != "0")
                {
                    sql += " AND itm_code LIKE CONCAT('%', @itm_code, '%')";
                }
                    sql += " AND pal_in_data BETWEEN @st_data AND @ed_data ";

                return await connection.QueryAsync<pal_tableDTO>(sql, new { itm_code, st_data, ed_data });

            
            
        }


        public async Task<IEnumerable<object>> PAL_LOG()
        {
            using var connection = new MySqlConnection(connectionString);
            var pallogsql = "select * from pallogtable ORDER BY MOVE_DT DESC";
          

                return await connection.QueryAsync<object>(pallogsql);

        }

        public async Task<IEnumerable<pal_tableDTO>> LOCregistration(string itm_code)
        {
            using var connection = new MySqlConnection(connectionString);
            var palsql = "SELECT pal_code, pal_quantity, itm_code, itm_name, pal_in_data, loc_code, ware_code FROM pal_table where isnull(loc_code) ";
            if (itm_code != "0")
            {
                palsql += " and itm_code LIKE concat('%', @itm_code, '%') ";
            }


            return await connection.QueryAsync<pal_tableDTO>(palsql, new { itm_code });
        }

        public async Task<IEnumerable<ID_PASS_DTO>> LOGINserchID(string ID)
        {
            using var connection = new MySqlConnection(connectionString);
            var palsql = "SELECT userid as id , pas as password FROM wms_member WHERE userid = @ID";
            return await connection.QueryAsync<ID_PASS_DTO>(palsql, new { ID });
        }

        public async Task<string> CRATE_ID(string userid, string pas, string us_name)
        {
            using var connection = new MySqlConnection(connectionString);
            var palsql = "INSERT INTO wms_member (userid, pas, us_name) VALUES (@userid, @pas, @us_name)";


            await connection.QueryAsync<object>(palsql, new { userid, pas, us_name });
            return ("OK");
        }

        public async Task<string> serch_WH_NAME(string warehouse_code)
        {
            using var connection = new MySqlConnection(connectionString);
            
            var palsql = "SELECT warehouse_NAME FROM loc_master_table WHERE warehouse_code = @warehouse_code LIMIT 1";
            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(palsql, new { warehouse_code });
            var warehouse_NAME = result?.warehouse_NAME;

            return warehouse_NAME ?? "serch_fail";
            
        }

        public async Task<string> loc_codecrate(string WH_NAME)
        {
            using var connection = new MySqlConnection(connectionString);
            var palsql =
                  "SELECT left(loc_code, 2) as crwarecode , " +
                  "LPAD(SUBSTRING_INDEX(loc_code, left(loc_code, 2), -1) + 1 ,'4','0') as crloccode " +
                  "FROM loc_master_table " +
                  "where warehouse_name = @WH_NAME " +
                  "order by loc_code desc " +
                  "LIMIT 1";

            var result = await connection.QueryAsync<dynamic>(palsql, new { WH_NAME });
            var crwarecode = result.FirstOrDefault()?.crwarecode;
            var crloccode = result.FirstOrDefault()?.crloccode;

            return $"{crwarecode}{crloccode}";
        }

        public async Task<string> loc_crate(loc_CrateDTO Params)
        {
            using var connection = new MySqlConnection(connectionString);
            var palsql = "INSERT INTO loc_master_table (Loc_code, Loc_name, Warehouse_code, Warehouse_name) VALUES (@Loc_code, @Loc_name, @Warehouse_code, @Warehouse_name)";

            await connection.QueryAsync<object>(palsql, Params);
            return "OK";
        }

        public async Task<string> loc_del(string loc_code)
        {
            using var connection = new MySqlConnection(connectionString);
            var palsql = "DELETE FROM loc_master_table WHERE loc_code = @loc_code";

            await connection.QueryAsync<object>(palsql, new { loc_code });
            return ("OK");
        }


        public async Task<string> loc_move(string pal_code, string where_move_loc)
        {
            using var connection = new MySqlConnection(connectionString);
            var palsql = "update pal_table set loc_code = @where_move_loc where pal_code = @pal_code";

            await connection.QueryAsync<object>(palsql, new { pal_code, where_move_loc });
            return ("OK");
        }

        public async Task<string> ware_move(string pal_code, string where_move)
        {
            using var connection = new MySqlConnection(connectionString);
            var palsql = "update pal_table set ware_code = @where_move, loc_code = NULL where pal_code = @pal_code ";

            await connection.QueryAsync<object>(palsql, new { pal_code, where_move });
            return("OK");
        }

        public async Task<string> palNOcrate()
        {
            using var connection = new MySqlConnection(connectionString);
            var palsql =
                  "select concat(DATE_FORMAT(now(), '%y%m'), SUBSTRING(pal_code,'5')+1) as SN " +
                  "from pal_table order by pal_in_data desc,PAL_CODE DESC limit 1";

            //var N_loc_code = connection.QueryAsync<object>(palsql);
            var result = await connection.QueryFirstOrDefaultAsync<string>(palsql);

            return result;
        }

        public async Task<string> Manualregistration(ManualregistrationDTO DTOparams)//DTO
        {
            using var connection = new MySqlConnection(connectionString);
            var palsql = "INSERT INTO PAL_TABLE VALUES (@crateNO,@pal_quantity,null,@ware_code,@itm_code,@itm_name,now())";

            //'INSERT INTO PAL_TABLE VALUES (?,?,null,?,?,?,now())';

            await connection.QueryAsync<object>(palsql,  DTOparams );
            return ("OK");
        }

        public async Task<string> inventory_adj(string pal_code, string how_many)//DTO
        {
            using var connection = new MySqlConnection(connectionString);
            var palsql = "update pal_table set pal_quantity = @how_many where pal_code = @pal_code";

           

            await connection.QueryAsync<object>(palsql, new { pal_code,how_many });

            return ("OK");
        }

        public async Task<IEnumerable<object>> all_qal(string itm_code)
        {
            using var connection = new MySqlConnection(connectionString);
            var palsql =
                "select * from (select PAL.ITM_CODE,ITM.ITM_NAME ,COALESCE((NOLOC_PAL.미적치개수), 0)  as 미적치개수,PAL.전체개수,ITM.price,(price*PAL.전체개수)as TOTALprice ," +
                " WHERE_PAL.재고위치,ITM.registrationdate as 제품등록일" +
                " from itm_master as ITM join (SELECT itm_code ,sum(pal_quantity) as 전체개수  FROM pal_table group by itm_code) as PAL on PAL.itm_code = ITM.ITM_CODE" +
                " left join (SELECT itm_code ,sum(pal_quantity) as 미적치개수  FROM pal_table where  LOC_CODE is null group by itm_code ) as NOLOC_PAL on NOLOC_PAL.itm_code = ITM.ITM_CODE" +
                " left join (SELECT ITM_CODE,GROUP_CONCAT(loc_code SEPARATOR ' ') as 재고위치  from pal_table pt  group by itm_code ) as WHERE_PAL on WHERE_PAL.itm_code = ITM.ITM_CODE) as all_qal";

            if (itm_code != "0")
            {
                palsql += " where itm_code LIKE concat('%', @itm_code, '%') ";
            }
            return await connection.QueryAsync<object>(palsql, new { itm_code });
        }




        public async Task<IEnumerable<object>> Salestatus(string st_data, string ed_data)
        {
            using var connection = new MySqlConnection(connectionString);
            var palsql =
               "select itm.ITM_NAME ,itm_prup.itm_code,(itm_prup.price/itm.price)as sellcnt,itm.price as single_price ,itm_prup.price,itm.registrationdate  from itm_master as itm " +
               " join (select SAL_M.itm_code,SUM(SEL_QUL)*ITM_M.price as price from sales_master as SAL_M join itm_master as ITM_M on SAL_M.itm_code = ITM_M.ITM_CODE ";
            palsql += " where SAL_M.sel_data between ? and ? ";
            palsql += " group by SAL_M.itm_code) as itm_prup on itm.itm_code = itm_prup.itm_code order by itm_prup.itm_code desc";

            return await connection.QueryAsync<object>(palsql, new { st_data, ed_data });
        }


        public async Task<int> SELECT_NOW_quantity(string PAL_CODE)
        {
            using var connection = new MySqlConnection(connectionString);
            var palsql = "SELECT pal_quantity FROM pal_table WHERE loc_code IS NOT NULL AND pal_code = @PAL_CODE ";

            var result = await connection.QueryAsync<dynamic>(palsql, new { PAL_CODE });
            var pal_quantity = result.FirstOrDefault()?.pal_quantity;

            return pal_quantity;

        }


        public async Task<string> salesNOcrate()
        {
            using var connection = new MySqlConnection(connectionString);
            var palsql =

                 "select concat(DATE_FORMAT(now(), '%y%m'), " +
                 "SUBSTRING(sellNO,'5')+1)  as SN " +
                 "from sales_master order by sel_data desc,sellNO DESC limit 1";
            var result = await connection.QueryAsync<dynamic>(palsql);
            var SN = result.FirstOrDefault()?.SN;
          

            return $"{SN}";

        }
        public async Task<string> salesINSERT(salesINSERTDTO DTOparams)
        {
            using var connection = new MySqlConnection(connectionString);
            var palsql = "INSERT INTO sales_master VALUES (@salesNO,@itm_code,@sel_ware_code,@sel_qul,now())";

            //'INSERT INTO sales_master VALUES (?,?,?,?,now())';

            await connection.QueryAsync<object>(palsql,DTOparams );
            return ("OK");
        }

        public async Task<bool> LOGSET(string pal_code, string WHERE_MOVE, string reason)
        {
            using var connection = new MySqlConnection(connectionString);
            var logsql = "INSERT INTO pallogtable VALUES (@pal_code,@WHERE_MOVE,@reason,NOW())";
            await connection.QueryAsync<object>(logsql, new { pal_code, WHERE_MOVE, reason });
            return true;
        }






    }
}
