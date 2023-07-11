using AutoMapper;
using Microsoft.AspNetCore.Rewrite;
using MySqlX.XDevAPI.Common;
using MySqlX.XDevAPI.Relational;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Tls;
using System;
using System.Text;
using WMSapi.Model;
using WMSapi.Models;
using WMSapi.Repositories;

namespace WMSapi.Service;

public interface WMSservice_R
{
    Task<IEnumerable<pal_tableDTO>> SELE_PAL(string? itm_code, DateTime? st_date, DateTime? ed_date);
    Task<IEnumerable<object>> SELE_PAL_LOG();
    Task<IEnumerable<pal_tableDTO>> SELE_LOC_regist(string? itm_code);
    Task<bool> login(string? id, string? password);
    Task<string> sign_up(string? id, string? pw, string? us_name, string? e_mail);
    Task<string> loc_add(loc_CrateDTO Loc_crate);
    Task<string> loc_del(string loc_code);
    Task<string> loc_move(PAL_MOVE_DTO PAL_MOVE);
    Task<string> ware_move(PAL_MOVE_DTO PAL_MOVE);
    Task<(string, string)> Manualstore(ManualregistrationDTO Manualregistration);
    Task<string> inventory_adj(PAL_MOVE_DTO PAL_MOVE);
    Task<IEnumerable<object>> all_qal(string itm_code);
    Task<IEnumerable<object>> Salestatus( DateTime? st_date, DateTime? ed_date);
    Task<string> sell(salesINSERTDTO salesINSE);
  
}


public class WMSservice : WMSservice_R
{
    private WMSRepositories_R WMSRepository;
    private readonly IMapper mapper;
    //di (STSHA512 di pass)
    public WMSservice(WMSRepositories_R di_WMSRepository, IMapper di_mapper)
    {
        WMSRepository = di_WMSRepository;
        mapper = di_mapper;
    }
    

    //파레트 조회
    public async Task<IEnumerable<pal_tableDTO>> SELE_PAL(string? itm_code, DateTime? st_date, DateTime? ed_date)
    {
        if (!st_date.HasValue && !ed_date.HasValue && st_date > ed_date)
        {
            throw new ArgumentException("올바른 날짜 선택이 아닙니다.");
        }
        itm_code = itm_code ?? "0";
        DateTime startDate = st_date ?? DateTime.MinValue;
        DateTime endDate = ed_date ?? DateTime.MaxValue;
        
        string Sst_date = startDate.ToString("yyyy-MM-dd");
        string Sed_date = endDate.ToString("yyyy-MM-dd");


        return await WMSRepository.SELE_PAL(itm_code, Sst_date, Sed_date);
    }
    public async Task<IEnumerable<object>> SELE_PAL_LOG()//dto 만들어야함
    {
        return await WMSRepository.PAL_LOG();
    }

    public async Task<IEnumerable<pal_tableDTO>> SELE_LOC_regist(string? itm_code)
    {
        itm_code = itm_code ?? "0";
        return await WMSRepository.LOCregistration(itm_code);
    }

    public async Task<bool> login(string? id,string? password)
    {
        if (id == null)
        {
            return false;
        }
        var result = await WMSRepository.LOGINserchID(id);
       
        if (result.Count() != 1)
        {

            return false;
        
        }

        if (password != null) 
        {
            var firstRow = result.FirstOrDefault();
            var SHApass = firstRow?.password?.ToString();
            var hasher = new STSHA512();
            string hashedPassword = hasher.HashPassword(password);

            if (SHApass == hashedPassword)
            {
                return true;
            }
            else
            {
                return false;
            }            
        }
        return false;
    }

    //회원가입
    public async Task<string> sign_up(string? id, string? pw, string? us_name, string? e_mail)
    {
        if (id == null || pw == null || us_name == null)//이메일 나중에 넣어야함.
        {
            return "유효하지 않는 정보 입니다.";
        }

        var IDresult = await WMSRepository.LOGINserchID(id);
        if (IDresult.Count() >= 1)
        {
            return "가입실패 - 중복아이디 "+ id;
        }


        var hashpass = new STSHA512();
        string Convhashpass = hashpass.HashPassword(pw);
        string Regiresult = await WMSRepository.CRATE_ID(id, Convhashpass, us_name);
        string resmessage = "가입에 실패 했습니다.(시스템 오류)";
        if (Regiresult == "OK")
        {
            resmessage = id + " 가입에 성공 했습니다.";
        }
        return resmessage;
    }
    //loc 생성
    public async Task<string> loc_add(loc_CrateDTO Loc_crate)
    {
        if (Loc_crate.Warehouse_code == null)
        {
            return "창고코드가 입력되지 않았습니다.";
        }

        Loc_crate.Warehouse_name = await WMSRepository.serch_WH_NAME(Loc_crate.Warehouse_code);

        if (Loc_crate.Warehouse_name == "serch_fail")
        {
            return "창고코드 생성에 실패 헸습니다.";
        }
        Loc_crate.Loc_code = await WMSRepository.loc_codecrate(Loc_crate.Warehouse_name);

        string result = await WMSRepository.loc_crate(Loc_crate);
        string resmessage = "loc 생성에 실패 했습니다.(시스템 오류)";
        if (result == "OK")
        {
            resmessage = "loc 생성 성공했습니다"+ Loc_crate.Loc_code;
        }
        return resmessage;
    }

    public async Task<string> loc_del(string loc_code)
    {
        string result = await WMSRepository.loc_del(loc_code);

        string resmessage = "loc 삭제에 실패 했습니다.(시스템 오류)";
        if (result == "OK")
        {
            resmessage = "loc 삭제 성공했습니다" + loc_code;
        }

        return resmessage;
    }
    //로케이션 이동

    public async Task<string> loc_move(PAL_MOVE_DTO PAL_MOVE)
    {
        int[] move_res = { 0, 0 };
        List<string> MOVEokloc = new List<string>();

        if (PAL_MOVE.pal_code == null || PAL_MOVE.where_move_loc == null)
        {
            return "재고이동에 실패 했습니다.";
        }


        foreach (var pal_code in PAL_MOVE.pal_code)
        {
            string result = await WMSRepository.loc_move(pal_code, PAL_MOVE.where_move_loc);
            if (result == "OK")
            {
                move_res[0]++;
                MOVEokloc.Add(pal_code);
            }
            else
            {
                move_res[1]++;
            }
        }

        string locs = string.Join(", ", MOVEokloc);
        string resmessage = $"파레트 {locs} (이)가 {PAL_MOVE.where_move_loc}로 이동처리 완료 "
        + $"총 {move_res[0]}건 처리, {move_res[1]}건 실패 되었습니다";

        return resmessage;
    }
    //창고 이동
    public async Task<string> ware_move(PAL_MOVE_DTO PAL_MOVE)
    {
        int[] move_res = { 0, 0 };
        List<string> MOVEoklist = new List<string>();
        if (PAL_MOVE.pal_code == null || PAL_MOVE.ware_code == null)
        {
            return "창고 이동에 실패 했습니다.";
        }

        foreach (var pal_code in PAL_MOVE.pal_code)
        {
            string result = await WMSRepository.ware_move(pal_code, PAL_MOVE.ware_code);
            if (result == "OK")
            {
                move_res[0]++;
                MOVEoklist.Add(pal_code);
            }
            else
            {
                move_res[1]++;
            }
        }

        string locs = string.Join(", ", MOVEoklist);
        string resmessage = $"파레트 {locs} (이)가 {PAL_MOVE.ware_code}로 이동처리 완료 "
        + $"총 {move_res[0]}건 처리, {move_res[1]}건 실패 되었습니다";

        return resmessage;
    }
    //재고수동등록
    public async Task<(string, string)> Manualstore(ManualregistrationDTO Manualregistration)
    {
        string N_PAL_NO = await WMSRepository.palNOcrate();

        Manualregistration.crateNO = N_PAL_NO;
        string res = await WMSRepository.Manualregistration(Manualregistration);
        string resmessage = "재고등록에 실패 했습니다.";
        if (res == "OK")
        {
            resmessage = "재고등록에 성공 했습니다.";
        }
        return (resmessage, N_PAL_NO);
    }

    public async Task<string> inventory_adj(PAL_MOVE_DTO PAL_MOVE)
    {
        int[] move_res = { 0, 0 };
        List<string> MOVEoklist = new List<string>();
        if (PAL_MOVE.pal_code == null || PAL_MOVE.how_many == null)
        {
            return "재고조정에 실패 했습니다.";
        }

        foreach (var pal_code in PAL_MOVE.pal_code)
        {
            string result = await WMSRepository.inventory_adj(pal_code, PAL_MOVE.how_many);
            if (result == "OK")
            {
                move_res[0]++;
                MOVEoklist.Add(pal_code);
            }
            else
            {
                move_res[1]++;
            }
        }

        string locs = string.Join(", ", MOVEoklist);
        string resmessage = $"파레트 {locs} 개수 {PAL_MOVE.how_many}로 조정 완료 "
        + $"총 {move_res[0]}건 처리, {move_res[1]}건 실패 되었습니다";

        return resmessage;
    }


    public async Task<IEnumerable<object>> all_qal(string itm_code)//dto 만들어야함
    {

      
        return await WMSRepository.all_qal(itm_code);
    }

    public async Task<IEnumerable<object>> Salestatus( DateTime? st_date, DateTime? ed_date)
    {
        if (!st_date.HasValue && !ed_date.HasValue && st_date > ed_date)
        {
            throw new ArgumentException("올바른 날짜 선택이 아닙니다.");
        }
       
        DateTime startDate = st_date ?? DateTime.MinValue;
        DateTime endDate = ed_date ?? DateTime.MaxValue;

        string Sst_date = startDate.ToString("yyyy-MM-dd");
        string Sed_date = endDate.ToString("yyyy-MM-dd");


        return await WMSRepository.Salestatus( Sst_date, Sed_date);
    }
    public async Task<string> sell(salesINSERTDTO salesINSE)
    {
        if (salesINSE.PAL_CODE == null || salesINSE.itm_code == null)
        {
            return "올바르지 않는 PAL_CODE 입니다.";

        }
        int Resqul = await WMSRepository.SELECT_NOW_quantity(salesINSE.PAL_CODE[0]);
        int resint = Resqul - salesINSE.sel_qul ;
        if (resint < 0)
        { 
            return "현 재고 개수보다 적습니다";

        }
        else if (salesINSE.PAL_CODE.Length > 1)
        {
            return "다중 파레트 미지원";
        }
        else
        {
            
            string salesNO = await WMSRepository.salesNOcrate();
            if (salesNO == null)
            {
                return "salesNO 채번 오류";
            }

            salesINSE.salesNO = salesNO;
            salesINSE.sel_qul = resint;
    

            await WMSRepository.salesINSERT(salesINSE);
            await WMSRepository.inventory_adj(salesINSE.PAL_CODE[0], resint.ToString());
            return $"{salesINSE.itm_code[0]}품목{salesINSE.sel_qul}'개 판매 등록 성공";


        }
          
    }












}
