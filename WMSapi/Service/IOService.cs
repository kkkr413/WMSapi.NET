using AutoMapper;
using OfficeOpenXml;
using System.Data;
using WMSapi.Model;
using WMSapi.Models;
using WMSapi.Repositories;

namespace WMSapi.Service
{
    public interface IOService_R
    {
      Task<string> UPLOADService(IFormFile file);

        Task<IFormFile> DOWNLOADService();
 
    }


    public class IOService : IOService_R
    {
        //DI
        private IORepositories_R IORepository;
        private readonly IMapper mapper;
        public IOService(IORepositories_R di_WMSRepository, IMapper di_mapper)
        {
            IORepository = di_WMSRepository;
            mapper = di_mapper;
        }


        public async Task<string> UPLOADService(IFormFile file)
        {
            int[] up_res = { 0, 0 };


            using var stream = new MemoryStream();
            file.CopyTo(stream);
            stream.Position = 0;

            using var package = new ExcelPackage(stream);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var worksheet = package.Workbook.Worksheets[0];


            for (int row = 1; row <= worksheet.Dimension.Rows; row++)
            {
                pal_tableDTO EXEL_UPLOAD = new pal_tableDTO();
                EXEL_UPLOAD.pal_code = worksheet.Cells[row, 1].Value?.ToString();
                EXEL_UPLOAD.pal_quantity = worksheet.Cells[row, 2].Value?.ToString();
                EXEL_UPLOAD.ware_code = worksheet.Cells[row, 3].Value?.ToString();
                EXEL_UPLOAD.loc_code = worksheet.Cells[row, 4].Value?.ToString();
                EXEL_UPLOAD.itm_code = worksheet.Cells[row, 5].Value?.ToString();
                EXEL_UPLOAD.itm_name = worksheet.Cells[row, 6].Value?.ToString();
                EXEL_UPLOAD.pal_in_data = worksheet.Cells[row, 7].Value?.ToString();
                
                string res = await IORepository.pal_upload(EXEL_UPLOAD);
                if (res== "OK")
                {
                    up_res[0]++;
                }
                else
                {
                    up_res[1]++;
                    return  $"오류가 발생하여  {row}ROW 에서 작업이 중단 되었습니다. {res} ,총 {up_res[0]}건 처리 완료 되었습니다.  ";
                   
                }

            }

            string resmessage = $"엑셀 업로드 완료 총 {up_res[0]}건 처리 완료 되었습니다. ";
            

            return resmessage;
        }

        public async Task<IFormFile> DOWNLOADService()
        {
            DateTime currentDate = DateTime.Now;
            IEnumerable<pal_tableDTO> res = await IORepository.Pal_dawnload();
            DataTable dataTable = new DataTable();
            // Add columns to the DataTable
            dataTable.Columns.Add("pal_code");
            dataTable.Columns.Add("pal_quantity");
            dataTable.Columns.Add("itm_code");
            dataTable.Columns.Add("itm_name");
            dataTable.Columns.Add("pal_in_data");
            dataTable.Columns.Add("loc_code");
            dataTable.Columns.Add("ware_code");

            foreach (var item in res)
            {
                dataTable.Rows.Add(
                    item.pal_code,
                    item.pal_quantity,
                    item.itm_code,
                    item.itm_name,
                    item.pal_in_data,
                    item.loc_code,
                    item.ware_code
                );
            }

            // Excel 파일 생성 및 데이터 작성
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var excelPackage = new ExcelPackage();
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Sheet1"); // Add a worksheet
            worksheet.Cells.LoadFromDataTable(dataTable, true);

            // Excel 파일 저장
            string fileName = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} 파레트_현황.xlsx";
            byte[] fileBytes;
            using (var stream = new MemoryStream())
            {
                excelPackage.SaveAs(stream);
                fileBytes = stream.ToArray();
            }

            // IFormFile 객체 생성
            IFormFile file = new FormFile(new MemoryStream(fileBytes), 0, fileBytes.Length, "file", fileName);

            return file;
        }


    }
}
