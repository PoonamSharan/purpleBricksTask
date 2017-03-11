using System.Collections;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using OrangeBricks.Web.Attributes;
using OrangeBricks.Web.Controllers.Property.Builders;
using OrangeBricks.Web.Controllers.Property.Commands;

//loading namespace for ado.net connectivity
using System.Data;
using System.Data.SqlClient;

using OrangeBricks.Web.Controllers.Property.ViewModels;
using OrangeBricks.Web.Models;
using OrangeBricks.Web.Controllers.Offers.Commands;

namespace OrangeBricks.Web.Controllers.Property
{
    public class PropertyController : Controller
    {
        private readonly IOrangeBricksContext _context;


        //sql connection 
        SqlConnection con = new SqlConnection(@"Data Source=(LocalDb)\v11.0;AttachDbFilename=C:\Users\user\Desktop\developer-test-master\OrangeBricks.Web\App_Data\aspnet-OrangeBricks.Web-20150505090900.mdf;Initial Catalog=aspnet-OrangeBricks.Web-20150505090900;Integrated Security=True");

        public PropertyController(IOrangeBricksContext context)
        {
            _context = context;
        }

        [Authorize]
        public ActionResult Index(PropertiesQuery query)
        {
            var builder = new PropertiesViewModelBuilder(_context);
            var viewModel = builder.Build(query);

            //now from here  we need to get status of property , which has been accepted by seller 
            //that status , we need to forward to buyers to see that it has been accpeted.

            
            string uname = User.Identity.Name;
            // we will get the user name
            string qry="select id  from AspNetUsers where Email='"+uname+"'";

            SqlDataAdapter adpt = new SqlDataAdapter(qry, con);
            DataSet ds = new DataSet();
            adpt.Fill(ds);

            string uid = ds.Tables[0].Rows[0]["id"].ToString();

            string qry1="select RoleId  from AspNetUserRoles where UserId='"+ uid + "'";
            // acording to userid we have got role id ao we know its seller or buyer.
            SqlDataAdapter adpt1 = new SqlDataAdapter(qry1, con);
            DataSet ds1 = new DataSet();
            adpt1.Fill(ds1);
            string RID = ds1.Tables[0].Rows[0]["RoleId"].ToString();
            string qry2="select name from AspNetRoles where Id='"+ RID + "'";

            SqlDataAdapter adpt2 = new SqlDataAdapter(qry2, con);
            DataSet ds2 = new DataSet();
            adpt2.Fill(ds2);
            string rolename = ds2.Tables[0].Rows[0]["name"].ToString();
       // we have got role name and then checked role name is buyer then 
            if(rolename== "Buyer")
            {

                string qry3 = "select  Properties.id from offers, Properties   where offers.Property_id=Properties.id and Status=1";
                SqlDataAdapter adpt3 = new SqlDataAdapter(qry3,con);
                DataSet ds3 = new DataSet();
                adpt3.Fill(ds3);

                Session["ds"] = ds3;




                // here we have done coding for whatever seller has accpeted that  offer made by buyr's and buyer can see that offer as soon 
                //as soon he will see his property with message that offer has been accpeted. after that coding n Index view page of property view folder.

            }






        






            return View(viewModel);
        }

        [OrangeBricksAuthorize(Roles = "Seller")]
        public ActionResult Create()
        {
            var viewModel = new CreatePropertyViewModel();

            viewModel.PossiblePropertyTypes = new string[] { "House", "Flat", "Bungalow" }
                .Select(x => new SelectListItem { Value = x, Text = x })
                .AsEnumerable();

            return View(viewModel);
        }

        [OrangeBricksAuthorize(Roles = "Seller")]
        [HttpPost]
        public ActionResult Create(CreatePropertyCommand command)
        {
            var handler = new CreatePropertyCommandHandler(_context);

            command.SellerUserId = User.Identity.GetUserId();

            handler.Handle(command);

            return RedirectToAction("MyProperties");
        }

        [OrangeBricksAuthorize(Roles = "Seller")]
        public ActionResult MyProperties()
        {
            var builder = new MyPropertiesViewModelBuilder(_context);
            var viewModel = builder.Build(User.Identity.GetUserId());

            return View(viewModel);
        }

        [HttpPost]
        [OrangeBricksAuthorize(Roles = "Seller")]
        public ActionResult Accept(AcceptOfferCommand command)
        {
            var handler = new AcceptOfferCommandHandler(_context);

            handler.Handle(command);

            return RedirectToAction("MyProperties");
        }





        [HttpPost]
        [OrangeBricksAuthorize(Roles = "Seller")]
        public ActionResult ListForSale(ListPropertyCommand command)
        {
            var handler = new ListPropertyCommandHandler(_context);

            handler.Handle(command);

            return RedirectToAction("MyProperties");
        }

        [OrangeBricksAuthorize(Roles = "Buyer")]
        public ActionResult MakeOffer(int id)
        {
            var builder = new MakeOfferViewModelBuilder(_context);
            var viewModel = builder.Build(id);
            return View(viewModel);
        }

        [HttpPost]
        [OrangeBricksAuthorize(Roles = "Buyer")]
        public ActionResult MakeOffer(MakeOfferCommand command)
        {
            var handler = new MakeOfferCommandHandler(_context);

            handler.Handle(command);

            return RedirectToAction("Index");
        }
    }
}