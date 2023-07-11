using AutoMapper;
using WMSapi.Models;
using WMSapi.Repositories;

namespace WMSapi.Service
{

    public interface LOGSETservice_R
    {
        Task<bool> LOGSET_QM(PAL_MOVE_DTO PAL_MOVE);
        Task<bool> LOGSET_LM(PAL_MOVE_DTO PAL_MOVE);
        Task<bool> LOGSET_WM(PAL_MOVE_DTO PAL_MOVE);
        Task<bool> LOGSET_II(string pal_code, string ware_code);


    }


    public class LOGSETservice : LOGSETservice_R
    {

        private WMSRepositories_R WMSRepository;
        private readonly IMapper mapper;
        //di (STSHA512 di pass)
        public LOGSETservice(WMSRepositories_R di_WMSRepository, IMapper di_mapper)
        {
            WMSRepository = di_WMSRepository;
            mapper = di_mapper;
        }


        public async Task<bool> LOGSET_QM(PAL_MOVE_DTO PAL_MOVE)
        {
            if (PAL_MOVE.pal_code == null || PAL_MOVE.how_many == null)
            {
                return false;
            }
            foreach (string pal_code in PAL_MOVE.pal_code)
            {
                
                bool Result = await WMSRepository.LOGSET(pal_code, PAL_MOVE.how_many, "QM");
                Console.WriteLine($"log 생성 완료 {pal_code} - 재고조정");

            }
            return true;

        }
        public async Task<bool> LOGSET_LM(PAL_MOVE_DTO PAL_MOVE)
        {
            if (PAL_MOVE.pal_code == null|| PAL_MOVE.where_move_loc == null)
            {
                return false;
            }
            foreach (string pal_code in PAL_MOVE.pal_code)
            {

                bool Result = await WMSRepository.LOGSET(pal_code, PAL_MOVE.where_move_loc, "LM");
                Console.WriteLine($"log 이동 완료 {pal_code} - 로케이션이동");

            }
            return true;
        }

        public async Task<bool> LOGSET_WM(PAL_MOVE_DTO PAL_MOVE)
        {
            if (PAL_MOVE.pal_code == null || PAL_MOVE.ware_code == null)
            {
                return false;
            }
            foreach (string pal_code in PAL_MOVE.pal_code)
            {

                bool Result = await WMSRepository.LOGSET(pal_code, PAL_MOVE.ware_code, "WM");
                Console.WriteLine($"log 이동 완료 {pal_code} - 창고이동");

            }
            return true;
        }

        public async Task<bool> LOGSET_II(string pal_code, string ware_code)
        {

          

                bool Result = await WMSRepository.LOGSET(pal_code, ware_code, "II");
                Console.WriteLine($"log {ware_code} 재고 등록 성공");

            
            return true;
        }



    }
}
