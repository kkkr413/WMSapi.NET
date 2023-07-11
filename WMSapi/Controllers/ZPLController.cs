using Microsoft.AspNetCore.Mvc;
using WMSapi.Models;
using WMSapi.Service;

namespace WMSapi.Controllers
{
    [Route("api/print")]
    [ApiController]
    public class ZPLController : Controller
    {

        [HttpPost("zplprint")]
        public IActionResult zplprint([FromBody] ZPLDATADTO model)
        {
            string res = "전송요청에 실패 했습니다.";
            if (model.data == null || model.data2 == null || model.print_ip == null || model.quantity == null)
            {
                return BadRequest(new { error = "필수 정보가 누락되었습니다." });
            }

            var zplservice = new ZPLservice();
                res = zplservice.PAL_zpl(model.print_ip, model.data[0], model.data2[0], model.quantity);
            
            return Ok(new { message = res });
        }
    }
}
