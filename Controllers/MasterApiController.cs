using Medical.BAL;
using Medical.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Medical.Controllers
{
    public class MasterApiController : ApiController
    {
        MasterBAL masterBAL;
        [HttpGet]
        public List<MandalModel> GetMandals(int districtId)
        {
            masterBAL = new MasterBAL();
            return masterBAL.GetMandalList(districtId);
        }

        [HttpGet]
        public List<VillageModel> GetVillages(int mandalId)
        {
            masterBAL = new MasterBAL();
            return masterBAL.GetVillageList(mandalId);
        }
        
    }
}
