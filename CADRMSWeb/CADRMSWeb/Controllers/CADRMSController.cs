using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using System.Data;

namespace CADRMSWeb.Controllers
{
    public class CADRMSController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }

        public static string ToJSON(object i)
        {
            return JsonConvert.SerializeObject(i);
        }

        [HttpGet]
        public string GetEventInfomationFromEventId(string Id)
        {
            EventInfo eventInfo = new EventInfo();

            return  ToJSON(eventInfo.GetEventInformation(Id));
        }

        [HttpGet]
        public string CreateEvent(string location, string type)
        {
            EventInfo eventInfo = new EventInfo();

            return eventInfo.CreateEvent(location, type);
        }

        [HttpGet]
        public string GetVehicleInfoFromVehicleId(string vehicleId)
        {
            VehicleInfo vehInfo = new VehicleInfo();

            return ToJSON(vehInfo.GetVehicleInformation(vehicleId));
        }

        [HttpGet]
        public string GetPersonInfo(string fname, string lname, string city)
        {
            personinfo person = new personinfo();

            return ToJSON(person.GetPersonInformations(fname, lname, city));
        }
    }

    public class EventInfo
    {
        public string AgencyEventId { get; set; }
        public int CommonEventId { get; set; }
        public string Location { get; set; }
        public string EventType { get; set; }
        public string CaseNumber { get; set; }

        public string CreateEvent(string location, string type)
        {
            EventInfo eventInfo = new EventInfo();

            eventInfo.AgencyEventId = "Event E20150000181 Created";


            return eventInfo.AgencyEventId;
        }

        public EventInfo GetEventInformation(string eventId)
        {
            EventInfo eventInfo = new EventInfo();

            try
            {
                //todo: DataBase query
                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = "Server=in-ips49\\SQL2014; Database=dev_lsv_db; User ID=sa; Password=Intergraph.1";
                    conn.Open();

                    SqlCommand command = new SqlCommand(@"SELECT ae.NUM_1, ae.eid, ae.TYCOD, ae.TYP_ENG, ce.elocation, cn.CASE_NUM
                                                            FROM AGENCY_EVENT ae
                                                            Left outer join COMMON_EVENT ce on ae.EID = ce.EID
                                                            left outer join CASE_NUM cn on ae.eid = cn.EID
                                                            where ae.num_1 = @agencyEventId", conn);

                    command.Parameters.Add(new SqlParameter("agencyEventId", eventId));

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            eventInfo.AgencyEventId = reader["num_1"].ToString();
                            eventInfo.CommonEventId = Convert.ToInt32(reader["eid"]);
                            eventInfo.Location = reader["elocation"].ToString();
                            eventInfo.EventType = reader["TYCOD"].ToString() + " - " + reader["TYP_ENG"].ToString();
                            eventInfo.CaseNumber = reader["CASE_NUM"].ToString();
                        }
                    }
                }

                //temporary assignment
                //eventInfo.AgencyEventId = eventId;
                //eventInfo.EventType = "10-30";
                //eventInfo.Location = "123 E MAIN ST LVIL";
                //eventInfo.CaseNumber = "R1234567";
            }
            catch (SqlException ex)
            {

            }

            return eventInfo;
        }
    }

    public class VehicleInfo
    {
        //define properties
        //public string plate_state { get; set; }
        public string Ownername { get; set; }
        //public string Insu_Pol_No { get; set; }
        public int DUICount { get; set; }
        public int TheftCount { get; set; }
        public List<theftInfo> StolenInfo { get; set; }
        public List<DUIInfo> VehicleDuiInfo { get; set; }
        //public string[] relatedIncidentNo { get; set; }
        //public string plate_State { get; set; }
        //public string[] relatedDUIno { get; set; }
        //public string DUIdrivername { get; set; }

        public VehicleInfo GetVehicleInformation(string vehicleId)
        {
            VehicleInfo vehicleInfo = new VehicleInfo();
            try
            {

                //Establich DB connection
                using (SqlConnection conn = new SqlConnection())
                {

                    conn.ConnectionString = "Data Source=IN-WRDBVM01;Initial Catalog=WEBRMS_37_TT;User id=sa;Password=Intergraph.1;";
                    conn.Open();
                    SqlCommand command;
                    SqlDataAdapter adapter;

                    //get owner name
                    command = new SqlCommand("select FNAME+LNAME from [webrms_37_TT].[dbo].[MASTNAME] where recnum =(select OWNER_NAME_REC from [webrms_37_TT].[dbo].[MASTAUTO] where [PLATE]='" + vehicleId + "')", conn);
                    //command.Parameters.AddWithValue("plateNo", vehicleId);
                    //vehicleInfo.Ownername = (string)command.ExecuteScalar();
                    vehicleInfo.Ownername = "STEVE JOHNSON";
                    //get theft info

                    adapter = new SqlDataAdapter("select [DATE_FOUND],[DATE_LOST],MLOC.STREETADDR STOLEN_LOCATION_REC,INC.[INCIDENT_NUM] from  [webrms_37_TT].[dbo].[THEFT] THEFT LEFT OUTER JOIN  [webrms_37_TT].[dbo].[MASTLOCATION] MLOC ON MLOC.RECNUM=THEFT.STOLEN_LOCATION_REC LEFT OUTER JOIN [webrms_37_TT].[dbo].[INCIDENT] INC ON INC.RECNUM=THEFT.INCIDENT_REC  where VEH_REC=(select RECNUM from  [webrms_37_TT].[dbo].[MASTAUTO] where [PLATE]='" + vehicleId + "')", conn);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    List<theftInfo> theftlist = new List<theftInfo>();
                    if (ds != null)
                    {
                        foreach (DataRow row in ds.Tables[0].AsEnumerable())
                        {
                            theftlist.Add(new theftInfo(createtheftrecord(row)));
                            break;
                        }
                    }
                    vehicleInfo.StolenInfo = theftlist;
                    vehicleInfo.TheftCount = theftlist.Count();

                    //get DUI LISt

                    adapter = new SqlDataAdapter("select [STOPPED_DATETIME],MLOC.STREETADDR [STOPPED_ADDR],MNAM.FNAME+MNAM.LNAME [DRIVER_NAME_REC],DUI_NUMBER from [webrms_37_TT].[dbo].[DUI] DRI LEFT OUTER JOIN [webrms_37_TT].[dbo].[MASTLOCATION] MLOC ON MLOC.RECNUM=DRI.STOPPED_LOCATION_REC LEFT OUTER JOIN [webrms_37_TT].[dbo].MASTNAME MNAM ON MNAM.RECNUM=DRI.DRIVER_NAME_REC  where [VEH_PLATE]='" + vehicleId + "'", conn);
                    DataSet dsDUI = new DataSet();

                    adapter.Fill(dsDUI);
                    List<DUIInfo> DUIList = new List<DUIInfo>();
                    if (dsDUI != null)
                    {
                        foreach (DataRow row in dsDUI.Tables[0].AsEnumerable())
                        {
                            DUIList.Add(new DUIInfo(createDUIrecord(row)));
                            break;
                        }
                    }
                    vehicleInfo.VehicleDuiInfo = DUIList;
                    vehicleInfo.DUICount = DUIList.Count();
                }
            }
            catch (SqlException ex)
            {
            }

            return vehicleInfo;
        }
        public theftInfo createtheftrecord(DataRow row)
        {

            theftInfo theft = new theftInfo();
            //theft.datefound = row["DATE_FOUND"].ToString();
            theft.datelost = row["DATE_LOST"].ToString();
            theft.stolenvehicleLoc = row["STOLEN_LOCATION_REC"].ToString();
            //theft.relatedIncidentno = row["INCIDENT_NUM"].ToString();
            return theft;

        }
        public DUIInfo createDUIrecord(DataRow row)
        {
            DUIInfo dui = new DUIInfo();
            //dui.relatedDUIno = row["DUI_NUMBER"].ToString();
            dui.stoppeddatetime = row["STOPPED_DATETIME"].ToString();
            dui.DriverName = row["DRIVER_NAME_REC"].ToString();
            dui.stoppedloc = row["STOPPED_ADDR"].ToString();
            return dui;
        }

        public class theftInfo
        {
            //public string relatedIncidentno { get; set; }
            public string datelost { get; set; }
            //public string datefound { get; set; }
            public string stolenvehicleLoc { get; set; }
            public theftInfo(theftInfo theft)
            {
                //relatedIncidentno = theft.relatedIncidentno;
                datelost = theft.datelost;
                //datefound = theft.datefound;
                stolenvehicleLoc = theft.stolenvehicleLoc;
            }
            public theftInfo()
            {
            }


        }
        public class DUIInfo
        {
            //public string relatedDUIno { get; set; }
            public string DriverName { get; set; }
            public string stoppedloc { get; set; }
            public string stoppeddatetime { get; set; }

            public DUIInfo(DUIInfo dui)
            {
                //relatedDUIno = dui.relatedDUIno;
                DriverName = dui.DriverName;
                stoppedloc = dui.stoppedloc;
                stoppeddatetime = dui.stoppeddatetime;
            }
            public DUIInfo()
            {
            }

        }

        public class ConnecttoRMS
        {
            //Plate state       
            //Insurance Policy number
            //name-rec(owner) -find first name and last name in mastname
            //veh rec
            //check is theft entry present -related incidents
            //if stolen(date found is null) then date lost
            //stolen location
            //DUI records with platenumber if so give DUI incident number
            //if warning present ,when was the last warning
            //warning data/time
            //warning response
            //warning locality
            //warning officer

            SqlConnection conn = new SqlConnection();
            public void openconnection()
            {
                conn.ConnectionString = "Data Source=IN-WRDBVM01;Initial Catalog=WEBRMS_37_TT;User id=sa;Password=Intergraph.1;";
                conn.Open();

            }
        }

    }

    public class personinfo
    {
        public string Name { get; set; }
        public int DuiRecordCount { get; set; }
        public int ArrestRecordCount { get; set; }
        public int IncidentRecordCount { get; set; }

        public personinfo GetPersonInformations(string fname, string lname, string city)
        {
            personinfo person = new personinfo();
            try
            {

                //Establich DB connection
                using (SqlConnection conn = new SqlConnection())
                {
                    Int32 mastnamerecnum;
                    conn.ConnectionString = "Data Source=IN-WRDBVM01;Initial Catalog=WEBRMS_37_TT;User id=sa;Password=Intergraph.1;";
                    conn.Open();
                    SqlCommand command;
                    command = new SqlCommand("select recnum from [webrms_37_TT].[dbo].[MASTNAME] where FNAME='" + fname + "'AND LNAME='" + lname + "'", conn);
                    //command.Parameters.AddWithValue("plateNo", vehicleId);
                    mastnamerecnum = Convert.ToInt32(command.ExecuteScalar());
                    command = new SqlCommand("select count(*) from [webrms_37_TT].[dbo].[DUI] where DRIVER_NAME_REC=" + mastnamerecnum, conn);
                    //person.DuiRecordCount = (Convert.ToInt32(command.ExecuteScalar()));
                    person.DuiRecordCount = 1;
                    command = new SqlCommand("select count(*) from [webrms_37_TT].[dbo].[ARRESTS] where NAME_REC=" + mastnamerecnum, conn);
                    //person.ArrestRecordCount = (Convert.ToInt32(command.ExecuteScalar()));
                    person.ArrestRecordCount = 2;
                    command = new SqlCommand("select count(*) from [webrms_37_TT].[dbo].[LINK] where NAME_REC=" + mastnamerecnum, conn);
                    //person.IncidentRecordCount = (Convert.ToInt32(command.ExecuteScalar()));
                    person.IncidentRecordCount = 3;

                    person.Name = "James Bond";
                }
            }
            catch (Exception ex)
            {
                person.DuiRecordCount = 1;
                person.ArrestRecordCount = 2;
                person.IncidentRecordCount = 3;
                person.Name = "James Bond";
            }
            return person;
        }
    }

}