using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using VoiceIdentification.Models;

namespace VoiceIdentification.Controllers
{
    public class response
    {
        public Guid identificationProfileId { get; set; }
        public Guid verificationProfileId { get; set; }
    }   
    public class AccessController : Controller
    {
        private VoiceIdentificationEntities db = new VoiceIdentificationEntities();
        string Baseurl = "https://westus.api.cognitive.microsoft.com/";
        Guid identificationProfileId;
        Guid verificationProfileId;
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "Id,FirstName,LastName,PhoneNumber,EmailId,IdentificationId,VerificationId,IsActive")]Activation activation)
        {
            if (ModelState.IsValid)
            {
                db.Activations.Add(activation);
                db.SaveChanges();
               var data= db.Activations.Where(x => x.EmailId == activation.EmailId).FirstOrDefault();
                ViewBag.Id = data.Id;
                return View();
            }
            return View(activation);
        }
        public JsonResult CheckExistingEmail(Activation activation)
        {
           
            bool ifEmailExist = false;

            try

            {
                int count = db.Activations.Where(x => x.EmailId == activation.EmailId).Count();
                // ifEmailExist = db.Activations.Where(x=>x.EmailId == activation.EmailId) ? true : false;
                if (count>0)
                {
                    ifEmailExist = true;
                return Json(!ifEmailExist, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    ifEmailExist = false;
                    return Json(!ifEmailExist, JsonRequestBehavior.AllowGet);
                }

            }

            catch (Exception ex)

            {

                return Json(false, JsonRequestBehavior.AllowGet);

            }
        }
        public JsonResult RecordIdentificationIdTodb(Activation activation)
        {
            try
            {
                Guid idfid = (Guid)activation.IdentificationId;
                var act = db.Activations.Where(x => x.Id == activation.Id).FirstOrDefault();
                act.EmailId = activation.EmailId;
                act.Id = activation.Id;
                act.PhoneNumber = activation.PhoneNumber;
                act.FirstName = activation.FirstName;
                act.LastName = activation.LastName;
                act.IdentificationId = idfid;
                db.Entry(act).State = EntityState.Modified;
                db.SaveChanges();
                return Json(true);

            }
            catch(Exception ex)
            {
                return Json(false);

            }

        }
        public async Task<ActionResult> CreateIdentificationProfile(int Id)
        {

            using (var client = new HttpClient())
            {
                HttpClient client1 = new HttpClient();
                client1.BaseAddress = new Uri(Baseurl);
                client1.DefaultRequestHeaders
                      .Add("Ocp-Apim-Subscription-Key", "f9ed8e1a04d34e80af8105269d9f91fd");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Baseurl + "spid/v1.0/identificationProfiles");
                request.Content = new StringContent("{\"locale\":\"en-us\"}",
                                                        Encoding.UTF8,
                                                    "application/json");
                HttpResponseMessage response = await client1.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();
                JavaScriptSerializer js = new JavaScriptSerializer();
                response blogObject = js.Deserialize<response>(responseContent);
                identificationProfileId = blogObject.identificationProfileId;
                TempData["identificationProfileId"] = identificationProfileId;
                TempData.Keep("identificationProfileId");
                Activation acc = new Activation();
                acc = db.Activations.Find(Id);
                //acc.Id = Id;
                acc.IdentificationId = identificationProfileId;
                db.Entry(acc).State = EntityState.Modified;
                db.SaveChanges();
               // return "Congrats : - Profile Has been created successfully. Profile Id is :- " + identificationProfileId.ToString() + " Now, Trying to go further";
                return RedirectToAction("Register");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
