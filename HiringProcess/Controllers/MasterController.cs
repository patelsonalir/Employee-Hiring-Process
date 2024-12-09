using HiringProcess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace HiringProcess.Controllers
{
    public class MasterController : Controller
    {

        public static int _pageSize;
        public static int totalData = 0;

        public MasterController()
        {
            _pageSize = 25;
        }

        public ActionResult LoadPagination(int pageNumber, int pageSize)
        {
            _pageSize = pageSize;
            PaginationModel pagination = new PaginationModel();

            pagination.Pages = new List<int>();
            pagination.ShownPages = 3;
            pagination.CurrentPage = pageNumber;
            pagination.TotalRecord = totalData;
            pagination.TotalPage = (int)Math.Ceiling((double)totalData / _pageSize);
            pagination.PreviousPage = 1;
            pagination.Nextpage = pagination.TotalPage;
            if (pageNumber - 1 > 0)
            {
                pagination.PreviousPage = pageNumber - 1;
            }
            if (pageNumber + 1 < pagination.TotalPage)
            {
                pagination.Nextpage = pageNumber + 1;
            }

            pagination.Pages = GetPaginationInfo(pagination.TotalRecord, _pageSize, pageNumber, 2).ToList();
            pagination.PageSize = _pageSize;

            var dat = PartialView("~/Views/Shared/_Pagination.cshtml", pagination);
            return dat;
        }

        private IEnumerable<int> GetPaginationInfo(int total, int itemsPerPage, int currentPage, int rangeBeforeAndAfter = 2)
        {
            int minPage = 0;
            int maxPage = ((int)Math.Ceiling((double)(((double)total) / ((double)itemsPerPage)))) - 1;
            int beginPage = currentPage - rangeBeforeAndAfter;
            int endPage = currentPage + rangeBeforeAndAfter;
            if (beginPage < minPage)
            {
                endPage += -beginPage;
                beginPage = minPage;
            }
            if (endPage > maxPage)
            {
                beginPage -= endPage - maxPage;
                if (beginPage < minPage)
                {
                    beginPage = minPage;
                }
                endPage = maxPage;
            }
            return Enumerable.Range(beginPage + 1, (endPage + 1) - beginPage);
        }

        public RequestModel GetUserDetails()
        {
            return new RequestModel()
            {
                UserId = Convert.ToInt32(User.Claims.FirstOrDefault(C => C.Type == "userid").Value),
                UserName = User.Claims.FirstOrDefault(C => C.Type == "username").Value,
                UserDisplayName = User.Claims.FirstOrDefault(C => C.Type == "userdisplayname").Value,
                UserRoleId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "roleid").Value),
                UserRoleName = User.Claims.FirstOrDefault(c => c.Type == "rolename").Value,
                Terminal = User.Claims.FirstOrDefault(C => C.Type == "terminal").Value
            };
        }


        public EntryDetailModel GetEntryDetails()
        {
            string entTerminal = User.Claims.FirstOrDefault(c => c.Type == "terminal").Value;
            string entUser = User.Claims.FirstOrDefault(c => c.Type == "username").Value;

            return new EntryDetailModel()
            {
                EntryUserName = entUser,
                EntryTerminal = entTerminal,
                EntryDatetime = DateTime.Now
            };

        }
    }
}
