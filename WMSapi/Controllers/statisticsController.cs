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
    [Route("api/statistics")]
    [ApiController]
    
    public class statisticsController : ControllerBase
    {
        private WMSservice_R WMSService;

        // DI
        public statisticsController(WMSservice_R di_WMSService)
        {
            WMSService = di_WMSService;
       
        }
        [HttpGet("allqal")]
        public async Task<IActionResult> allqal(string? itm_code)
        {
            if (itm_code == null)
            {
                itm_code = "0";
            }
            var allqal = await WMSService.all_qal(itm_code);
            var response = new { message = allqal };
            return Ok(response);
        }
        [HttpGet("Salestatus")]
        public async Task<IActionResult> Salestatus( DateTime? pal_in_data, DateTime? pal_in_data_end)
        {
            var Salestatus = await WMSService.Salestatus(pal_in_data, pal_in_data_end);
            var response = new { message = Salestatus };
            return Ok(response);
        }




    }
}

