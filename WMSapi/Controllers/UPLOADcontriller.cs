using Microsoft.AspNetCore.Mvc;
using WMSapi.Service;
using System.Data;
using System.Data.SqlClient;
using System.IO;
namespace WMSapi.Controllers
{
    
    [Route("api/upload")]
    [ApiController]
    public class UPLOADcontriller : ControllerBase
    {

        private IOService_R IODService;
        

        // DI
        public UPLOADcontriller(IOService_R di_UPLOADService)
        {
            IODService = di_UPLOADService;
         
        }
        [HttpPost("PAL_strok_upload")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                var response = new { message = "업로드된 파일이 없습니다." };
                return Ok(response);
                
            }
            string stmassage = await IODService.UPLOADService(file);
            return Ok(stmassage);
        }
        [HttpPost("PAL_strok_download")]
        public async Task<IActionResult> DOWNloadExcel()
        {
            IFormFile file = await IODService.DOWNLOADService();

            if (file == null)
            {
                return NotFound(); 
            }
            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            // 파일 스트림 반환
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.FileName);

        }




    }
}
