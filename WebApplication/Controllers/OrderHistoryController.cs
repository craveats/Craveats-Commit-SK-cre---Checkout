using Generic.Obfuscation.TripleDES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication.DAL;
using WebApplication.Models.ViewModel;

namespace WebApplication.Controllers
{
    [Authorize, Tls]
    public class OrderHistoryController : Controller
    {
        private CraveatsDbContext db = new CraveatsDbContext();

        // GET: OrderHistory
        public ActionResult Index(string id)
        {
            var viewModel = new OrderOrderDetailIndexData();

            int userId = int.Parse(
                    DataSecurityTripleDES.GetPlainText(
                        SessionManager.GetContextSessionLoggedUserID()));

            List<Order> userOrders = db.Order.Where(u =>
                u.UserId == userId).OrderByDescending(
                    u => u.LastUpdated ?? u.DateCreated).ToList();

            List<OrderHistoryDTO> orderHistoryDTOs = new List<OrderHistoryDTO>();

            foreach (Order anOrder in userOrders)
            {
                orderHistoryDTOs.Add(EntityDTOHelper.GetEntityDTO<Order, OrderHistoryDTO>(anOrder));
            }
            viewModel.Orders = orderHistoryDTOs;

            if (id != null)
            {
                int? anOrderId = (int?)int.Parse(
                    DataSecurityTripleDES.GetPlainText(id));

                List<OrderDetail> userOrderDetails = db.OrderDetail.Where(u =>
                    u.OrderId == anOrderId).OrderBy(
                    u => u.Id).ToList();

                List<OrderDetailHistoryDTO> orderDetailHistoryDTO = new List<OrderDetailHistoryDTO>();
                foreach (OrderDetail anOrderDetail in userOrderDetails)
                {
                    orderDetailHistoryDTO.Add(EntityDTOHelper.GetEntityDTO<OrderDetail, OrderDetailHistoryDTO>(anOrderDetail));
                }

                ViewBag.OrderId = id;
                viewModel.OrderDetails = orderDetailHistoryDTO;
            }
            
            return View(viewModel);
        }
    }
}