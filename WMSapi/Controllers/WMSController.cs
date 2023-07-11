using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using WMSapi.Model;
using WMSapi.Models;
using WMSapi.Service;

namespace WMSapi.Controllers
{
    //[ApiVersion("1.0")]
    [Route("api/wms")]
    [ApiController]
    public class WMSController : ControllerBase
    {


        private WMSservice_R WMSService;
        private LOGSETservice_R LOGSETservice;

        // DI
        public WMSController(WMSservice_R di_WMSService, LOGSETservice_R di_LOGSETservice)
        {
            WMSService = di_WMSService;
            LOGSETservice = di_LOGSETservice;
        }

        [HttpGet]
        public async Task<IActionResult> SELE_PAL(string? itm_code, DateTime? pal_in_data, DateTime? pal_in_data_end)
        {
            var paldata = await WMSService.SELE_PAL(itm_code, pal_in_data, pal_in_data_end);
            var response = new { message = paldata };
            return Ok(response);
        }

        [HttpGet("PAL_LOG")]
        public async Task<IActionResult> PAL_LOG()
        {
            var pallog = await WMSService.SELE_PAL_LOG();
            var response = new { message = pallog };
            return Ok(response);
        }
        [HttpGet("LOCregistration")]
        public async Task<IActionResult> LOCregistration(string? itm_code)
        {
            var palloc = await WMSService.SELE_LOC_regist(itm_code);
            var response = new { message = palloc };
            return Ok(response);
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] ID_PASS_DTO model)
        {
            bool loginOk = await WMSService.login(model.id, model.password);

            if (loginOk && !string.IsNullOrEmpty(model.id) && !string.IsNullOrEmpty(model.password))
            {
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddMinutes(10),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None
                };

                Response.Cookies.Append("wms00teen", model.password, cookieOptions);

                return Ok(new { message = "로그인 성공" });
            }

            return Ok(new { message = "로그인 실패" });
        }
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await Task.Run(() => Response.Cookies.Delete("wms00teen"));
            return Ok(new { message = "로그아웃 성공" });
        }

        [HttpPost("sign_up")]
        public async Task<IActionResult> sign_up(string? id, string? password,string? us_name , string? e_mail )
        {
            string loginOk = await WMSService.sign_up(id, password, us_name, e_mail);
            return Ok(new { message = loginOk });

        }

        [HttpPost("loc_add")]
        public async Task<IActionResult> loc_add(loc_CrateDTO loc_Crate)
        {
            string locadd = await WMSService.loc_add(loc_Crate);
            return Ok(new { message = locadd });

        }
        [HttpPost("loc_del")]
        public async Task<IActionResult> loc_del(string loc_code)
        {
            string locdel = await WMSService.loc_del(loc_code);
            return Ok(new { message = locdel });

        }
    
        [HttpPut("loc_move")]
        public async Task<IActionResult> loc_move([FromBody] PAL_MOVE_DTO PAL_MOVE)
        {
            string locmove = await WMSService.loc_move(PAL_MOVE);
            await LOGSETservice.LOGSET_LM(PAL_MOVE);
            return Ok(new { message = locmove });
        }
        [HttpPut("warehouse_move")]
        public async Task<IActionResult> warehouse_move([FromBody] PAL_MOVE_DTO PAL_MOVE)
        {
            string waremove = await WMSService.ware_move(PAL_MOVE);
            await LOGSETservice.LOGSET_WM(PAL_MOVE);
            return Ok(new { message = waremove });
        }
        [HttpPost("Manualstore")]
        public async Task<IActionResult> Manualstore([FromBody] ManualregistrationDTO Manualregistration)
        {

            var result = await WMSService.Manualstore(Manualregistration);
            if (Manualregistration.ware_code != null)
            {
                await LOGSETservice.LOGSET_II(result.Item2, Manualregistration.ware_code);
                return Ok(new { message = result.Item1 });
            }
            return Ok(new { message = "요청 실패" });
        }

        [HttpPut("inventory_adj")]
        public async Task<IActionResult> Manualstore([FromBody]PAL_MOVE_DTO PAL_MOVE)
        {

            string paladj = await WMSService.inventory_adj(PAL_MOVE);

            await LOGSETservice.LOGSET_QM(PAL_MOVE);

            return Ok(new { message = paladj });
        }
        [HttpPost("sell")]
        public async Task<IActionResult> sell([FromBody] salesINSERTDTO salesINSERT)
        {
            string locsell = await WMSService.sell(salesINSERT);

            //await LOGSETservice.LOGSET_QM(pal_code, how_many.ToString());

            return Ok(new { message = locsell });
        }

         
    }
}

// private readonly string connectionString = "server=192.168.50.174;port=3306;database=wesdhy_wms;user=wesdhy;password=!!Gqq11ee33;";


/*   using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("SELECT pal_code, pal_quantity, loc_code, ware_code, itm_code, itm_name, pal_in_data FROM pal_table", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            var result = new List<pal_table>();

                            while (reader.Read())
                            {
                                var item = new pal_table
                                {
                                    pal_code = reader.GetString("pal_code"),
                                    pal_in_data = reader.GetString("pal_in_data"),
                                    pal_quantity = reader.GetInt32("pal_quantity"),
                                    loc_code = reader.IsDBNull(reader.GetOrdinal("loc_code")) ? null : reader.GetString("loc_code"),
                                    ware_code = reader.GetString("ware_code"),
                                    itm_code = reader.GetString("itm_code"),
                                    itm_name = reader.GetString("itm_name")
                                };

                                result.Add(item);
                            }

                            return Ok(result);
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }*/