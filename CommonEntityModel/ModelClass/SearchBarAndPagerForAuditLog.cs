using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonEntityModel.ModelClass
{
    public class SearchBarAndPagerForAuditLog
    {
        public string Controller { get; set; }
        public string Action { get; set; }
        public string ControllerNameTxt { get; set; }
        public string ActionNameTxt { get; set; }
        public string PreInfoTxt { get; set; }
        public string UpdatedInfoNameTxt { get; set; }
        public string AuditByTxt { get; set; }

        public int TotalItems { get; private set; }
        public int CurrentPage { get; private set; }
        public int PageSize { get; private set; }

        public int TotalPages { get; private set; }
        public int StartPage { get; private set; }
        public int EndPage { get; private set; }

        public SearchBarAndPagerForAuditLog()
        {

        }

        public SearchBarAndPagerForAuditLog(int totalItems, int page, int pageSize = 5)
        {
            int totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)pageSize);
            int currentPage = page;

            int startPage = currentPage - 5;
            int endPage = currentPage + 4;

            if (startPage <= 0)
            {
                endPage = endPage - (startPage - 1);
                startPage = 1;
            }

            if (endPage > totalPages)
            {
                endPage = totalPages;
                if (endPage > 10)
                {
                    startPage = endPage - 9;
                }
            }

            TotalItems = totalItems;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = totalPages;
            StartPage = startPage;
            EndPage = endPage;

        }
    }
}
